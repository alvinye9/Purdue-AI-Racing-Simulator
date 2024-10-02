/* 
Copyright 2024 Purdue AI Racing.

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

public class KeyboardInputs : MonoBehaviour
{
    public CarController carController;
    public float throttle,brake,steering;
    public bool gearUp,gearDown;
    void Start()
    {
        carController = HelperFunctions.GetParentComponent<CarController>(transform);
    }
    
    void Update() //without rate limitng
    {
        throttle = Mathf.Clamp(Input.GetAxisRaw("Throttle"), 0f, 1f);
        brake = Mathf.Clamp(Input.GetAxisRaw("Brake"), 0f, 1f);
        steering = Input.GetAxisRaw("Steering");

        // Debug.Log($"Throttle: {throttle}, Brake: {brake}, Steering: {steering}");

        carController.physicalActuator = true;
        carController.gearUp = Input.GetKey(KeyCode.Tab);
        carController.gearDown = Input.GetKey(KeyCode.CapsLock) || Input.GetKey(KeyCode.LeftShift);
        carController.steerAngleCmd = carController.vehicleParams.maxSteeringAngle * -steering / carController.vehicleParams.steeringRatio;
        carController.throttleCmd = throttle;
        carController.brakeCmd = brake * carController.vehicleParams.maxBrakeKpa;

        carController.throttleCmd = Mathf.Clamp(carController.throttleCmd, 0f, 1f);
        carController.brakeCmd = Mathf.Clamp(carController.brakeCmd, 0f, carController.vehicleParams.maxBrakeKpa);
    }

}