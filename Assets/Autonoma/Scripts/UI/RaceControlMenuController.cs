/* 
Copyright 2024 Purdue AI Racing

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at:

    http://www.apache.org/licenses/LICENSE-2.0

The software is provided "AS IS", WITHOUT WARRANTY OF ANY KIND, 
express or implied. In no event shall the authors or copyright 
holders be liable for any claim, damages or other liability, 
whether in action of contract, tort or otherwise, arising from, 
out of or in connection with the software or the use of the software.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Autonoma;

public class RaceControlMenuController : MonoBehaviour
{
    public List<GameObject> rosCars;
    public TMP_Dropdown trackFlagDropdown;     
    public TMP_Dropdown vehFlagDropdown;
    public TMP_Dropdown roundTargetSpeedDropdown;
    public int[] track_flag_vec = {3,9,1,37}; //red, full_course_yellow, green, waving_green (Marelli)
    public int[] veh_flag_vec = {0,25,7,34,4,33,36,35}; //none, orange, yellow, stop, black, engine_kill, attacker, defender
    public int[] round_target_speed_vec = {80,60,70,90,100,110,120,130,140,150,160,170,180}; //for use with waving green flag
    bool initialized = false;
    void Start()
    {
        trackFlagDropdown.onValueChanged.AddListener(delegate { trackFlagChanged(); } );

        trackFlagChanged();

        vehFlagDropdown.onValueChanged.AddListener(delegate { vehFlagChanged(); } );

        vehFlagChanged();

        roundTargetSpeedDropdown.onValueChanged.AddListener(delegate { roundTargetSpeedChanged(); } );

        roundTargetSpeedChanged();
    }

    void Update()
    {
        if (!initialized && rosCars.Count > 0)
        {
            vehFlagChanged();
            trackFlagChanged();
            roundTargetSpeedChanged();

            if(GameManager.Instance.Settings.shouldStartWithGreenFlag &&
                GameManager.Instance.Settings.greenFlagDelay == 0.0f)
            {
                setGreenFlag();
            }
            else if(GameManager.Instance.Settings.shouldStartWithGreenFlag)
            {
                StartCoroutine(DelayedSetGreenFlag());
            }

            bool isPractice = GameManager.Instance.Settings.isPracticeRun;
            float runTimeout = GameManager.Instance.Settings.maxRunTime;
            if(runTimeout > 0f && !isPractice)
            {
                StartCoroutine(RunTimeout());
            }

            initialized = true;
        }
    }

    private IEnumerator DelayedSetGreenFlag()
    {
        yield return new WaitForSeconds(GameManager.Instance.Settings.greenFlagDelay);
        setGreenFlag();
    }

    private IEnumerator RunTimeout()
    {
        yield return new WaitForSeconds(GameManager.Instance.Settings.maxRunTime);
        Debug.Log($"Reset due to max run length exceeded ({GameManager.Instance.Settings.maxRunTime}s)", this);
        GameManager.Instance.OnResetEvent(GameResetReason.Timeout);
    }


    void OnDisable()
    {
        initialized = false;
    }

    void trackFlagChanged()
    {
        int idx = trackFlagDropdown.value;

        if (trackFlagDropdown.options.Count != track_flag_vec.Length)
        {
            Debug.LogError($"Mismatch! Dropdown options: {trackFlagDropdown.options.Count}, track_flag_vec length: {track_flag_vec.Length}");
            return;
        }

        foreach(GameObject car in rosCars)
        {
            RaceControlData raceControl = car.transform.Find("URDF").Find("base_link").Find("Vehicle Sensors").Find("Race Control").GetComponent<RaceControlData>();
            raceControl.rc.TrackFlag = (byte)track_flag_vec[idx];
        }
    }

    void setGreenFlag()
    {
        trackFlagDropdown.value = 2;
        trackFlagChanged();
    }

    void vehFlagChanged()
    {
        // All cars get same vehicle flag
        int idx = vehFlagDropdown.value;
        foreach(GameObject car in rosCars)
        {
            RaceControlData raceControl = car.transform.Find("URDF").Find("base_link").Find("Vehicle Sensors").Find("Race Control").GetComponent<RaceControlData>();
            raceControl.rc.VehicleFlag = (byte)veh_flag_vec[idx];
        }    
    }

    void roundTargetSpeedChanged()
    {
        int idx = roundTargetSpeedDropdown.value;
        foreach(GameObject car in rosCars)
        {
            RaceControlData raceControl = car.transform.Find("URDF").Find("base_link").Find("Vehicle Sensors").Find("Race Control").GetComponent<RaceControlData>();
            raceControl.rc.RoundTargetSpeed = (byte)round_target_speed_vec[idx];
        }    
    }
}
