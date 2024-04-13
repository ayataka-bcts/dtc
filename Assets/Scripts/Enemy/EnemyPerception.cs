using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class EnemyPerception : MonoBehaviour
{
    [SerializeField]
    private float _sightDistance = 10.0f;

    [SerializeField]
    private float _sightRadius = 60.0f;
    // �����̃��C���x�i����1�{�͍Œ�Ƃ��ĉ����ĉ��{���j
    [SerializeField]
    private int _sightDensity = 6;

    public bool IsFoundPlayer { get; private set; }
    public bool IsCathcPlayer { get; private set; }

    public GameObject playerGameObject;
    // Start is called before the first frame update
    void Start()
    {
        IsFoundPlayer = false;
        IsCathcPlayer = false;
    }

    public void Exec()
    {
        SightUpdate();
        TouchUpdate();
    }

    void SightUpdate()
    {
        Quaternion offsetRotation = Quaternion.Euler(0, -0.5f * _sightRadius, 0); // Y���𒆐S��30�x��]����l����
        Vector3 rayTargetVec = offsetRotation * transform.forward;
        float durationRadius = _sightRadius / _sightDensity;
        for(int i = 0; i < _sightDensity + 1; i++) 
        {
            var eyePos = transform.position + new Vector3(0.0f, 1.0f, 0.0f);
            Ray ray = new Ray(eyePos, rayTargetVec);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _sightDistance))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                if (hit.transform.gameObject.tag == "Player")
                {
                    IsFoundPlayer = true;
                    playerGameObject = hit.transform.gameObject;
                    return;
                }
            }
            else
            {
                Debug.DrawRay(ray.origin, ray.direction * _sightDistance, Color.red);
                IsFoundPlayer = false;
            }

            Quaternion rotation = Quaternion.Euler(0, durationRadius, 0); // Y���𒆐S��30�x��]����l����
            rayTargetVec = rotation * rayTargetVec;
        }
    }

    private void TouchUpdate()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            IsCathcPlayer = true;
        }
    }
}
