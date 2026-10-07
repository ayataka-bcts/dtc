using UnityEngine;
using UnityEditor;

// 元は Scripts/Enemy/EnemyMovePoint.cs に同居していたが、Editor 専用クラスを
// ランタイムスクリプトに置くとプレイヤービルドが通らないためこちらへ移した。
[CustomEditor(typeof(EnemyMovePoint))]
public class EnemyMovePointEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        var script = target as EnemyMovePoint;

        var pos = script.transform.position + script.transform.localToWorldMatrix.MultiplyVector(script.transform.position);
        var rot = script.Rotation;

        EditorGUI.BeginChangeCheck();
        var newPos = Handles.PositionHandle(pos, rot);
        var posChanges = EditorGUI.EndChangeCheck();

        EditorGUI.BeginChangeCheck();
        var newRot = Handles.RotationHandle(rot, pos);
        var rotChanges = EditorGUI.EndChangeCheck();

        Handles.CubeHandleCap(0, pos, rot, 0.2f, EventType.MouseDown);

        if (posChanges || rotChanges)
        {
            if (posChanges)
            {
                script.transform.position = newPos;
            }
            if (rotChanges)
            {
                script.Rotation = newRot;
            }
        }
    }
}
