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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROS2;
using System.IO;
using System;
using sensor_msgs.msg;

namespace Autonoma
{
    [RequireComponent(typeof(CameraSensor))]
    public class CameraInfoPublisher : Publisher<CameraInfo>
    {
        // [Header("ROS Topic parameters")]

        public string modifiedRosNamespace = "/perception";
        public string modifiedTopicName = "/camera/info";
        public float modifiedFrequency = 30f;
        public string modifiedFrameId = "camera_link";

        public void getPublisherParams()
        {
            // get things from sensor assigned by ui to the sensor
        }

        // Publishers
        IPublisher<sensor_msgs.msg.Image> imagePublisher;
        IPublisher<sensor_msgs.msg.CameraInfo> cameraInfoPublisher;
        sensor_msgs.msg.Image imageMsg;
        sensor_msgs.msg.CameraInfo cameraInfoMsg;

        private CameraSensor sensor;

        protected override void  Start()
        {
            this.rosNamespace = modifiedRosNamespace;
            this.topicName = modifiedTopicName;
            this.frequency = modifiedFrequency; // Hz
            this.frameId = modifiedFrameId;
            base.Start();

            sensor = GetComponent<CameraSensor>();
            if (sensor == null)
            {
               Debug.LogError("[CameraInfoPublisher] No CameraSensor found on this GameObject.");
                return;
            }
            
            // callback
            sensor.OnOutputData += OnNewCameraData;

        }

        public void OnNewCameraData(CameraSensor.OutputData outputData)
        {
            fillMsg(outputData);
        }

        public void fillMsg(CameraSensor.OutputData data)
        {
            if (msg == null)
            {
                msg = new sensor_msgs.msg.CameraInfo();
            }

            msg.Header.Frame_id = modifiedFrameId;
            // msg.Header.Stamp = SimulatorROS2Node.GetCurrentRosTime(); //FIXME: UNIX time??

            msg.Distortion_model = "plumb_bob"; //FIXME: confirm distortion model

            CameraSensor.CameraParameters cameraParameters = data.cameraParameters;

            msg.Width = (uint)cameraParameters.width;
            msg.Height = (uint)cameraParameters.height;

            // Update distortion parameters
            msg.D = cameraParameters.getDistortionParameters();

            // Update camera matrix
            var K = cameraParameters.getCameraMatrix();
            for (int i = 0; i < K.Length; i++)
                msg.K[i] = K[i];

            // Update projection matrix
            var P = cameraParameters.getProjectionMatrix();
            for (int i = 0; i < P.Length; i++)
                msg.P[i] = P[i];
        }


        // private void UpdateCameraInfoMsg(CameraSensor.CameraParameters cameraParameters)
        // {
        //     if (cameraInfoMsg.Width != cameraParameters.width || cameraInfoMsg.Height != cameraParameters.height)
        //     {
        //         cameraInfoMsg.Width = (uint)cameraParameters.width;
        //         cameraInfoMsg.Height = (uint)cameraParameters.height;
        //     }

        //     // Update distortion parameters
        //     var D = cameraParameters.getDistortionParameters();
        //     if (!D.Equals(cameraInfoMsg.D))
        //     {
        //         cameraInfoMsg.D = cameraParameters.getDistortionParameters();
        //     }

        //     // Update camera matrix
        //     var K = cameraParameters.getCameraMatrix();
        //     if (!K.Equals(cameraInfoMsg.K))
        //     {
        //         for (int i = 0; i < K.Length; i++)
        //             cameraInfoMsg.K[i] = K[i];
        //     }

        //     // Update projection matrix
        //     var P = cameraParameters.getProjectionMatrix();
        //     if (!P.Equals(cameraInfoMsg.P))
        //     {
        //         for (int i = 0; i < P.Length; i++)
        //             cameraInfoMsg.P[i] = P[i];
        //     }
        // }

        // private sensor_msgs.msg.CameraInfo InitializeEmptyCameraInfoMsg()
        // {
        //     var message = new sensor_msgs.msg.CameraInfo()
        //     {
        //         Header = new std_msgs.msg.Header()
        //         {
        //             Frame_id = frameId
        //         },
        //         Distortion_model = "plumb_bob",
        //         Binning_x = 0,
        //         Binning_y = 0,
        //         Roi = new sensor_msgs.msg.RegionOfInterest()
        //         {
        //             X_offset = 0,
        //             Y_offset = 0,
        //             Height = 0,
        //             Width = 0,
        //             Do_rectify = false,
        //         }
        //     };

        //     // Set the rectification matrix for monocular camera
        //     var R = new double[] {
        //         1.0, 0.0, 0.0,
        //         0.0, 1.0, 0.0,
        //         0.0, 0.0, 1.0};

        //     for (int i = 0; i < R.Length; i++)
        //         message.R[i] = R[i];

        //     return message;
        // }
    }
}