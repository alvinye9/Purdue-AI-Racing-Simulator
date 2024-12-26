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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class WaypointController : MonoBehaviour
{
    public Rigidbody waypoint;
    public bool recievedFrontPath = false;
    public Rigidbody egoCarBody; 
    private Vector3 targetPosition;  // Current target position for the waypoint
    public string waypointName;

    void Start()
    {
        waypoint = GetComponent<Rigidbody>();
        waypointName = waypoint.name;

        GameObject egoCarObject = GameObject.Find("DallaraAV24(Clone)");
        if (egoCarObject != null)
        {
            egoCarBody = egoCarObject.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("EgoCar object not found in the scene!");
        }

    }
    void Update()
    {
        if (recievedFrontPath)
        {
            targetPosition = HelperFunctions.vehDynCoord2Unity(targetPosition); //convert ghost_frame CRS (conventional veh dyn CRS) -> Unity CRS

            // Calculate the relative target position in the ego car's local frame
            Vector3 relativeTargetPosition = egoCarBody.transform.TransformPoint(targetPosition);
            relativeTargetPosition.y = egoCarBody.position.y;
            targetPosition = relativeTargetPosition;

            DirectSetStates(targetPosition);


        }
    }

    // Directly set the waypoint position
    public void DirectSetStates(Vector3 newPosition)
    {
        waypoint.position = newPosition;  // Directly set Rigidbody's position
        waypoint.velocity = Vector3.zero;  // Reset velocity
        transform.position = newPosition;  // Update transform position as well
    }

    // Set the target position for the car to move towards, called from FrontPathSubscriber.cs
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position; //in veh dynamics frame (RH)
    }

}
