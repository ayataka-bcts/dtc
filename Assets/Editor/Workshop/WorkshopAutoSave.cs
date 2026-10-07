using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Workshop
{
    /// <summary>
    /// どの流れで保存しようとしているか。タイミングによって外せるガードが違う。
    /// </summary>
    public enum SaveContext
    {
        /// <summary>一定間隔の自動保存。</summary>
        Auto,

        /// <summary>「今すぐ保存」ボタン。変更が無くても結果を UI に返す。</summary>
        Manual,

        /// <summary>Play 直前。isPlayingOrWillChangePlaymode が立っているので通す必要がある。</summary>
        BeforePlay,

        /// <summary>アセンブリ再読み込み直前。コンパイル中扱いになりうるので通す。</summary>
        BeforeReload,

        /// <summary>Unity 終了時。</summary>
        Quitting,
    }

    /// <summary>
    /// 一定間隔・Play 直前・コンパイル直前にシーンを保存し、世代バックアップを残す。
    /// Play 中は保存しない。原本シーンも保存しない。
    /// </summary>
    [InitializeOnLoad]
    public static class WorkshopAutoSave
    {
        private const string SessionKeyLastSave = "Workshop.LastSaveTicks";
        private const string SessionKeyLastResult = "Workshop.LastSaveResult";
        private const string SessionKeyLastFailed = "Workshop.LastSaveFailed";
        private const string PrefsKeyLastSave = "Workshop.LastSaveTicks.";

        private static double _nextCheckAt;

        /// <summary>直前の保存で何が起きたかを UI に出すための文字列。</summary>
        public static string LastResult
        {
            get { return SessionState.GetString(SessionKeyLastResult, string.Empty); }
            private set { SessionState.SetString(SessionKeyLastResult, value ?? string.Empty); }
        }

        /// <summary>直前の結果が失敗や拒否だったか。UI の色分けに使う。</summary>
        public static bool LastResultIsProblem
        {
            get { return SessionState.GetBool(SessionKeyLastFailed, false); }
            private set { SessionState.SetBool(SessionKeyLastFailed, value); }
        }

        /// <summary>UI の再描画用。</summary>
        public static event Action Changed;

        static WorkshopAutoSave()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnQuitting;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        //=====================================================================
        // 最終保存時刻
        //=====================================================================

        // EditorPrefs はマシン全体で共有なのでプロジェクトごとに分ける
        private static string PrefsKey
        {
            get { return PrefsKeyLastSave + Application.dataPath.GetHashCode().ToString("X8"); }
        }

        public static DateTime? LastSaveTime
        {
            get
            {
                // SessionState は domain reload をまたぐが Editor 再起動では消えるので、
                // 表示用に EditorPrefs にも書いておく
                var s = SessionState.GetString(SessionKeyLastSave, string.Empty);
                if (string.IsNullOrEmpty(s))
                {
                    s = EditorPrefs.GetString(PrefsKey, string.Empty);
                }

                long ticks;
                if (long.TryParse(s, out ticks))
                {
                    return new DateTime(ticks, DateTimeKind.Local);
                }

                return null;
            }
        }

        private static void StampNow()
        {
            var ticks = DateTime.Now.Ticks.ToString();
            SessionState.SetString(SessionKeyLastSave, ticks);
            EditorPrefs.SetString(PrefsKey, ticks);
        }

        //=====================================================================
        // フック
        //=====================================================================

        private static void OnEditorUpdate()
        {
            var now = EditorApplication.timeSinceStartup;
            if (now < _nextCheckAt)
            {
                return;
            }

            var config = WorkshopConfig.GetOrNull();
            var interval = config != null ? config.intervalSeconds : 120f;
            _nextCheckAt = now + Mathf.Max(5f, interval);

            if (config == null)
            {
                // 設定アセットの生成は、ここ（通常の Editor 更新中）だけでやる。
                // assembly reload 直前や終了時に AssetDatabase へ書き込むのは危ない
                string blocked;
                if (CanSave(SaveContext.Auto, out blocked))
                {
                    WorkshopConfig.GetOrCreate();
                }

                return;
            }

            if (!config.autoSaveEnabled)
            {
                return;
            }

            TrySave(SaveContext.Auto);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // ExitingEditMode はまだ保存できる最後のタイミング。
            // Scene Reload を切っている場合、ここがクラッシュとの唯一の防波堤になる
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            var config = WorkshopConfig.GetOrNull();
            if (config != null && !config.saveBeforePlay)
            {
                return;
            }

            TrySave(SaveContext.BeforePlay);
        }

        private static void OnBeforeAssemblyReload()
        {
            // 厳密には「コンパイル開始前」ではなく「リロード直前」。
            // Unity に開始前のフックは無いので、ここが実用上の限界。
            var config = WorkshopConfig.GetOrNull();
            if (config != null && !config.saveBeforeAssemblyReload)
            {
                return;
            }

            TrySave(SaveContext.BeforeReload);
        }

        private static void OnQuitting()
        {
            TrySave(SaveContext.Quitting);
        }

        //=====================================================================
        // 保存
        //=====================================================================

        public static bool TrySave(SaveContext context)
        {
            string blocked;
            if (!CanSave(context, out blocked))
            {
                if (context == SaveContext.Manual)
                {
                    Report("保存できません: " + blocked, true);
                }

                return false;
            }

            var config = context == SaveContext.Manual
                ? WorkshopConfig.GetOrCreate()
                : WorkshopConfig.GetOrNull();
            if (config == null)
            {
                return false;
            }

            var force = context == SaveContext.Manual;

            var targets = new List<Scene>();
            var protectedDirty = new List<string>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                {
                    // 一度も保存されていないシーン。保存を試みるとダイアログが開いて止まる
                    continue;
                }

                if (config.IsProtected(scene.path))
                {
                    if (scene.isDirty)
                    {
                        protectedDirty.Add(scene.name);
                    }

                    continue;
                }

                if (scene.isDirty || force)
                {
                    targets.Add(scene);
                }
            }

            // マテリアルの色なども保存したいが、「どれかが dirty か」を一発で聞く API が無い。
            // SaveAssets() は変更が無ければ何もしないので、毎回呼んで取りこぼしを防ぐ
            if (config.saveDirtyAssets)
            {
                AssetDatabase.SaveAssets();
            }

            if (targets.Count == 0)
            {
                if (protectedDirty.Count > 0)
                {
                    Report("原本シーンなので保存しませんでした: " + string.Join(", ", protectedDirty.ToArray()), true);
                    return false;
                }

                if (force)
                {
                    Report("シーンに変更はありません", false);
                }

                return false;
            }

            var saved = new List<string>();
            var failed = new List<string>();
            foreach (var scene in targets)
            {
                // 保存する前にディスク上の状態を控える。自動保存は「消してしまった状態」も
                // 確定させてしまうので、戻せる手段と一緒でなければ安全にならない
                if (config.backupEnabled)
                {
                    WorkshopBackup.Capture(scene.path, config.backupKeepCount);
                }

                if (EditorSceneManager.SaveScene(scene))
                {
                    saved.Add(scene.name);
                }
                else
                {
                    failed.Add(scene.name);
                }
            }

            StampNow();

            if (failed.Count > 0)
            {
                var msg = "保存に失敗しました: " + string.Join(", ", failed.ToArray());
                Report(msg, true);
                Debug.LogWarning("[ワークショップ] " + msg);
                return false;
            }

            var text = Describe(context) + ": " + string.Join(", ", saved.ToArray());
            if (protectedDirty.Count > 0)
            {
                text += "（原本のため除外: " + string.Join(", ", protectedDirty.ToArray()) + "）";
            }

            // 成功はログだけ。数分おきにモーダルが出るツールは必ず切られる
            Report(text, false);
            Debug.Log("[ワークショップ] " + text);
            return true;
        }

        /// <summary>
        /// 保存してよい状態か。ひとつでも欠けると、保存ダイアログが勝手に開いたり
        /// ビルド中に割り込んだりする。
        /// </summary>
        public static bool CanSave(SaveContext context, out string reason)
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                reason = "テストプレイ中です";
                return false;
            }

            // Play 直前（ExitingEditMode）では isPlayingOrWillChangePlaymode が立つ。
            // そこで弾くと「Play 前に保存」が永久に動かないので、この文脈だけ通す
            if (context != SaveContext.BeforePlay && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                reason = "テストプレイに入ろうとしています";
                return false;
            }

            // beforeAssemblyReload はコンパイル直後に呼ばれるため、ここも文脈で通す
            if (context != SaveContext.BeforeReload && EditorApplication.isCompiling)
            {
                reason = "スクリプトのコンパイル中です";
                return false;
            }

            if (EditorApplication.isUpdating)
            {
                reason = "アセットの読み込み中です";
                return false;
            }

            if (BuildPipeline.isBuildingPlayer)
            {
                reason = "ビルド中です";
                return false;
            }

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                // プレハブ編集中の黙った保存は別種のリスクなのでやらない
                reason = "プレハブを編集中です";
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>原本シーンが編集されていれば、その名前を返す。UI の警告帯用。</summary>
        public static List<string> GetDirtyProtectedScenes()
        {
            var result = new List<string>();
            var config = WorkshopConfig.GetOrNull();
            if (config == null)
            {
                return result;
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.IsValid() && scene.isDirty && config.IsProtected(scene.path))
                {
                    result.Add(scene.name);
                }
            }

            return result;
        }

        private static void Report(string text, bool isProblem)
        {
            LastResult = text;
            LastResultIsProblem = isProblem;
            if (Changed != null)
            {
                Changed();
            }
        }

        private static string Describe(SaveContext context)
        {
            switch (context)
            {
                case SaveContext.Manual: return "手動で保存";
                case SaveContext.BeforePlay: return "テストプレイ前に保存";
                case SaveContext.BeforeReload: return "コンパイル前に保存";
                case SaveContext.Quitting: return "終了時に保存";
                default: return "自動保存";
            }
        }

        public static string DescribeAge(DateTime? time)
        {
            if (!time.HasValue)
            {
                return "まだ保存していません";
            }

            var span = DateTime.Now - time.Value;
            var stamp = time.Value.ToString("HH:mm:ss");
            if (span.TotalSeconds < 10)
            {
                return stamp + "（たったいま）";
            }

            if (span.TotalMinutes < 1)
            {
                return stamp + "（" + ((int)span.TotalSeconds) + "秒前）";
            }

            if (span.TotalHours < 1)
            {
                return stamp + "（" + ((int)span.TotalMinutes) + "分前）";
            }

            return stamp + "（" + ((int)span.TotalHours) + "時間前）";
        }
    }
}
