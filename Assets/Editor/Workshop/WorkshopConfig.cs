using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Workshop
{
    /// <summary>
    /// ワークショップ用ツールの設定。Assets/Editor 配下に置いてあるので
    /// ビルドには持ち込まれない。
    /// </summary>
    public class WorkshopConfig : ScriptableObject
    {
        public const string AssetPath = "Assets/Editor/Workshop/WorkshopConfig.asset";

        [Header("自動保存")]
        [Tooltip("OFF にすると自動保存を止めます。ワークショップ中は ON のままにしてください")]
        [Label("自動保存する")]
        public bool autoSaveEnabled = true;

        [Tooltip("この秒数ごとに、変更があれば保存します")]
        [Label("保存の間隔（秒）")]
        [Range(30f, 600f)]
        public float intervalSeconds = 120f;

        [Tooltip("Play を押す直前に保存します。Unity が落ちたときに作業を失わないための最後の砦です")]
        [Label("テストプレイ前に保存する")]
        public bool saveBeforePlay = true;

        [Tooltip("スクリプトの再読み込み直前に保存します")]
        [Label("コンパイル前に保存する")]
        public bool saveBeforeAssemblyReload = true;

        [Tooltip("マテリアルの色など、シーン以外の変更も保存します")]
        [Label("シーン以外の変更も保存する")]
        public bool saveDirtyAssets = true;

        [Header("原本の保護")]
        [Tooltip("ここに書いたシーンは自動保存の対象外になり、NavMesh の更新もできません")]
        [Label("保存しないシーン")]
        public List<string> protectedScenePaths = new List<string>
        {
            "Assets/Scenes/Main_old.unity",
            "Assets/Scenes/Sim_01.unity",
            "Assets/Scenes/Title.unity",
            "Assets/Scenes/Result.unity",
            "Assets/Scenes/Failure.unity",
        };

        [Header("もとにもどす（世代バックアップ）")]
        [Tooltip("保存のたびにシーンのコピーを残します。これが無いと、消してしまった状態も自動保存で確定してしまいます")]
        [Label("バックアップを残す")]
        public bool backupEnabled = true;

        [Tooltip("シーンごとに残す世代の数。古いものから消えます")]
        [Label("残す世代の数")]
        [Range(5, 100)]
        public int backupKeepCount = 20;

        private static WorkshopConfig _cached;

        /// <summary>
        /// 設定アセットを取得する。無ければ作る。
        /// アセットの生成はインポート中にやると危ないので、呼び出し側で
        /// isUpdating / isCompiling を弾いたあとに呼ぶこと。
        /// </summary>
        public static WorkshopConfig GetOrCreate()
        {
            if (_cached != null)
            {
                return _cached;
            }

            _cached = AssetDatabase.LoadAssetAtPath<WorkshopConfig>(AssetPath);
            if (_cached != null)
            {
                return _cached;
            }

            var dir = Path.GetDirectoryName(AssetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            _cached = CreateInstance<WorkshopConfig>();
            AssetDatabase.CreateAsset(_cached, AssetPath);
            AssetDatabase.SaveAssets();
            return _cached;
        }

        /// <summary>
        /// 読み込むだけ。無ければ null。アセットを作りたくない場面で使う。
        /// </summary>
        public static WorkshopConfig GetOrNull()
        {
            if (_cached != null)
            {
                return _cached;
            }

            _cached = AssetDatabase.LoadAssetAtPath<WorkshopConfig>(AssetPath);
            return _cached;
        }

        public bool IsProtected(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return false;
            }

            var normalized = scenePath.Replace('\\', '/');
            foreach (var p in protectedScenePaths)
            {
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }

                if (string.Equals(p.Replace('\\', '/'), normalized, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
