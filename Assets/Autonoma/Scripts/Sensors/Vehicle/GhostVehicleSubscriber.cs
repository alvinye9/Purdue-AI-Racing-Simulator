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
// using autoware_auto_perception_msgs.msg;  // Include Autonoma Perception msg type for BoundingBoxArray (Former implementation)
// using nav_msgs.msg;  // Include for Path.msg
using geometry_msgs.msg; //Point32
using std_msgs.msg; //Float32


namespace Autonoma
{
    public class GhostVehicleSubscriber:MonoBehaviour
    {
        // public string ghostVehiclePositionTopic = "/planning/ghost_veh_position";
        // public string ghostVehiclePathTopic = "/planning/ghost_vehicle/path";
        public string ghostVehiclePositionTopic = "/planning/ghost_vehicle/measurement";
        public string ghostVehicleHeadingTopic = "/planning/ghost_vehicle/heading";
        
        public float targetHeading = 0.0f;
        public QoSSettings qosSettings = new QoSSettings();
        public NpcCarController npcCarController;  // Reference to NPC Car Controller
        // ISubscription<BoundingBoxArray> ghostVehiclePositionSubscriber;
        // ISubscription<Path> ghostVehiclePathSubscriber;
        ISubscription<Point32> ghostVehiclePositionSubscriber;
        ISubscription<Float32> ghostVehicleHeadingSubscriber;

        void Start()
        {
            npcCarController = HelperFunctions.GetParentComponent<NpcCarController>(transform);
            var qos = qosSettings.GetQoSProfile();

            // ghostVehiclePositionSubscriber = SimulatorROS2Node.CreateSubscription<BoundingBoxArray>(ghostVehiclePositionTopic, msg =>
            // {
            //     UpdateGhostVehicleStates(msg);
            // }, qos);

            // ghostVehiclePathSubscriber = SimulatorROS2Node.CreateSubscription<Path>(ghostVehiclePathTopic, msg =>
            // {
            //     UpdateGhostVehicleHeading(msg);
            // }, qos);

            ghostVehiclePositionSubscriber = SimulatorROS2Node.CreateSubscription<Point32>(ghostVehiclePositionTopic, msg =>
            {
                UpdateGhostVehicleStates(msg);
            }, qos);

            ghostVehicleHeadingSubscriber = SimulatorROS2Node.CreateSubscription<Float32>(ghostVehicleHeadingTopic, msg =>
            {
                UpdateGhostVehicleHeading(msg);
            }, qos);
        }

        void OnDestroy()
        {
            // SimulatorROS2Node.RemoveSubscription<BoundingBoxArray>(ghostVehiclePositionSubscriber);
            SimulatorROS2Node.RemoveSubscription<Point32>(ghostVehiclePositionSubscriber);
            SimulatorROS2Node.RemoveSubscription<Float32>(ghostVehicleHeadingSubscriber);
        }

        // void UpdateGhostVehicleStates(BoundingBoxArray msg)
        // {
        //     // npcCarController.isKinematicBool = true; //the npc vehicle should act kinematically if being moved without vehicle dynamics
        //     npcCarController.recievedGhostPosition = true;

        //     if (msg.Boxes.Length == 0)
        //     {
        //         Debug.LogWarning("No bounding boxes in msg");
        //         return;
        //     }

        //     BoundingBox ghostBoundingBox = msg.Boxes[0];

        //     // Update NPC car position based on the ghost vehicle's position
        //     float x = ghostBoundingBox.Centroid.X;
        //     float y = ghostBoundingBox.Centroid.Y;
        //     float z = 0.0f;  // Assuming Z is 0 for 2D positioning in ENU frame

        //     Vector3 newTargetPosition = new Vector3(x, y, z);

        //     float newTargetHeading = ghostBoundingBox.Heading;

        //     npcCarController.SetTargetPosition(newTargetPosition);  
        //     npcCarController.SetTargetHeading(newTargetHeading);
        // }

        // float GetYawFromQuaternion(float x, float y, float z, float w)
        // {
        //     float siny_cosp = 2.0f * (w * z + x * y);
        //     float cosy_cosp = 1.0f - 2.0f * (y * y + z * z);
        //     float yaw = Mathf.Atan2(siny_cosp, cosy_cosp);
        //     return yaw; 
        // }

        void UpdateGhostVehicleStates(Point32 msg)
        {
            npcCarController.recievedGhostPosition = true;

            // Null check for msg
            if (msg == null)
            {
                Debug.LogWarning("Received null message.");
                return; // Exit the function early if msg is null
            }
            // else{
            //     Debug.LogWarning("Message: " + msg.X + " " + msg.Y + " " + msg.Z);
            // }

            float x = msg.X;
            float y = msg.Y;
            float z = 0.0f;  // Assuming Z is 0 for 2D positioning in ENU frame

            UnityEngine.Vector3 newTargetPosition = new UnityEngine.Vector3(x, y, z);

            npcCarController.SetTargetPosition(newTargetPosition);  
            npcCarController.SetTargetHeading(targetHeading); //updated via other callback (or left at 0.0f)
        }

        void UpdateGhostVehicleHeading(Float32 msg)
        {
            // int numPoses = msg.Poses.Length;
            // if (numPoses < 2)
            // {
            //     Debug.LogWarning("Not enough poses to calculate heading.");
            //     return; 
            // }
            // // else{
            // //     Debug.LogWarning("Number of Poses: " + numPoses); //5 poses
            // // }

            // // Use the first and second poses to compute heading angle
            // PoseStamped pose1 = msg.Poses[0]; // First pose
            // PoseStamped pose2 = msg.Poses[1]; // Second pose
            // float dx = (float)(pose2.Pose.Position.X - pose1.Pose.Position.X);
            // float dy = (float)(pose2.Pose.Position.Y - pose1.Pose.Position.Y);
            // float newTargetHeading = Mathf.Atan2(dy, dx); // Result is in radians

            if (msg == null)
            {
                Debug.LogWarning("Received null message.");
                return; // Exit the function early if msg is null
            }

            targetHeading = (float)msg.Data;
        }
    }
}
