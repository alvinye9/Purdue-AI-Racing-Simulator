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
using VehicleDynamics;
using std_msgs.msg;
using geometry_msgs.msg;

namespace Autonoma
{
public class CompassPublisher : Publisher<PoseWithCovarianceStamped>
{
    public string modifiedRosNamespace = "/novatel_bottom";
    public string modifiedTopicName = "/Compass";
    public float modifiedFrequency = 100f;
    public string modifiedFrameId = "";
    public float pose_covariance = 0.0f;
    public bool isBottom = true;

    public void getPublisherParams()
    {
        // get things from sensor assigned by ui to the sensor
    }
    protected override void Start()
    {
        getPublisherParams();
        this.rosNamespace = modifiedRosNamespace;
        this.topicName = modifiedTopicName;
        this.frequency = modifiedFrequency; // Hz
        this.frameId = modifiedFrameId;
        base.Start();
    }
    public Heading2Simulator heading2Sim;
    public ImuSimulator imuSim;

    public override void fillMsg()
    {
        msg.Header.Frame_id = modifiedFrameId;

        //// in the future see if we can get Compass to be simulated similarly to heading2
        // // Convert heading2 yaw from NED to ENU and then to quaternion
        // UnityEngine.Quaternion compassOrientation = UnityEngine.Quaternion.Euler(0f, heading2Sim.heading, 0f); //heading in degrees
        // msg.Pose.Pose.Orientation.X = compassOrientation.x;
        // msg.Pose.Pose.Orientation.Y  = compassOrientation.y;
        // msg.Pose.Pose.Orientation.Z = compassOrientation.z;
        // msg.Pose.Pose.Orientation.W = compassOrientation.w;

        //functionally should be the same as imu/data
        float imuAngleX = (float)(imuSim.imuAngle.x);
        float imuAngleY = (float)(imuSim.imuAngle.y);
        float imuAngleZ = (float)(imuSim.imuAngle.z + 90.0);
        
        UnityEngine.Quaternion quat = UnityEngine.Quaternion.Euler(imuAngleY, imuAngleX, imuAngleZ);
        msg.Pose.Pose.Orientation.X = quat.x;
        msg.Pose.Pose.Orientation.Y  = quat.y;
        msg.Pose.Pose.Orientation.Z = quat.z;
        msg.Pose.Pose.Orientation.W = quat.w;

        // Set covariance diagonals to 6e-5
        msg.Pose.Covariance[0] = pose_covariance;
        msg.Pose.Covariance[7] = pose_covariance;
        msg.Pose.Covariance[14] = pose_covariance;
        msg.Pose.Covariance[21] = pose_covariance;
        msg.Pose.Covariance[28] = pose_covariance;
        msg.Pose.Covariance[35] = pose_covariance;

    }
} // end of class
} // end of autonoma namespace