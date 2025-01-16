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

using UnityEngine;
using ROS2;
using AWSIM;
using System;
using VehicleDynamics;
using nav_msgs.msg;  // Include for Path.msg
using geometry_msgs.msg;
// using System.Diagnostics;  // Add this namespace for Stopwatch

namespace Autonoma
{
    public class FrontPathSubscriber:MonoBehaviour
    {
        public string frontPathPositionTopic = "/planning/front_path/offset_path";
        public QoSSettings qosSettings = new QoSSettings();
        public WaypointController waypointController;  
        private UnityEngine.Vector3 newTargetPosition = UnityEngine.Vector3.zero;

        ISubscription<Path> frontPathSubscriber;

        void Start()
        {
            waypointController = HelperFunctions.GetParentComponent<WaypointController>(transform);
            var qos = qosSettings.GetQoSProfile();
            waypointController.recievedFrontPath = false;

            frontPathSubscriber = SimulatorROS2Node.CreateSubscription<Path>(frontPathPositionTopic, msg =>
            {
                UpdateWaypointStates(msg);
            }, qos);
        }

        void OnDestroy()
        {
            SimulatorROS2Node.RemoveSubscription<Path>(frontPathSubscriber);
        }

        void UpdateWaypointStates(Path msg)
        {
            // Stopwatch stopwatch = new Stopwatch();
            // stopwatch.Start();

            int numPoses = msg.Poses.Length;
            if (numPoses <= 10)
            {
                UnityEngine.Debug.LogWarning("Not enough poses to be visually seen ");
                return;
            }
            
            int numWaypoints = 5; //should match value in spawnmanager
            
            PoseStamped frontPathPoses; 
            //Set Waypoint positions to be equally spaced based on number and position of poses in path msg
            for (var i = 0; i < numWaypoints; i++){
                if (waypointController.waypointName == "waypoint" + i){
                    waypointController.recievedFrontPath = true;
                    frontPathPoses = msg.Poses[numPoses/numWaypoints/2 * i];
                    float x = (float)frontPathPoses.Pose.Position.X;
                    float y = (float)frontPathPoses.Pose.Position.Y;
                    float z = 0.0f;  // Assuming Z is 0 for 2D positioning in ENU frame

                    newTargetPosition.Set(x, y, z);

                    waypointController.SetTargetPosition(newTargetPosition);  
                }
            }

            // stopwatch.Stop();
    
            // UnityEngine.Debug.Log($"For loop execution time: {stopwatch.ElapsedMilliseconds} ms");

        }
    }
}
