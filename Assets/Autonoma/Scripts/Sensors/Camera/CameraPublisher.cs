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
                Debug.LogError("[CameraPublisher] No CameraSensor found on this GameObject.");
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
            // msg.Header.Stamp = this.rosNode.Now(); //FIXME: UNIX time??

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

    }
}