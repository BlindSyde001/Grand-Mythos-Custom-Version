using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameClock : MonoBehaviour
{
    public int playTime;
    internal int hour;
    internal int minute;
    internal int second;

    private void Start()
    {
        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            playTime += 1;
            second = playTime % 60;
            minute = playTime / 60 % 60;
            hour = playTime / 3600 % 24;
        }
    }
}
