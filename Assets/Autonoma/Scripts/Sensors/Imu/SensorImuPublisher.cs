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
using sensor_msgs.msg;
using VehicleDynamics;
using System;

namespace Autonoma
{
public class SensorImuPublisher : Publisher<Imu>
{
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/imu/data_raw";
    public float modifiedFrequency = 100f;
    public string modifiedFrameId = "gps_bottom_imu";
    public float linear_acceleration_covariance = 0.0009f; //will be overriden in Start()
    public float angular_velocity_covariance = 0.00035f;
    public float orientation_covariance = 0.0001f;
    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }
    public NoiseGenerator gyroNoiseGenerator;
    public NoiseGenerator accelNoiseGenerator;
    public NoiseGenerator headingNoiseGenerator;

    protected override void Start()
    {
        linear_acceleration_covariance = GameManager.Instance.Settings.mySensorSet.linearAccelCovariance;
        angular_velocity_covariance = GameManager.Instance.Settings.mySensorSet.angularVelocityCovariance;

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

        float accelMean = GameManager.Instance.Settings.mySensorSet.accelMean;
        float accelVariance = GameManager.Instance.Settings.mySensorSet.accelVariance;
        int accelSeed = GameManager.Instance.Settings.mySensorSet.accelSeed;
        accelNoiseGenerator = new NoiseGenerator(accelMean, accelVariance, accelSeed);

        float headingMean = GameManager.Instance.Settings.mySensorSet.headingMean;
        float headingVariance = GameManager.Instance.Settings.mySensorSet.headingVariance;
        int headingSeed = GameManager.Instance.Settings.mySensorSet.headingSeed;
        headingNoiseGenerator = new NoiseGenerator(headingMean, headingVariance, headingSeed);

        // Burn some random numbers for top sensor to create divergence
        if (modifiedRosNamespace.Equals("/novatel_top"))
        {
            for (int i = 0; i < 100; i++) // Example: Burn 100 numbers
            {
                headingNoiseGenerator.NextGaussian();
                accelNoiseGenerator.NextGaussian();
                gyroNoiseGenerator.NextGaussian();
            }
        }
    }
    public ImuSimulator imuSim;
 
    public override void fillMsg()
    {
        
        msg.Header.Frame_id = modifiedFrameId;

        float accelNoiseX = (float)accelNoiseGenerator.NextGaussian();
        float accelNoiseY = (float)accelNoiseGenerator.NextGaussian();
        float accelNoiseZ = (float)accelNoiseGenerator.NextGaussian();
        msg.Linear_acceleration = new geometry_msgs.msg.Vector3();
        // msg.Linear_acceleration.X = imuSim.imuAccel.x;
        // msg.Linear_acceleration.Y = imuSim.imuAccel.y;
        // msg.Linear_acceleration.Z = imuSim.imuAccel.z;
        msg.Linear_acceleration.X = imuSim.imuAccel.x + accelNoiseX;
        msg.Linear_acceleration.Y = imuSim.imuAccel.y + accelNoiseY;
        msg.Linear_acceleration.Z = imuSim.imuAccel.z + accelNoiseZ;
        msg.Linear_acceleration_covariance[0] = linear_acceleration_covariance;
        msg.Linear_acceleration_covariance[4] = linear_acceleration_covariance;
        msg.Linear_acceleration_covariance[8] = linear_acceleration_covariance;

        float gyroNoiseX = (float)gyroNoiseGenerator.NextGaussian();
        float gyroNoiseY = (float)gyroNoiseGenerator.NextGaussian();
        float gyroNoiseZ = (float)gyroNoiseGenerator.NextGaussian();
        msg.Angular_velocity = new geometry_msgs.msg.Vector3();
        // msg.Angular_velocity.X = imuSim.imuGyro.x;
        // msg.Angular_velocity.Y = imuSim.imuGyro.y;
        // msg.Angular_velocity.Z = imuSim.imuGyro.z;
        msg.Angular_velocity.X = imuSim.imuGyro.x + gyroNoiseX;
        msg.Angular_velocity.Y = imuSim.imuGyro.y + gyroNoiseY;
        msg.Angular_velocity.Z = imuSim.imuGyro.z + gyroNoiseZ;
        msg.Angular_velocity_covariance[0] = angular_velocity_covariance;
        msg.Angular_velocity_covariance[4] = angular_velocity_covariance;
        msg.Angular_velocity_covariance[8] = angular_velocity_covariance;

// for testing purposes only, in real car there is no orientation element of this message
        // float headingNoiseX = (float)headingNoiseGenerator.NextGaussian();
        // float headingNoiseY = (float)headingNoiseGenerator.NextGaussian();
        // float headingNoiseZ = (float)headingNoiseGenerator.NextGaussian();
        // float imuAngleX = (float)(imuSim.imuAngle.x);
        // float imuAngleY = (float)(imuSim.imuAngle.y);
        // float imuAngleZ = (float)(imuSim.imuAngle.z + 90.0);
        // imuAngleX += headingNoiseX;
        // imuAngleY += headingNoiseY;
        // imuAngleZ += headingNoiseZ;

    //     // //orientation is in ego frame
    //     GameObject egoCarObject = GameObject.Find("DallaraAV24(Clone)");
    //     if (egoCarObject != null)
    //     { 
    //         Transform egoTransform = egoCarObject.transform;
    //         float rollAngle = egoTransform.eulerAngles.z;//roll angle in local frame
    //         float pitchAngle = egoTransform.eulerAngles.x; // Pitch in degrees
    //         float yawAngle = egoTransform.eulerAngles.y;   // Yaw in degrees

    //         // Convert yaw to a range of -180 to 180 degrees
    //         if (yawAngle > 180)
    //         {
    //             yawAngle -= 360;
    //         }

    //         yawAngle = -1 * yawAngle + 90.0f;

    //         // rpy -> quat
    //         Quaternion finalOrientation = HelperFunctions.EulerToQuaternion(rollAngle, pitchAngle, yawAngle); //helper function works better

    //         msg.Orientation.X = finalOrientation.x;
    //         msg.Orientation.Y = finalOrientation.y;
    //         msg.Orientation.Z = finalOrientation.z;
    //         msg.Orientation.W = finalOrientation.w;

    //         msg.Orientation_covariance[0] = orientation_covariance;
    //         msg.Orientation_covariance[4] = orientation_covariance;
    //         msg.Orientation_covariance[8] = orientation_covariance;
    //     }
    //     else{ 
    //         Debug.LogError("EGO VEHICLe NOT FOUND");
    //     }
    }
}
}