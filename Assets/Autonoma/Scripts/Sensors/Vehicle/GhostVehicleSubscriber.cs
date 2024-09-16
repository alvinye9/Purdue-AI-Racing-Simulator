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
using autoware_auto_perception_msgs.msg;  // Include Autonoma Perception msg type for BoundingBoxArray

namespace Autonoma
{
    public class GhostVehicleSubscriber:MonoBehaviour
    {
        public string ghostVehiclePositionTopic = "/planning/ghost_veh_position";
        public QoSSettings qosSettings = new QoSSettings();
        public NpcCarController npcCarController;  // Reference to NPC Car Controller
        ISubscription<BoundingBoxArray> ghostVehiclePositionSubscriber;

        void Start()
        {
            npcCarController = HelperFunctions.GetParentComponent<NpcCarController>(transform);
            var qos = qosSettings.GetQoSProfile();

            ghostVehiclePositionSubscriber = SimulatorROS2Node.CreateSubscription<BoundingBoxArray>(ghostVehiclePositionTopic, msg =>
            {
                UpdateGhostVehicleStates(msg);
            }, qos);
        }

        void OnDestroy()
        {
            SimulatorROS2Node.RemoveSubscription<BoundingBoxArray>(ghostVehiclePositionSubscriber);
        }

        void UpdateGhostVehicleStates(BoundingBoxArray msg)
        {
            // npcCarController.isKinematicBool = true; //the npc vehicle should act kinematically if being moved without vehicle dynamics
            npcCarController.recievedGhostPosition = true;

            if (msg.Boxes.Length == 0)
            {
                Debug.LogWarning("No bounding boxes in msg");
                return;
            }

            BoundingBox ghostBoundingBox = msg.Boxes[0];

            // Update NPC car position based on the ghost vehicle's position
            float x = ghostBoundingBox.Centroid.X;
            float y = ghostBoundingBox.Centroid.Y;
            float z = 0.0f;  // Assuming Z is 0 for 2D positioning in ENU frame

            Vector3 newTargetPosition = new Vector3(x, y, z);

            float newTargetHeading = ghostBoundingBox.Heading;

            npcCarController.SetTargetPosition(newTargetPosition);  
            npcCarController.SetTargetHeading(newTargetHeading);
        }
    }
}
