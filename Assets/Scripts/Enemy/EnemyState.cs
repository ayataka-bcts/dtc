using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState 
{
    public delegate void StateChangeHandler(EnemyState state);
    public event StateChangeHandler OnStateChange;

    public static event Action OnCatchPlayer;

    public EnemyState()
    {
        Init();
    }

    ~EnemyState()
    {

    }

    public virtual void Init() 
    { 
    }

    public virtual void Exec(EnemyPerception perception)
    { 
    }

    public virtual void OnChange(EnemyStateManager manager)
    {
    }

    public virtual Vector3 GetTargetPos()
    {
        return Vector3.zero;
    }

    protected void ChangeState(EnemyState state) 
    {
        OnStateChange?.Invoke(state);
    }

    protected void CatchPlayer()
    {
        OnCatchPlayer?.Invoke();
    }
}
