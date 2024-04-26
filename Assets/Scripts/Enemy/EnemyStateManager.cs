using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EnemyType
{
    �p�g���[��,
    �X�^���h,
}

public class EnemyStateManager : MonoBehaviour
{
    [Tooltip("�X�^���h�F���̏�ł���낫���@�p�g���[���F�s�����藈���肤�낤��")]
    [Label("�G�̎��")]
    public EnemyType enemyType;

    [SerializeField]
    private GameObject movePointsParent;

    public List<Vector3> movePoints;
    public EnemyState currentState;

    private EnemyPerception _enemyPerception;

    private void Awake()
    {
        for (int i = 0; i < movePointsParent.transform.childCount; i++)
        {
            movePoints.Add(movePointsParent.transform.GetChild(i).position);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _enemyPerception = GetComponent<EnemyPerception>();

        switch (enemyType)
        {
            case EnemyType.�p�g���[��:
                EnemyStateChange(new EnemyStatePatrol());
                break;
            case EnemyType.�X�^���h:
                EnemyStateChange(new EnemyStateStand());
                break;
            default:
                break;
        }
    }

    public void Exec()
    {
        if (_enemyPerception != null)
        {
            _enemyPerception.Exec();
        }

        if (currentState != null)
        {
            currentState.Exec(_enemyPerception);
        }

        Debug.Log(currentState.ToString());
    }

    public void EnemyStateChange(EnemyState state)
    {
        currentState = state;
        currentState.OnStateChange += EnemyStateChange;
        currentState.OnChange(this);
    }

    public Vector3 GetTargetPos()
    {
        Vector3 target = Vector3.one;

        target = currentState.GetTargetPos();

        return target;
    }

    public bool IsChase()
    {
        return (currentState.GetType() == typeof(EnemyStateChase));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GUI.color = Color.black;

        int pointCount = 1;
        foreach (var pos in movePoints)
        {
            Handles.Label(pos, "waypoint_" + pointCount++);
        }
    }
#endif 
}
