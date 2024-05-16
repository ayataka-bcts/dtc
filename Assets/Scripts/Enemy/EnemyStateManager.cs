using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EnemyType
{
    パトロール,
    スタンド,
}

public class EnemyStateManager : MonoBehaviour
{
    [Tooltip("スタンド：その場できょろきょろ　パトロール：行ったり来たりうろうろ")]
    [Label("敵の種類")]
    public EnemyType enemyType;

    [SerializeField]
    private GameObject movePointsParent;

    public List<Vector3> movePoints;
    public EnemyState currentState;

    private EnemyPerception _enemyPerception;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < movePointsParent.transform.childCount; i++)
        {
            var point = movePointsParent.transform.GetChild(i);
            point.gameObject.SetActive(false);
            var pos = point.position;
            pos.y = 0.0f;
            movePoints.Add(pos);
        }

        _enemyPerception = GetComponent<EnemyPerception>();

        switch (enemyType)
        {
            case EnemyType.パトロール:
                EnemyStateChange(new EnemyStatePatrol());
                break;
            case EnemyType.スタンド:
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
