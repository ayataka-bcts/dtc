using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StealthGameManagerView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timerText;

    private StealthGameManager _manager;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GetComponent<StealthGameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _timerText.text = TimeUtil.ToTimeText(_manager.timer);
    }
}
