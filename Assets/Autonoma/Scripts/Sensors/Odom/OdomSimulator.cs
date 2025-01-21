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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleDynamics;

namespace Autonoma
{
public class OdomSimulator : MonoBehaviour
{

    public Vector3 odomAngle; // [deg]
    public Vector3 odomVelWorld;
    public Vector3 odomVelENU;

    public Rigidbody rb;
    void Start()
    {
        rb = HelperFunctions.GetParentComponent<Rigidbody>(transform);
    }

    public GnssSimulator gnssSim;

    void FixedUpdate()
    {   
        // odomVelWorld = rb.velocity; //Velocity COG in Unity CRS
        odomVelENU = gnssSim.antennaVelGlobal;
        odomVelWorld = HelperFunctions.enu2Unity(odomVelENU); //Velocity of Primary Antenna in ENU -> Unity

        float yawAngle = Mathf.Atan2(odomVelWorld.x, odomVelWorld.z) * Mathf.Rad2Deg;
        float pitchAngle = 0f; // FIXME Can be computed from vertical velocity if needed
        float rollAngle = 0f;  // FIXME

        odomAngle = new Vector3(rollAngle, yawAngle, pitchAngle);
        
        // RPY, RHS +, [deg], NORTH = 0 for yaw, EAST = -90, [-180,180]
        odomAngle = HelperFunctions.unity2vehDynCoord(-odomAngle);         

    }


}
}