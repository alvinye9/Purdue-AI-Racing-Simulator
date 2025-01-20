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
using nav_msgs.msg;
using std_msgs.msg;
using geometry_msgs.msg;
using ROS2;

namespace Autonoma
{
    public class OdomPublisher : Publisher<Odometry>
    {
        public string modifiedRosNamespace = "/novatel_bottom";
        public string modifiedTopicName = "/odom";
        public float modifiedFrequency = 125f;
        public string modifiedFrameId = "utm";
        public string modifiedChildFrameId = "gps_top_ant1";
        private LatLngUTMConverter latLngUtmConverter;
        private double latitude;
        private double longitude;
        private double utmX;
        private double utmY;
        public float position_covariance = 0.0f;
        public float twist_orientation_covariance = 0.0f;
        public float orientation_covariance = 0.0f;
        
        public void getPublisherParams()
        {
            // get things from sensor assigned by UI to the sensor
        }

        public NoiseGenerator headingNoiseGenerator;
        public NoiseGenerator posNoiseGenerator;
        public NoiseGenerator gyroNoiseGenerator;
        public NoiseGenerator velNoiseGenerator;

        protected override void Start()
        {
            position_covariance = GameManager.Instance.Settings.mySensorSet.positionCovariance;
            twist_orientation_covariance = GameManager.Instance.Settings.mySensorSet.orientationCovariance;

            getPublisherParams();
            this.rosNamespace = modifiedRosNamespace;
            this.topicName = modifiedTopicName;
            this.frequency = modifiedFrequency; // Hz
            this.frameId = modifiedFrameId;
            latLngUtmConverter = new LatLngUTMConverter("WGS 84"); //initialize converter
            base.Start();

            float headingMean = GameManager.Instance.Settings.mySensorSet.headingMean;
            float headingVariance = GameManager.Instance.Settings.mySensorSet.headingVariance;
            int headingSeed = GameManager.Instance.Settings.mySensorSet.headingSeed;
            headingNoiseGenerator = new NoiseGenerator(headingMean, headingVariance, headingSeed);

            float gyroMean = GameManager.Instance.Settings.mySensorSet.gyroMean;
            float gyroVariance = GameManager.Instance.Settings.mySensorSet.gyroVariance;
            int gyroSeed = GameManager.Instance.Settings.mySensorSet.gyroSeed;
            gyroNoiseGenerator = new NoiseGenerator(gyroMean, gyroVariance, gyroSeed);

            float velMean = GameManager.Instance.Settings.mySensorSet.velMean;
            float velVariance = GameManager.Instance.Settings.mySensorSet.velVariance;
            int velSeed = GameManager.Instance.Settings.mySensorSet.velSeed;
            velNoiseGenerator = new NoiseGenerator(velMean, velVariance, velSeed);

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
                    velNoiseGenerator.NextGaussian();
                    gyroNoiseGenerator.NextGaussian();
                    posNoiseGenerator.NextGaussian();
                }
            }
        }
        public OdomSimulator odomSim;
        public GnssSimulator gnssSim;
        public ImuSimulator imuSim;
        public override void fillMsg()
        {
            msg.Header.Frame_id = modifiedFrameId;
            msg.Child_frame_id = modifiedChildFrameId;

            float latNoise = (float)posNoiseGenerator.NextGaussian();
            float lonNoise = (float)posNoiseGenerator.NextGaussian();
            // Convert latitude and longitude to UTM coordinates
            latitude = gnssSim.lat;
            longitude = gnssSim.lon;
            latitude += latNoise;
            longitude += lonNoise;
            var utmResult = latLngUtmConverter.convertLatLngToUtm(latitude, longitude);
            utmX = utmResult.Easting;
            utmY = utmResult.Northing;

            //Position
            msg.Pose.Pose.Position.X = utmX;
            msg.Pose.Pose.Position.Y = utmY;
            msg.Pose.Pose.Position.Z = gnssSim.height;

            float headingNoiseX = (float)headingNoiseGenerator.NextGaussian();
            float headingNoiseY = (float)headingNoiseGenerator.NextGaussian();
            float headingNoiseZ = (float)headingNoiseGenerator.NextGaussian();
            float odomAngleX = (float)(odomSim.odomAngle.x);
            float odomAngleY = (float)(odomSim.odomAngle.y);
            float odomAngleZ = (float)(odomSim.odomAngle.z + 90.0);
            odomAngleX += headingNoiseX;
            odomAngleY += headingNoiseY;
            odomAngleZ += headingNoiseZ;

            float gammaDeg = latLngUtmConverter.computeMeridianConvergence(utmResult.ZoneNumber, latitude, longitude);
            // Debug.LogWarning("Meridian Convergence (Deg): " + gammaDeg);
            UnityEngine.Quaternion gammaQuat = UnityEngine.Quaternion.Euler(0, 0, gammaDeg); // Negative gamma for ENU -> UTM
            UnityEngine.Quaternion imuEnuQuat = UnityEngine.Quaternion.Euler(odomAngleY, odomAngleX, odomAngleZ); //The Euler input is in ENU
            UnityEngine.Quaternion finalQuat = imuEnuQuat * gammaQuat; // Adjust quat with meridian convergence
            finalQuat = UnityEngine.Quaternion.Normalize(finalQuat); // Normalize to prevent drift

            msg.Pose.Pose.Orientation.X = finalQuat.x;
            msg.Pose.Pose.Orientation.Y = finalQuat.y;
            msg.Pose.Pose.Orientation.Z = finalQuat.z;
            msg.Pose.Pose.Orientation.W = finalQuat.w;

            //====== Twist =========
            float velNoiseX = (float)velNoiseGenerator.NextGaussian();
            float velNoiseY = (float)velNoiseGenerator.NextGaussian();
            float velNoiseZ = (float)velNoiseGenerator.NextGaussian();
            msg.Twist.Twist.Linear.X = imuSim.imuVelLocal.x + velNoiseX; // Forward   //change this to be using GPS twist
            msg.Twist.Twist.Linear.Y = imuSim.imuVelLocal.y + velNoiseY; // Left
            msg.Twist.Twist.Linear.Z = imuSim.imuVelLocal.z + velNoiseZ; // Up

            float gyroNoiseX = (float)gyroNoiseGenerator.NextGaussian();
            float gyroNoiseY = (float)gyroNoiseGenerator.NextGaussian();
            float gyroNoiseZ = (float)gyroNoiseGenerator.NextGaussian();
            msg.Twist.Twist.Angular.X = imuSim.imuGyro.x + gyroNoiseX; 
            msg.Twist.Twist.Angular.Y = imuSim.imuGyro.y + gyroNoiseY; 
            msg.Twist.Twist.Angular.Z = imuSim.imuGyro.z + gyroNoiseZ; 

            //Covariance
            msg.Pose.Covariance[0] = position_covariance;
            msg.Pose.Covariance[7] = position_covariance;
            msg.Pose.Covariance[14] = position_covariance;

            float vehicleSpeed = odomSim.odomVelWorld.magnitude;
            orientation_covariance = Mathf.Pow(Mathf.Atan2(0.3f, vehicleSpeed), 2);

            msg.Pose.Covariance[21] = orientation_covariance;
            msg.Pose.Covariance[28] = orientation_covariance;
            msg.Pose.Covariance[35] = orientation_covariance;

            msg.Twist.Covariance[0] = position_covariance;
            msg.Twist.Covariance[7] = position_covariance;
            msg.Twist.Covariance[14] = position_covariance;
            msg.Twist.Covariance[21] = twist_orientation_covariance;
            msg.Twist.Covariance[28] = twist_orientation_covariance;
            msg.Twist.Covariance[35] = twist_orientation_covariance;
        }

    }
}
