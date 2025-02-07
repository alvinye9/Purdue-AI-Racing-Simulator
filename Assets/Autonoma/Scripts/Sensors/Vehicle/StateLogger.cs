/* 
Copyright 2025 Purdue AI Racing

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

using UnityEngine;
using ROS2;
using AWSIM;
using System;

namespace Autonoma
{
    public class StateLogger:MonoBehaviour
    {
        public RaptorSM sm;
        public RaceControlData racecontrol; 
        public HUDManager hud;
        public QoSSettings qosSettings = new QoSSettings();

        private int updateCounter = 0;
        private int logFrequency = 500; // Log every n update cycles

        void Start()
        {

        }

        void Update(){
            updateCounter++;
            if (updateCounter >= logFrequency)
            {
                LogStates();
                updateCounter = 0;
            }
        }

        void LogStates(){

            string logMessage = $"[StateLogger] " +
                $"Track & Vehicle Flags: {racecontrol.rc.TrackFlag}, {sm.current_flag} | " +
                $"CT & Sys States: {sm.current_ct}, {sm.current_sys} | " +
                $"Speed: {hud.hudMPH} m/s";

            Debug.Log(logMessage);
        }

    }
}
