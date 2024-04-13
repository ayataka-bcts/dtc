using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatePatrol : EnemyState
{
    private GameObject _self;

    private Vector3[] patrolPositions;

    private Vector3 targetPos;
    private int targetPosIndex = 0;

    private float waitTimer = 0.0f;

    public EnemyStatePatrol()
    {
        //OnStateChange += OnChange;
    }

    public override void Init()
    {
        waitTimer = 0.0f;
        targetPosIndex = 0;
    }

    public override void Exec(EnemyPerception perception)
    {
        if(perception.IsFoundPlayer)
        {
            ChangeState(new EnemyStateChase());
        }

        float distance = Vector3.Distance(_self.transform.position, targetPos);
        if(distance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if(waitTimer > 2.0f)
            {
                UpdateTargetPos();
                waitTimer = 0.0f;
            }
        }
    }

    public override void OnChange(EnemyStateManager manager)
    {
        this.patrolPositions = manager.movePoints.ToArray() ;
        _self = manager.gameObject;

        UpdateTargetPos();
    }

    public override Vector3 GetTargetPos()
    {
        return targetPos;
    }

    private void UpdateTargetPos()
    {
        targetPosIndex++;
        targetPosIndex = targetPosIndex % patrolPositions.Length;
        targetPos = patrolPositions[targetPosIndex];
    }
}
