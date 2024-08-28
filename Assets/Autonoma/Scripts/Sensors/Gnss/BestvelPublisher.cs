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

namespace Autonoma
{
public class BestvelPublisher : Publisher<BESTVEL>
{
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/bestvel";
    public float modifiedFrequency = 100f; //originally 20
    public string modifiedFrameId = "";
    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }
    public NoiseGenerator velNoiseGenerator;

    protected override void Start()
    {
        getPublisherParams();
        this.rosNamespace = modifiedRosNamespace;
        this.topicName = modifiedTopicName;
        this.frequency = modifiedFrequency; // Hz
        this.frameId = modifiedFrameId;
        base.Start();

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
            }
        }
    }
    public GnssSimulator gnssSim;
    public override void fillMsg()
    {
        (ushort week, uint ms) weekMs = GnssSimulator.GetGPSWeekAndMS();

        msg.Nov_header.Gps_week_number = weekMs.week;
        msg.Nov_header.Gps_week_milliseconds = weekMs.ms;

        msg.Vel_type = new PositionOrVelocityType();
        msg.Vel_type.Type = 50;
        msg.Latency = 0.0f;
        msg.Diff_age = 0.0f;
        // msg.Hor_speed = Mathf.Sqrt(Mathf.Pow(gnssSim.vE,2) + Mathf.Pow(gnssSim.vN,2) );
        msg.Trk_gnd = (Mathf.Atan2(gnssSim.vE,gnssSim.vN)*180f/Mathf.PI) % 360;
        // msg.Ver_speed = gnssSim.vU;
        msg.Reserved = 0.0f;

        float velNoiseHorSpeed = (float)velNoiseGenerator.NextGaussian();
        float velNoiseVerSpeed = (float)velNoiseGenerator.NextGaussian();
        msg.Hor_speed = Mathf.Sqrt(Mathf.Pow(gnssSim.vE,2) + Mathf.Pow(gnssSim.vN,2)) + velNoiseHorSpeed;
        msg.Ver_speed = gnssSim.vU + velNoiseVerSpeed;

    }
} // end of class
} // end of autonoma namespace
