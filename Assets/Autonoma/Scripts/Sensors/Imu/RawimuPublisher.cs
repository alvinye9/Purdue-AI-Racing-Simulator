/* 
Copyright 2023 Autonoma, Inc.

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
using novatel_oem7_msgs.msg;

// using System.Collections;
// using System.Collections.Generic;
// using ROS2;

namespace Autonoma
{
public class RawimuPublisher : Publisher<RAWIMU>
{
    
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/rawimu";
    public float modifiedFrequency = 125f;
    public string modifiedFrameId = "";
    
    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }

    public NoiseGenerator gyroNoiseGenerator;
    public NoiseGenerator accelNoiseGenerator;

    protected override void Start()
    {
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

        // Burn some random numbers for top sensor to create divergence
        if (modifiedRosNamespace.Equals("/novatel_top"))
        {
            for (int i = 0; i < 100; i++) // Example: Burn 100 numbers
            {
                accelNoiseGenerator.NextGaussian();
                gyroNoiseGenerator.NextGaussian();
            }
        }
    }
    public ImuSimulator imuSim;
    public override void fillMsg()
    {
        (ushort week, uint ms) weekMs = GnssSimulator.GetGPSWeekAndMS();

        msg.Nov_header.Gps_week_number = weekMs.week;
        msg.Nov_header.Gps_week_milliseconds = weekMs.ms;

        // msg.Header.Frame_id = modifiedFrameId;
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
    }

    
} // end of class
} // end of autonoma namespace
