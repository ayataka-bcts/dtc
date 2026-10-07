using UnityEngine;
using System.Collections;

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
