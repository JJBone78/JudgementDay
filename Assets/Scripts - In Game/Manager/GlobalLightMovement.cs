using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GlobalLightMovement : MonoBehaviour
{
    private bool _cycle_completed = false;

    
    void Start()
    {
        iTween.RotateBy(gameObject, iTween.Hash("x", 1, "easeType", "linear", "loopType", "loop", "time", 360 / (1 * GUIManager._global_game_speed)));
    }

    void Update()
    {
        var Timer = 0.0;
        Timer += Time.deltaTime;
        Debug.Log(Timer);



        if (GUIManager._global_displayed_minute == 50)
            _cycle_completed = true;
        if (GUIManager._global_displayed_minute == 0 && _cycle_completed)
        {
            iTween.Stop(gameObject);
            iTween.RotateBy(gameObject, iTween.Hash("x", 0.0416, "easeType", "linear", "loopType", "none", "time", 11));
            _cycle_completed = false;
        }
    }
}
