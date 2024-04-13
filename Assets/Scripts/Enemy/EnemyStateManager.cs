using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EnemyType
{
    Patrol,
    Stand,
}

public class EnemyStateManager : MonoBehaviour
{
    [SerializeField]
    private GameObject movePointsParent;

    public EnemyType enemyType;
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
            case EnemyType.Patrol:
                EnemyStateChange(new EnemyStatePatrol());
                break;
            case EnemyType.Stand:
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
