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
using geometry_msgs.msg;
using ROS2;

namespace Autonoma
{
    public class InsTwistPublisher : Publisher<TwistWithCovarianceStamped>
    {
        public string modifiedRosNamespace = "/novatel_bottom";
        public string modifiedTopicName = "/ins_twist";
        public float modifiedFrequency = 125f;
        public string modifiedFrameId = "gps_bottom";
        public float twist_covariance = 0.0f;
        
        public void getPublisherParams()
        {
            // get things from sensor assigned by UI to the sensor
        }

        public NoiseGenerator gyroNoiseGenerator;
        public NoiseGenerator velNoiseGenerator;

        protected override void Start()
        {
            twist_covariance = GameManager.Instance.Settings.mySensorSet.twistCovariance;

            getPublisherParams();
            this.rosNamespace = modifiedRosNamespace;
            this.topicName = modifiedTopicName;
            this.frequency = modifiedFrequency; // Hz
            this.frameId = modifiedFrameId;
            base.Start();

            float gyroMean = GameManager.Instance.Settings.mySensorSet.gyroMean;
            float gyroVariance = GameManager.Instance.Settings.mySensorSet.gyroVariance;
            int gyroSeed = GameManager.Instance.Settings.mySensorSet.gyroSeed;
            gyroNoiseGenerator = new NoiseGenerator(gyroMean, gyroVariance, gyroSeed);

            float velMean = GameManager.Instance.Settings.mySensorSet.velMean;
            float velVariance = GameManager.Instance.Settings.mySensorSet.velVariance;
            int velSeed = GameManager.Instance.Settings.mySensorSet.velSeed;
            velNoiseGenerator = new NoiseGenerator(velMean, velVariance, velSeed);

            // Burn some random numbers for top sensor to create divergence
            if (modifiedRosNamespace.Equals("/novatel_top"))
            {
                for (int i = 0; i < 100; i++) // Example: Burn 100 numbers
                {
                    velNoiseGenerator.NextGaussian();
                    gyroNoiseGenerator.NextGaussian();
                }
            }
        }
        public ImuSimulator imuSim;
        public InsSimulator insSim;
        public override void fillMsg()
        {
            msg.Header.Frame_id = modifiedFrameId;

            float velNoiseX = (float)velNoiseGenerator.NextGaussian();
            float velNoiseY = (float)velNoiseGenerator.NextGaussian();
            float velNoiseZ = (float)velNoiseGenerator.NextGaussian();
            // msg.Twist.Twist.Linear.X = imuSim.imuVelLocal.x; // Forward   
            // msg.Twist.Twist.Linear.Y = imuSim.imuVelLocal.y; // Left
            // msg.Twist.Twist.Linear.Z = imuSim.imuVelLocal.z; // Up
            msg.Twist.Twist.Linear.X = imuSim.imuVelLocal.x + velNoiseX; // Forward   
            msg.Twist.Twist.Linear.Y = imuSim.imuVelLocal.y + velNoiseY; // Left
            msg.Twist.Twist.Linear.Z = imuSim.imuVelLocal.z + velNoiseZ; // Up

            float gyroNoiseX = (float)gyroNoiseGenerator.NextGaussian();
            float gyroNoiseY = (float)gyroNoiseGenerator.NextGaussian();
            float gyroNoiseZ = (float)gyroNoiseGenerator.NextGaussian();
            // msg.Twist.Twist.Angular.X = imuSim.imuGyro.x; 
            // msg.Twist.Twist.Angular.Y = imuSim.imuGyro.y; 
            // msg.Twist.Twist.Angular.Z = imuSim.imuGyro.z; 
            msg.Twist.Twist.Angular.X = imuSim.imuGyro.x + gyroNoiseX; 
            msg.Twist.Twist.Angular.Y = imuSim.imuGyro.y + gyroNoiseY; 
            msg.Twist.Twist.Angular.Z = imuSim.imuGyro.z + gyroNoiseZ; 

            msg.Twist.Covariance[0] = twist_covariance;
            msg.Twist.Covariance[7] = twist_covariance;
            msg.Twist.Covariance[14] = twist_covariance;
            msg.Twist.Covariance[21] = twist_covariance;
            msg.Twist.Covariance[28] = twist_covariance;
            msg.Twist.Covariance[35] = twist_covariance;

        }


    }
}
