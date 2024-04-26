using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyStateChase : EnemyState
{
    private GameObject _player;

    private float lostTimer = 0.0f;
    private float lostTime = 5.0f;

    public EnemyStateChase()
    {
        //OnStateChange += OnChange;
    }

    public override void Exec(EnemyPerception perception)
    {
        _player = perception.playerGameObject;
        lostTime = perception.lostTime;

        if (IsLostPlayer(perception.IsFoundPlayer))
        {
            ChangeState(new EnemyStateBack());
        }

        if(perception.IsCathcPlayer)
        {
            CatchPlayer();
        }
    }

    public override Vector3 GetTargetPos()
    {
        if(_player == null)
        {
            return base.GetTargetPos();
        }

        return _player.transform.position;
    }

    public override void Init()
    {
        lostTimer = 0.0f;
    }

    public override void OnChange(EnemyStateManager manager)
    {
    }

    private bool IsLostPlayer(bool isFoundPlayer)
    {
        if(isFoundPlayer)
        {
            lostTimer = 0.0f;
            return false;
        }

        if(lostTimer > lostTime) 
        {
            lostTimer = 0.0f;
            return true;
        }

        lostTimer += Time.deltaTime;
        return false;
    }
}
