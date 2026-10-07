using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [SerializeField]
    [Tooltip("どこまで遠くのプレイヤーを見つけられるか")]
    [Label("目の良さ（視力）")]
    private float _sightDistance = 10.0f;

    [SerializeField]
    [Tooltip("どこまで広くのプレイヤーを見つけられるか")]
    [Label("目の良さ（広さ）")]
    private float _sightRadius = 60.0f;
    [Tooltip("プレイヤーを見失ってから諦めるまでのじかん")]
    [Label("見失うまでの時間")]
    public float lostTime = 5.0f;

    // 視線のレイ密度（正面1本は最低として加えて何本か）
    [SerializeField]
    [Tooltip("視線を何本に分けて調べるか。多いほど見つけ漏れが減ります")]
    [Label("目の細かさ")]
    private int _sightDensity = 6;

    [SerializeField]
    [Tooltip("視線をさえぎるものがあるレイヤー。プレイヤーのレイヤーも含めてください")]
    [Label("視線がぶつかるもの")]
    private LayerMask _sightBlockers = ~0;

    public bool IsFoundPlayer { get; private set; }
    public bool IsCathcPlayer { get; private set; }

    [HideInInspector]
    public GameObject playerGameObject;
    // Start is called before the first frame update
    void Start()
    {
        IsFoundPlayer = false;
        IsCathcPlayer = false;
    }

    public void Exec()
    {
#if UNITY_EDITOR
        if (StealthGameManager.s_isNoCatchMode)
        {
            IsFoundPlayer = false;
            IsCathcPlayer = false;
            return;
        }
#endif
        SightUpdate();
        TouchUpdate();
    }

    void SightUpdate()
    {
        Quaternion offsetRotation = Quaternion.Euler(0, -0.5f * _sightRadius, 0); // Y軸を中心に30度回転する四元数
        Vector3 rayTargetVec = offsetRotation * transform.forward;
        float durationRadius = _sightRadius / _sightDensity;

        // 扇のうち1本でもプレイヤーに当たれば発見。途中で return すると
        // 「最後に調べたレイの結果」だけが残ってしまうので、全部調べてから1回だけ書き込む
        bool found = false;
        GameObject foundPlayer = null;

        for(int i = 0; i < _sightDensity + 1; i++)
        {
            var eyePos = transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            Ray ray = new Ray(eyePos, rayTargetVec);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _sightDistance, _sightBlockers))
            {
                if (hit.transform.gameObject.tag == "Player")
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
                    found = true;
                    foundPlayer = hit.transform.gameObject;
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * _sightDistance, Color.red);
            }

            Quaternion rotation = Quaternion.Euler(0, durationRadius, 0); // Y軸を中心に30度回転する四元数
            rayTargetVec = rotation * rayTargetVec;
        }

        IsFoundPlayer = found;
        if (found)
        {
            playerGameObject = foundPlayer;
        }
    }

    private void TouchUpdate()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
#if UNITY_EDITOR
            if (StealthGameManager.s_isNoCatchMode) return;
#endif
            IsCathcPlayer = true;
        }
    }

    // 離れたら降ろす。これが無いと一度かすっただけで触れた状態が残り続け、
    // あとから追跡状態に入った瞬間に問答無用で捕まることになる
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            IsCathcPlayer = false;
        }
    }
}
