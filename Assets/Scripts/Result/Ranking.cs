using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranking
{
    private string[] rankingKeys = { "FirstTime", "SecondTime", "ThirdTime"};
    private float[] rankingTimes;

    private float currentTime = 0.0f;

    // Start is called before the first frame update
    public Ranking(float currentScore)
    {
        currentTime = currentScore;
        rankingTimes = new float[rankingKeys.Length];

        for (int i = 0; i < rankingKeys.Length; i++)
        {
            rankingTimes[i] = PlayerPrefs.GetFloat(rankingKeys[i]);
            if (rankingTimes[i] == 0.0f)
            {
                rankingTimes[i] = 999.99f;
            }
        }

        RankingUpdate();
    }

    ~Ranking ()
    {
    }

    public void SaveRankingData()
    {
        for (int i = 0; i < rankingKeys.Length; i++)
        {
            PlayerPrefs.SetFloat(rankingKeys[i], rankingTimes[i]);
        }
    }

    private bool RankingUpdate()
    {
        bool isUpdate = false;
        float time = currentTime;
        for (int i = 0; i < rankingTimes.Length; i++)
        {
            if (time < rankingTimes[i])
            {
                isUpdate = true;

                float tempTime = rankingTimes[i];
                rankingTimes[i] = time;
                time = tempTime;
            }
        }

        return isUpdate;
    }

    public float[] GetRankingTime()
    {
        return rankingTimes;
    }

}
