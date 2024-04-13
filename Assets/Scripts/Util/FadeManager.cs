using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KanKikuchi.AudioManager;

public abstract class FadeManager<T> : SingletonMonoBehaviour<T> where T : MonoBehaviourWithInit
{
    protected override void Init()
    {
        base.Init();
    }
}   
