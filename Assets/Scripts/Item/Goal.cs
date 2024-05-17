using KanKikuchi.AudioManager;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public static event Action OnSuccessGame;

    [SerializeField]
    [Tooltip("プレイヤーがゴールしたときの音")]
    [Label("ゴール音")]
    private AudioClip goalAudioClip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            var tpc = collision.gameObject.GetComponent<ThirdPersonController>();
            if(tpc != null && tpc.hasTreasure)
            {
                OnSuccessGame?.Invoke();

                if(goalAudioClip != null)
                {
                    SEManager.Instance.Play(goalAudioClip);
                }
            }
        }
    }
}
