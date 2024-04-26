using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure : MonoBehaviour
{

    [SerializeField]
    [Tooltip("プレイヤーがお宝をゲットしたときの音")]
    [Label("ゲット音")]
    private AudioClip getAudioClip;

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
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        
    }
}
