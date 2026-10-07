using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// 「保存 → NavMesh更新 → Play」を1手にまとめる。
    /// 手順書から「配置したらベイクする」「Ctrl+S で保存する」を消すのが目的。
    /// </summary>
    public static class WorkshopPlayFlow
    {
        /// <summary>直前の実行結果。UI 表示用。</summary>
        public static string LastResult { get; private set; }

        public static bool LastResultIsProblem { get; private set; }

        /// <summary>
        /// NavMesh を更新する。保護シーンでは何もしない。
        ///
        /// Main と Main_old が同じ NavMeshData を共有していた経緯があるため、
        /// 保護シーンでのベイク禁止は「あればいい機能」ではなく必須のガード。
        /// </summary>
        public static bool UpdateNavMesh()
        {
            string blocked;
            if (!CanTouchActiveScene(out blocked))
            {
                Report(blocked, true);
                return false;
            }

            // BuildNavMesh() は同期で全面ベイクする。非同期版を使うと完了前に
            // Play へ進む競合が生まれるのでこちらを使う。
            // ClearAllNavMeshes() は呼ばない。失敗したときに NavMesh が無い状態で
            // 残るほうが、古い NavMesh が残るより悪い
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

            // ベイクで NavMeshSettings の m_NavMeshData 参照が変わることがあるので、
            // もう一度保存しておく
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.IsValid() && !string.IsNullOrEmpty(scene.path) && scene.isDirty)
            {
                EditorSceneManager.SaveScene(scene);
            }

            Report("NavMesh を更新しました", false);
            Debug.Log("[ワークショップ] NavMesh を更新しました: " + scene.name);
            return true;
        }

        /// <summary>保存 → NavMesh更新 → Play。途中で失敗したら先へ進まない。</summary>
        public static void SaveBakeAndPlay()
        {
            string blocked;
            if (!CanTouchActiveScene(out blocked))
            {
                Report(blocked, true);
                return;
            }

            // 1. 保存。ここで失敗したまま Play に進むのが一番まずい
            if (!WorkshopAutoSave.TrySave(SaveContext.Manual))
            {
                // 変更が無くて false のこともあるので、保存できない状態かどうかで判断する
                string reason;
                if (!WorkshopAutoSave.CanSave(SaveContext.Manual, out reason))
                {
                    Report("保存できないので中止しました: " + reason, true);
                    return;
                }
            }

            // 2. NavMesh
            if (!UpdateNavMesh())
            {
                return;
            }

            // 3. Play
            Report("保存して NavMesh を更新しました。テストプレイを始めます", false);
            EditorApplication.EnterPlaymode();
        }

        /// <summary>
        /// いま開いているシーンに手を加えてよいか。
        /// </summary>
        public static bool CanTouchActiveScene(out string reason)
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                reason = "テストプレイ中はできません";
                return false;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                reason = "コンパイルか読み込みが終わるまで待ってください";
                return false;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                reason = "シーンがまだ保存されていません";
                return false;
            }

            var config = WorkshopConfig.GetOrNull();
            if (config != null && config.IsProtected(scene.path))
            {
                reason = scene.name + " は原本なので変更できません。Main を開いてください";
                return false;
            }

            reason = null;
            return true;
        }

        private static void Report(string text, bool isProblem)
        {
            LastResult = text;
            LastResultIsProblem = isProblem;
            if (isProblem)
            {
                Debug.LogWarning("[ワークショップ] " + text);
            }
        }
    }
}
