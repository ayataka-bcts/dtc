using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyStateBack : EnemyState
{
    private EnemyType _enemyState;
    private GameObject _self;
    private Vector3 targetPos;
    
    public EnemyStateBack()
    {
        //OnStateChange += OnChange;
    }

    public override void Exec(EnemyPerception perception)
    {
        if (perception.IsFoundPlayer)
        {
            ChangeState(new EnemyStateChase());
        }

        float distance = Vector3.Distance(_self.transform.position, targetPos);
        if (distance < 0.5f)
        {
            switch(_enemyState)
            {
                case EnemyType.スタンド:
                    ChangeState(new EnemyStateStand());
                    break;
                case EnemyType.パトロール:
                    ChangeState(new EnemyStatePatrol());
                    break;
                default:
                    break;
            }
        }
    }

    public override Vector3 GetTargetPos()
    {
        return targetPos;
    }

    public override void Init()
    {
        base.Init();
    }

    public override void OnChange(EnemyStateManager manager)
    {
        _enemyState = manager.enemyType;
        _self = manager.gameObject;

        var currentPos = _self.transform.position;
        targetPos = manager.movePoints.OrderBy(p => (currentPos - p).sqrMagnitude).FirstOrDefault();
    }
}
