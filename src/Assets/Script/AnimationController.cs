using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    // Start is called before the first frame update
    const float DELTA_TIME_MAX = 1.0f;
    int _time = 0;
    float _inv_time_max = 1.0f;
  public void Set(int max_time)
    {
        Debug.Assert(0<max_time);
        _time = max_time;
        _inv_time_max = 1.0f / (float)max_time;
    }
    // Update is called once per frame
    public bool Update()
    {
        _time = Math.Max(--_time, 0);
        return (0< _time);
    }
    public float GetNormalized()
    {
        return   _inv_time_max*(float)_time;
    }
}
