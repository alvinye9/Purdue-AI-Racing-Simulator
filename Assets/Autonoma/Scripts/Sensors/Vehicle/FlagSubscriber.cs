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
using VehicleDynamics;
// using deep_orange_msgs.msg;
using std_msgs.msg;


namespace Autonoma
{
    public class FlagSubscriber:MonoBehaviour
    {
        public string flagTopic = "/flag_spoofer/vehicle_flag";
        public string trackFlagTopic = "/flag_spoofer/track_flag";
        public RaptorSM sm;
        public RaceControlData racecontrol; 
        public QoSSettings qosSettings = new QoSSettings();

        private bool is_test_mode = false;

        // ISubscription<RcToCt> rctoctSubscriber;
        ISubscription<UInt8> vehicleFlagSubscriber;
        ISubscription<UInt8> trackFlagSubscriber;

        void Start()
        {
            var qos = qosSettings.GetQoSProfile();
            // rctoctSubscriber = SimulatorROS2Node.CreateSubscription<RcToCt>(flagTopic, msg =>
            // {
            //     UpdateSpoofedFlag(msg);
            // }, qos);
            vehicleFlagSubscriber = SimulatorROS2Node.CreateSubscription<UInt8>(flagTopic, msg =>
            {
                UpdateSpoofedVehFlag(msg);
            }, qos);

            trackFlagSubscriber = SimulatorROS2Node.CreateSubscription<UInt8>(trackFlagTopic, msg =>
            {
                UpdateSpoofedTrackFlag(msg);
            }, qos);

            if(GameManager.Instance.Settings.myScenarioObj.ModeSwitchState){
                is_test_mode = true;
                Debug.Log("Currently in test mode, use flag spoofer to send vehicle flags to: " + flagTopic );
                Debug.Log("Currently in test mode, use flag spoofer to send track flags to: " + trackFlagTopic );
            }

        }

        void OnDestroy()
        {
            // SimulatorROS2Node.RemoveSubscription<RcToCt>(rctoctSubscriber);
            SimulatorROS2Node.RemoveSubscription<UInt8>(vehicleFlagSubscriber);
            SimulatorROS2Node.RemoveSubscription<UInt8>(trackFlagSubscriber);
        }

        void UpdateSpoofedVehFlag(UInt8 msg)
        {
            if(GameManager.Instance.Settings.myScenarioObj.ModeSwitchState){
                // sm.current_flag = msg.Vehicle_flag;
                sm.current_flag = msg.Data;
                // Debug.Log("Raptor SM Current Veh Flag and CT state: " + sm.current_flag + " " + sm.current_ct);
            }
        }

        void UpdateSpoofedTrackFlag(UInt8 msg)
        {
            if(GameManager.Instance.Settings.myScenarioObj.ModeSwitchState){
                racecontrol.rc.TrackFlag = msg.Data;
                // Debug.Log("Current Track Flag: " + racecontrol.rc.TrackFlag);
            }
        }

    }
}
