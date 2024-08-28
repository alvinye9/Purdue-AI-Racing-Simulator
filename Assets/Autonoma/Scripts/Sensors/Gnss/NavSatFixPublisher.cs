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

namespace Autonoma
{
public class NavSatFixPublisher : Publisher<NavSatFix>
{
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/fix";
    public float modifiedFrequency = 100f;
    public string modifiedFrameId = "gps_bottom_ant1";
    public float position_covariance = 0.0001f;

    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }
    protected override void Start()
    {
        position_covariance = GameManager.Instance.Settings.mySensorSet.positionCovariance;

        getPublisherParams();
        this.rosNamespace = modifiedRosNamespace;
        this.topicName = modifiedTopicName;
        this.frequency = modifiedFrequency; // Hz
        this.frameId = modifiedFrameId;
        base.Start();
    }
    public GnssSimulator gnssSim;
    public override void fillMsg()
    {
        msg.Header.Frame_id = modifiedFrameId;

        msg.Status.Status = 2;
        msg.Status.Service = 3;
        
        msg.Latitude = gnssSim.lat;
        msg.Longitude = gnssSim.lon;
        msg.Altitude = gnssSim.height;

        msg.Position_covariance_type = 2;

        msg.Position_covariance[0] = position_covariance;
        msg.Position_covariance[4] = position_covariance;
        msg.Position_covariance[8] = position_covariance;

    }
} // end of class
} // end of autonoma namespace