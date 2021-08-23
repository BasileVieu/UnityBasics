using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public Transform hoursTransform;
    public Transform minutesTransform;
    public Transform secondsTransform;

    public bool continuous;

    private const float degreesPerHour = 30;
    private const float degreesPerMinute = 6;
    private const float degreesPerSecond = 6;

    void Update()
    {
        if (continuous)
        {
            UpdateContinuous();
        }
        else
        {
            UpdateDiscrete();
        }
    }

    void UpdateContinuous()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursTransform.rotation = Quaternion.Euler(0f, (float)time.TotalHours * degreesPerHour, 0f);
        minutesTransform.rotation = Quaternion.Euler(0f, (float)time.TotalMinutes * degreesPerMinute, 0f);
        secondsTransform.rotation = Quaternion.Euler(0f, (float)time.TotalSeconds * degreesPerSecond, 0f);
    }

    void UpdateDiscrete()
    {
        DateTime time = DateTime.Now;
        hoursTransform.rotation = Quaternion.Euler(0f, time.Hour * degreesPerHour, 0f);
        minutesTransform.rotation = Quaternion.Euler(0f, time.Minute * degreesPerMinute, 0f);
        secondsTransform.rotation = Quaternion.Euler(0f, time.Second * degreesPerSecond, 0f);
    }
}