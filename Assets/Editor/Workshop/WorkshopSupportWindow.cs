using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// 制作サポート。安全装置だけを置く窓で、ゲームバランスの数値は扱わない。
    /// 数値は各オブジェクトの Inspector で見つけてもらう。
    /// </summary>
    public class WorkshopSupportWindow : EditorWindow
    {
        private Vector2 _scroll;
        private double _nextRepaintAt;
        private bool _showBackups = true;

        [MenuItem("ワークショップ/制作サポート")]
        public static void Open()
        {
            var window = GetWindow<WorkshopSupportWindow>("制作サポート");
            window.minSize = new Vector2(260f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            WorkshopAutoSave.Changed += Repaint;
        }

        private void OnDisable()
        {
            WorkshopAutoSave.Changed -= Repaint;
        }

        private void Update()
        {
            // 「N分前」を進めるためだけの再描画。毎フレームやる必要はない
            if (EditorApplication.timeSinceStartup < _nextRepaintAt)
            {
                return;
            }

            _nextRepaintAt = EditorApplication.timeSinceStartup + 1.0;
            Repaint();
        }

        private void OnGUI()
        {
            var config = WorkshopConfig.GetOrNull();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawProtectedWarning();
            DrawAutoSave(config);
            EditorGUILayout.Space(8f);
            DrawPlayFlow();
            EditorGUILayout.Space(8f);
            DrawBackups(config);
            EditorGUILayout.Space(8f);
            DrawSettingsLink(config);

            EditorGUILayout.EndScrollView();
        }

        //=====================================================================

        private void DrawProtectedWarning()
        {
            var dirty = WorkshopAutoSave.GetDirtyProtectedScenes();
            if (dirty.Count == 0)
            {
                return;
            }

            var names = string.Join(", ", dirty.ToArray());
            EditorGUILayout.HelpBox(
                names + " は原本なので保存できません。\n"
                + "このまま作業を続けても保存されません。Main を開いてください。",
                MessageType.Error);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("変更をすてて開き直す"))
                {
                    DiscardAndReopen();
                }

                if (GUILayout.Button("Main を開く"))
                {
                    OpenMain();
                }
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawAutoSave(WorkshopConfig config)
        {
            EditorGUILayout.LabelField("自動保存", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (config == null)
                {
                    EditorGUILayout.LabelField("設定がまだありません");
                    if (GUILayout.Button("設定を作る"))
                    {
                        WorkshopConfig.GetOrCreate();
                    }

                    return;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var enabled = EditorGUILayout.ToggleLeft(
                        "自動保存する（" + Mathf.RoundToInt(config.intervalSeconds) + "秒おき）",
                        config.autoSaveEnabled);
                    if (check.changed)
                    {
                        config.autoSaveEnabled = enabled;
                        EditorUtility.SetDirty(config);
                    }
                }

                EditorGUILayout.LabelField("最終保存", WorkshopAutoSave.DescribeAge(WorkshopAutoSave.LastSaveTime));

                var last = WorkshopAutoSave.LastResult;
                if (!string.IsNullOrEmpty(last))
                {
                    EditorGUILayout.HelpBox(last,
                        WorkshopAutoSave.LastResultIsProblem ? MessageType.Warning : MessageType.None);
                }

                if (GUILayout.Button("今すぐ保存", GUILayout.Height(24f)))
                {
                    WorkshopAutoSave.TrySave(SaveContext.Manual);
                }
            }
        }

        private void DrawPlayFlow()
        {
            EditorGUILayout.LabelField("テストプレイ", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string blocked;
                var ok = WorkshopPlayFlow.CanTouchActiveScene(out blocked);

                using (new EditorGUI.DisabledScope(!ok))
                {
                    if (GUILayout.Button("保存して テストプレイ", GUILayout.Height(32f)))
                    {
                        WorkshopPlayFlow.SaveBakeAndPlay();
                    }

                    EditorGUILayout.LabelField("保存 → NavMesh更新 → Play", EditorStyles.miniLabel);

                    if (GUILayout.Button("NavMesh だけ更新"))
                    {
                        WorkshopPlayFlow.UpdateNavMesh();
                    }
                }

                if (!ok)
                {
                    EditorGUILayout.HelpBox(blocked, MessageType.Info);
                }
                else if (!string.IsNullOrEmpty(WorkshopPlayFlow.LastResult))
                {
                    EditorGUILayout.HelpBox(WorkshopPlayFlow.LastResult,
                        WorkshopPlayFlow.LastResultIsProblem ? MessageType.Warning : MessageType.None);
                }
            }
        }

        private void DrawBackups(WorkshopConfig config)
        {
            _showBackups = EditorGUILayout.Foldout(_showBackups, "もとにもどす", true, EditorStyles.foldoutHeader);
            if (!_showBackups)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var scene = EditorSceneManager.GetActiveScene();
                if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                {
                    EditorGUILayout.LabelField("シーンが保存されていません");
                    return;
                }

                var entries = WorkshopBackup.List(scene.name);
                if (entries.Count == 0)
                {
                    EditorGUILayout.LabelField(scene.name + " のバックアップはまだありません");
                }
                else
                {
                    EditorGUILayout.LabelField(scene.name + " の保存履歴（新しい順）", EditorStyles.miniLabel);

                    var shown = Mathf.Min(entries.Count, 10);
                    for (int i = 0; i < shown; i++)
                    {
                        var entry = entries[i];
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(entry.TakenAt.ToString("MM/dd HH:mm:ss"));
                            if (GUILayout.Button("ここにもどす", GUILayout.Width(96f)))
                            {
                                WorkshopBackup.Restore(entry, scene.path);
                                GUIUtility.ExitGUI();
                            }
                        }
                    }

                    if (entries.Count > shown)
                    {
                        EditorGUILayout.LabelField("ほか " + (entries.Count - shown) + " 件", EditorStyles.miniLabel);
                    }
                }

                if (GUILayout.Button("バックアップのフォルダを開く"))
                {
                    WorkshopBackup.RevealInFinder();
                }
            }
        }

        private void DrawSettingsLink(WorkshopConfig config)
        {
            if (config == null)
            {
                return;
            }

            if (GUILayout.Button("くわしい設定をひらく"))
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }

        //=====================================================================

        private static void OpenMain()
        {
            const string path = "Assets/Scenes/Main.unity";
            if (System.IO.File.Exists(path))
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            }
            else
            {
                EditorUtility.DisplayDialog("Main を開く", path + " が見つかりません。", "とじる");
            }
        }

        private static void DiscardAndReopen()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "変更をすてる",
                    scene.name + " の変更をすべて捨てて、保存されている状態で開き直します。\n\nよろしいですか？",
                    "すてて開き直す", "やめる"))
            {
                return;
            }

            // ディスク上の状態で開き直す。原本は保存していないので、ここで捨てるのは
            // 原本に対する未保存の変更だけ
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
        }
    }
}
