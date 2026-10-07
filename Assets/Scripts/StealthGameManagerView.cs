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

        if (_timerText == null)
        {
            Debug.LogWarning("タイム表示用のテキストが設定されていません。ゲームマネージャーの Timer Text を確認してください。", this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_timerText == null || _manager == null)
        {
            return;
        }

        _timerText.text = TimeUtil.ToTimeText(_manager.timer);
    }
}
