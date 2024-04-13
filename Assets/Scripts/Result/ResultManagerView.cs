using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultManagerView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timerText;
    [SerializeField]
    private TextMeshProUGUI[] _rankingTexts;

    private ResultManager _manager;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GetComponent<ResultManager>();
    }

    // Update is called once per frame
    void Update()
    {
        _timerText.text = _manager.currentScore.ToString("00:00.00");

        for(int i = 0; i < _rankingTexts.Length - 1; i++)
        {
            _rankingTexts[i].text = TimeUtil.ToTimeText(_manager.rankingScores[i]);
        }
    }
}
