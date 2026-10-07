using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// シーンファイルの世代バックアップ。
    ///
    /// 置き場所は Assets/ の外（プロジェクト直下の WorkshopBackups/）。
    /// Assets/ 配下に置くと Unity がシーンとしてインポートしてしまい、
    /// Project ウィンドウが「Main_20261007_143205」で埋まり、
    /// インポート時間も伸びる。
    /// </summary>
    public static class WorkshopBackup
    {
        public const string DirName = "WorkshopBackups";

        public struct Entry
        {
            public string FilePath;
            public string SceneName;
            public DateTime TakenAt;
        }

        public static string RootDir
        {
            get
            {
                // Application.dataPath は <project>/Assets なので、その親がプロジェクト直下
                var project = Directory.GetParent(Application.dataPath);
                return Path.Combine(project.FullName, DirName);
            }
        }

        /// <summary>
        /// 保存の直前に、ディスク上のシーンファイルをコピーして控える。
        /// </summary>
        public static void Capture(string scenePath, int keepCount)
        {
            try
            {
                var abs = ToAbsolute(scenePath);
                if (!File.Exists(abs))
                {
                    // まだ一度もディスクに書かれていないシーン。控える元が無い
                    return;
                }

                Directory.CreateDirectory(RootDir);

                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var dest = Path.Combine(RootDir, sceneName + "_" + stamp + ".unity");

                // 同じ秒に2回来た場合に上書きしない
                int suffix = 1;
                while (File.Exists(dest))
                {
                    dest = Path.Combine(RootDir, sceneName + "_" + stamp + "_" + suffix + ".unity");
                    suffix++;
                }

                File.Copy(abs, dest);
                Prune(sceneName, keepCount);
            }
            catch (Exception e)
            {
                // バックアップの失敗で保存自体を止めたくはない
                Debug.LogWarning("[ワークショップ] バックアップに失敗しました: " + e.Message);
            }
        }

        /// <summary>古い世代を捨てる。シーンごとに数える。</summary>
        private static void Prune(string sceneName, int keepCount)
        {
            if (keepCount <= 0)
            {
                return;
            }

            var all = List(sceneName);
            for (int i = keepCount; i < all.Count; i++)
            {
                try
                {
                    File.Delete(all[i].FilePath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[ワークショップ] 古いバックアップを消せませんでした: " + e.Message);
                }
            }
        }

        /// <summary>新しい順に返す。sceneName が null なら全シーン分。</summary>
        public static List<Entry> List(string sceneName)
        {
            var result = new List<Entry>();
            if (!Directory.Exists(RootDir))
            {
                return result;
            }

            foreach (var file in Directory.GetFiles(RootDir, "*.unity"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var m = NamePattern.Match(name);
                if (!m.Success)
                {
                    continue;
                }

                var head = m.Groups["name"].Value;
                if (sceneName != null && !string.Equals(head, sceneName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DateTime taken;
                if (!DateTime.TryParseExact(
                        m.Groups["d"].Value + "_" + m.Groups["t"].Value,
                        "yyyyMMdd_HHmmss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out taken))
                {
                    continue;
                }

                result.Add(new Entry { FilePath = file, SceneName = head, TakenAt = taken });
            }

            return result.OrderByDescending(e => e.TakenAt).ThenByDescending(e => e.FilePath).ToList();
        }

        // <SceneName>_yyyyMMdd_HHmmss[_n]。シーン名に _ が入っていても後ろから確定できる
        private static readonly System.Text.RegularExpressions.Regex NamePattern =
            new System.Text.RegularExpressions.Regex(
                @"^(?<name>.+)_(?<d>\d{8})_(?<t>\d{6})(?:_(?<n>\d+))?$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// バックアップでシーンを置き換える。
        /// 今の状態も必ず1件控えてから入れ替えるので、戻し間違いからも戻れる。
        /// </summary>
        public static bool Restore(Entry entry, string scenePath)
        {
            var abs = ToAbsolute(scenePath);

            var openScene = default(UnityEngine.SceneManagement.Scene);
            bool isOpen = false;
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var s = EditorSceneManager.GetSceneAt(i);
                if (s.IsValid() && s.path == scenePath)
                {
                    openScene = s;
                    isOpen = true;
                    break;
                }
            }

            var message = entry.SceneName + " を " + entry.TakenAt.ToString("HH:mm:ss") + " の状態に戻します。\n\n"
                          + "いまの状態は先にバックアップへ控えるので、戻しすぎた場合もやり直せます。";
            if (isOpen && openScene.isDirty)
            {
                message += "\n\n開いているシーンに未保存の変更があります。先に保存してから戻します。";
            }

            if (!EditorUtility.DisplayDialog("もとにもどす", message, "もどす", "やめる"))
            {
                return false;
            }

            try
            {
                // 未保存の変更があるなら先に書き出す。そうしないと「いまの状態」の控えが
                // 古いままになり、戻しすぎたときに失われる
                if (isOpen && openScene.isDirty)
                {
                    EditorSceneManager.SaveScene(openScene);
                }

                var config = WorkshopConfig.GetOrCreate();
                Capture(scenePath, config.backupKeepCount);

                File.Copy(entry.FilePath, abs, true);
                AssetDatabase.Refresh();

                if (isOpen)
                {
                    // ここまででファイルは clean なので、開き直しでダイアログは出ない
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }

                Debug.Log("[ワークショップ] " + entry.SceneName + " を " + entry.TakenAt.ToString("HH:mm:ss") + " の状態に戻しました");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[ワークショップ] もとにもどせませんでした: " + e.Message);
                EditorUtility.DisplayDialog("もとにもどす", "失敗しました。\n\n" + e.Message, "とじる");
                return false;
            }
        }

        public static void RevealInFinder()
        {
            Directory.CreateDirectory(RootDir);
            EditorUtility.RevealInFinder(RootDir);
        }

        private static string ToAbsolute(string assetPath)
        {
            var project = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(project, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
