using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        if (movePointsParent == null)
        {
            Debug.LogWarning("巡回地点の親オブジェクトが設定されていません。その場で立ち止まります。", this);
        }
        else
        {
            for (int i = 0; i < movePointsParent.transform.childCount; i++)
            {
                var point = movePointsParent.transform.GetChild(i);
                point.gameObject.SetActive(false);
                var pos = point.position;
                pos.y = 0.0f;
                movePoints.Add(pos);
            }
        }

        _enemyPerception = GetComponent<EnemyPerception>();

        // パトロールは巡回地点が1つ以上ないと成立しないので、無いときはスタンドに落とす
        var type = enemyType;
        if (type == EnemyType.パトロール && movePoints.Count == 0)
        {
            Debug.LogWarning("巡回地点が1つもないため、パトロールではなくスタンドで動きます。巡回地点を追加してください。", this);
            type = EnemyType.スタンド;
        }

        switch (type)
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
    }

    public void EnemyStateChange(EnemyState state)
    {
        currentState = state;
        currentState.OnStateChange += EnemyStateChange;
        currentState.OnChange(this);
    }

    public Vector3 GetTargetPos()
    {
        if (currentState == null)
        {
            return transform.position;
        }

        return currentState.GetTargetPos();
    }

    public bool IsChase()
    {
        return (currentState is EnemyStateChase);
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
