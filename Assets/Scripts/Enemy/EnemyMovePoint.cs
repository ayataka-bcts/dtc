using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public class EnemyMovePoint : MonoBehaviour
{
    [SerializeField]
    private Vector3 _position;

    [SerializeField]
    private Vector3 _rotation;

    public Vector3 Position { get { return _position; } set { _position = value; } }
    public Quaternion Rotation { get { return Quaternion.Euler(_rotation); } set { _rotation = value.eulerAngles; } }
}

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