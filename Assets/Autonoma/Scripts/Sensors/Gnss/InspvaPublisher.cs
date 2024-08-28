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
using novatel_oem7_msgs.msg;

namespace Autonoma
{
public class InspvaPublisher : Publisher<INSPVA>
{
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/inspva";
    public float modifiedFrequency = 100f;
    public string modifiedFrameId = "";
    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }


    public NoiseGenerator posNoiseGenerator;
    public NoiseGenerator velNoiseGenerator;
    public NoiseGenerator headingNoiseGenerator;


    protected override void Start()
    {
        getPublisherParams();
        this.rosNamespace = modifiedRosNamespace;
        this.topicName = modifiedTopicName;
        this.frequency = modifiedFrequency; // Hz
        this.frameId = modifiedFrameId;
        base.Start();

        // Gaussian Noise Generators
        float velMean = GameManager.Instance.Settings.mySensorSet.velMean;
        float velVariance = GameManager.Instance.Settings.mySensorSet.velVariance;
        int velSeed = GameManager.Instance.Settings.mySensorSet.velSeed;
        velNoiseGenerator = new NoiseGenerator(velMean, velVariance, velSeed);

        float headingMean = GameManager.Instance.Settings.mySensorSet.headingMean;
        float headingVariance = GameManager.Instance.Settings.mySensorSet.headingVariance;
        int headingSeed = GameManager.Instance.Settings.mySensorSet.headingSeed;
        headingNoiseGenerator = new NoiseGenerator(headingMean, headingVariance, headingSeed);

        float posMean = GameManager.Instance.Settings.mySensorSet.posMean;
        float posVariance = GameManager.Instance.Settings.mySensorSet.posVariance;
        int posSeed = GameManager.Instance.Settings.mySensorSet.posSeed;
        posNoiseGenerator = new NoiseGenerator(posMean, posVariance, posSeed);

        // Burn some random numbers for top sensor to create divergence
        if (modifiedRosNamespace.Equals("/novatel_top"))
        {
            for (int i = 0; i < 100; i++) // Example: Burn 100 numbers
            {
                headingNoiseGenerator.NextGaussian();
                posNoiseGenerator.NextGaussian();
                velNoiseGenerator.NextGaussian();
            }
        }

        
    }

    public GnssSimulator gnssSim;
    public ImuSimulator imuSim;
    public override void fillMsg()
    {
        (ushort week, uint ms) weekMs = GnssSimulator.GetGPSWeekAndMS();

        msg.Nov_header.Gps_week_number = weekMs.week;
        msg.Nov_header.Gps_week_milliseconds = weekMs.ms;
        // msg.Latitude = gnssSim.lat + latNoise;
        // msg.Longitude = gnssSim.lon;
        // msg.Height = gnssSim.height;
        // msg.East_velocity = gnssSim.vE;
        // msg.North_velocity = gnssSim.vN;
        // msg.Up_velocity = gnssSim.vU;
        // msg.Roll = imuSim.imuAngle[0]; //deg
        // msg.Pitch = imuSim.imuAngle[1];
        // msg.Azimuth = -imuSim.imuAngle[2]; 

        float headingNoiseX = (float)headingNoiseGenerator.NextGaussian();
        float headingNoiseY = (float)headingNoiseGenerator.NextGaussian();
        float headingNoiseZ = (float)headingNoiseGenerator.NextGaussian();
        float latNoise= (float)posNoiseGenerator.NextGaussian() * 0.00001f;
        float lonNoise = (float)posNoiseGenerator.NextGaussian() * 0.00001f;
        float heightNoise= (float)posNoiseGenerator.NextGaussian();
        float velNoiseX = (float)velNoiseGenerator.NextGaussian();
        float velNoiseY = (float)velNoiseGenerator.NextGaussian();
        float velNoiseZ = (float)velNoiseGenerator.NextGaussian();

        msg.Latitude = gnssSim.lat + latNoise;
        msg.Longitude = gnssSim.lon + lonNoise;
        msg.Height = gnssSim.height + heightNoise;
        msg.East_velocity = gnssSim.vE + velNoiseX;
        msg.North_velocity = gnssSim.vN + velNoiseY;
        msg.Up_velocity = gnssSim.vU + velNoiseZ;
        msg.Roll = imuSim.imuAngle[0] + headingNoiseX; //deg
        msg.Pitch = imuSim.imuAngle[1] + headingNoiseY;
        msg.Azimuth = -imuSim.imuAngle[2] + headingNoiseZ; 

    }
} // end of class
} // end of autonoma namespace