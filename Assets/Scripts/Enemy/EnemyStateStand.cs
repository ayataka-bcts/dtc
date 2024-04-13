using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateStand : EnemyState
{
    private GameObject _self;

    [SerializeField]
    private const float turnAroundTime = 2.0f;

    private float turnAroundTimer = 0.0f;

    public EnemyStateStand()
    {
        //OnStateChange += OnChange;
    }

    public override void Init()
    {
        turnAroundTimer = 0.0f;
    }

    public override void Exec(EnemyPerception perception)
    {
        if(turnAroundTimer > turnAroundTime)
        {
            TurnAround();
            turnAroundTimer = 0.0f;
        }

        if (perception.IsFoundPlayer)
        {
            ChangeState(new EnemyStateChase());
        }

        turnAroundTimer += Time.deltaTime;
    }

    public override void OnChange(EnemyStateManager manager)
    {
        _self = manager.gameObject;
        manager.movePoints.Insert(0, _self.transform.position);
    }

    public override Vector3 GetTargetPos()
    {
        return _self.transform.position;
    }

    private void TurnAround() 
    {
        _self.transform.rotation *= Quaternion.Euler(0.0f, 90.0f, 0.0f);
    }
}
