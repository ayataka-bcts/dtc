using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultManagerView : MonoBehaviour
{
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
        for(int i = 0; i < _rankingTexts.Length - 1; i++)
        {
            _rankingTexts[i].text = TimeUtil.ToTimeText(_manager.rankingScores[i]);
        }

        _rankingTexts.Last().text = _manager.currentScore.ToString("00:00.00");
    }
}
