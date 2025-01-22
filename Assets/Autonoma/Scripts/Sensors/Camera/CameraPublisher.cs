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
    public class CameraPublisher : Publisher<Image>
    {
        // [Header("ROS Topic parameters")]

        public string modifiedRosNamespace = "/perception";
        public string modifiedTopicName = "/camera/image_raw";
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
                // throw new MissingComponentException("No active CameraSensor component found.");
                Debug.LogError("[CameraRos2Publisher] No CameraSensor found on this GameObject.");
                return;
            }
            // callback
            sensor.OnOutputData += OnNewCameraData;

        }

        public void OnNewCameraData(CameraSensor.OutputData outputData)
        {
            // Debug.Log("Reached callback");
            // Populate the message and let Publisher<Image> handle publishing
            fillMsg(outputData);
        }

        public void fillMsg(CameraSensor.OutputData data)
        {
            if (msg == null)
            {
                msg = new sensor_msgs.msg.Image();
            }

            msg.Header.Frame_id = modifiedFrameId;
            // msg.Header.Stamp = this.rosNode.Now(); // Ensure timestamp synchronization

            msg.Encoding = "bgr8";
            msg.Is_bigendian = 0;
            msg.Width = (uint)data.cameraParameters.width;
            msg.Height = (uint)data.cameraParameters.height;
            msg.Step = (uint)(data.cameraParameters.width * 3);

            if (sensor.outputData.imageDataBuffer != null && sensor.outputData.imageDataBuffer.Length > 0)
            {
                msg.Data = sensor.outputData.imageDataBuffer;
            }
            else
            {
                Debug.LogError("[CameraPublisher] Image buffer is empty, cannot publish!");
            }
        }

        // void UpdateMessagesAndPublish(CameraSensor.OutputData outputData)
        // {
        //     if (!SimulatorROS2Node.Ok())
        //     {
        //         return;
        //     }

        //     // Update msgs
        //     UpdateImageMsg(outputData);
        //     // UpdateCameraInfoMsg(outputData.cameraParameters);

        //     // Update msgs timestamp, timestamps should be synchronized in order to connect image and camera_info msgs
        //     var timeMsg = SimulatorROS2Node.GetCurrentRosTime();
        //     imageMsg.Header.Stamp = timeMsg;
        //     // cameraInfoMsg.Header.Stamp = timeMsg;

        //     // Publish to ROS2
        //     imagePublisher.Publish(imageMsg);
        //     // cameraInfoPublisher.Publish(cameraInfoMsg);
        // }

        private void UpdateImageMsg(CameraSensor.OutputData data)
        {
            if (imageMsg.Width != data.cameraParameters.width || imageMsg.Height != data.cameraParameters.height)
            {
                imageMsg.Width = (uint)data.cameraParameters.width;
                imageMsg.Height = (uint)data.cameraParameters.height;
                imageMsg.Step = (uint)(data.cameraParameters.width * 3);

                imageMsg.Data = new byte[data.cameraParameters.height * data.cameraParameters.width * 3];
            }

            imageMsg.Data = data.imageDataBuffer;
        }

        private void UpdateCameraInfoMsg(CameraSensor.CameraParameters cameraParameters)
        {
            if (cameraInfoMsg.Width != cameraParameters.width || cameraInfoMsg.Height != cameraParameters.height)
            {
                cameraInfoMsg.Width = (uint)cameraParameters.width;
                cameraInfoMsg.Height = (uint)cameraParameters.height;
            }

            // Update distortion parameters
            var D = cameraParameters.getDistortionParameters();
            if (!D.Equals(cameraInfoMsg.D))
            {
                cameraInfoMsg.D = cameraParameters.getDistortionParameters();
            }

            // Update camera matrix
            var K = cameraParameters.getCameraMatrix();
            if (!K.Equals(cameraInfoMsg.K))
            {
                for (int i = 0; i < K.Length; i++)
                    cameraInfoMsg.K[i] = K[i];
            }

            // Update projection matrix
            var P = cameraParameters.getProjectionMatrix();
            if (!P.Equals(cameraInfoMsg.P))
            {
                for (int i = 0; i < P.Length; i++)
                    cameraInfoMsg.P[i] = P[i];
            }
        }

        private sensor_msgs.msg.Image InitializeEmptyImageMsg()
        {
            return new sensor_msgs.msg.Image()
            {
                Header = new std_msgs.msg.Header()
                {
                    Frame_id = frameId
                },
                Encoding = "bgr8",
                Is_bigendian = 0,
            };
        }

        private sensor_msgs.msg.CameraInfo InitializeEmptyCameraInfoMsg()
        {
            var message = new sensor_msgs.msg.CameraInfo()
            {
                Header = new std_msgs.msg.Header()
                {
                    Frame_id = frameId
                },
                Distortion_model = "plumb_bob",
                Binning_x = 0,
                Binning_y = 0,
                Roi = new sensor_msgs.msg.RegionOfInterest()
                {
                    X_offset = 0,
                    Y_offset = 0,
                    Height = 0,
                    Width = 0,
                    Do_rectify = false,
                }
            };

            // Set the rectification matrix for monocular camera
            var R = new double[] {
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0};

            for (int i = 0; i < R.Length; i++)
                message.R[i] = R[i];

            return message;
        }
    }
}