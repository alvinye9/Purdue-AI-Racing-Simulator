/* 
Copyright 2023 Autonoma, Inc.

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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public ScenarioObj myScenarioObj = new ScenarioObj();
    public VehSetup myVehSetup = new VehSetup();
    public SensorSet mySensorSet = new SensorSet();
    public TrackParams myTrackParams;
    public bool shouldBypassMenu = false;
    public bool shouldStartWithGreenFlag = false;
    public float greenFlagDelay = 0.0f; // if starting with green flag, delay before switching
    public System.String menuSceneName = "MenuScene";
    public bool useLapTimeInterpolationAdjustment = true;

    public float maxRunTime = 0.0f;
    public int maxLaps = 0;
    public bool exitOnCompletion = false;

    public bool isPracticeRun = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
