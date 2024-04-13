using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeUtil
{
    public static string ToTimeText(float time)
    {
        int minute = (int)(time / 60.0f);
        float seconds = time % 60;

        return minute.ToString("00") + ":" + seconds.ToString("00.00");
    }
}
