/* 
Copyright 2024 Purdue AI Racing
Copyright 2023 Autonoma Inc.

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
using ROS2;
using AWSIM;
using System;
using VehicleDynamics;
using autonoma_msgs.msg;

namespace Autonoma
{
public class VehicleInputSubscriber : MonoBehaviour
{
    public string vehicleInputsTopic = "/vehicle_inputs";
    public QoSSettings qosSettings = new QoSSettings();
    public CarController carController;
    ISubscription<VehicleInputs> vehicleInputsSubscriber;

    CanSubscriber canSteerCommandSubscriber;
    CanSubscriber canThrottleCommandSubscriber;
    CanSubscriber canBrakeCommandSubscriber;
    CanSubscriber canGearCommandSubscriber;
    CanSubscriber canDashSwitchesCmdSubscriber; 
    // private bool brakeBiasMessageReceived = false;

    void Start()
    {
        carController = HelperFunctions.GetParentComponent<CarController>(transform);
        var qos = qosSettings.GetQoSProfile();


        vehicleInputsSubscriber = SimulatorROS2Node.CreateSubscription<VehicleInputs>(vehicleInputsTopic, msg =>
            {
                UpdateVehicleInputs(msg);
            }, qos);

        // if not Npc, subscribe from CAN
        int targetLayer = LayerMask.NameToLayer("Npc");
        if (gameObject.layer != targetLayer)
        {
            Debug.Log("NOT NPC VEHICLE, STARTING CAN SUBSCRIBING");

            canSteerCommandSubscriber = new CanSubscriber("steering_cmd", qosSettings, data_values => { //vehicle_inputs not triggering this
                carController.steerAngleCmd = (float)data_values[1]/carController.vehicleParams.steeringRatio;
                float maxAngleAtwheel = Mathf.Abs(carController.vehicleParams.maxSteeringAngle/carController.vehicleParams.steeringRatio);
                carController.steerAngleCmd = Mathf.Clamp(carController.steerAngleCmd,-maxAngleAtwheel,maxAngleAtwheel);
            });

            canThrottleCommandSubscriber = new CanSubscriber("accelerator_cmd", qosSettings, data_values => {
                carController.throttleCmd = (float)data_values[1] / 100f;
                carController.throttleCmd = Mathf.Clamp(carController.throttleCmd,0f,1f); 
                // if(data_values[1] == null){
                //     Debug.Log("No accelerator_cmd: "+ data_values[1]);
                // }
                // Debug.Log("accelerator_cmd from CAN (#1): " + carController.throttleCmd);
            });

            canBrakeCommandSubscriber = new CanSubscriber("brake_pressure_cmd", qosSettings, data_values => {
                carController.brakeCmd = (float)data_values[1];
                carController.brakeCmd = Mathf.Clamp(carController.brakeCmd,0f,carController.vehicleParams.maxBrakeKpa);
            });

            canGearCommandSubscriber = new CanSubscriber("gear_shift_cmd", qosSettings, data_values => {
                carController.gearUp = false;
                carController.gearDown = false;
                int newGear = (int)data_values[0];
                if (newGear > carController.gear)
                {
                    carController.gearUp = true;
                }
                if (newGear < carController.gear)
                {
                    carController.gearDown = true;
                }
            });

            canDashSwitchesCmdSubscriber = new CanSubscriber("dash_switches_cmd", qosSettings, data_values => {
                // Debug.Log("Default Brake Bias: " + carController.vehicleParams.brakeBias );
                if (carController.brakeCmd == 0f){
                    carController.vehicleParams.brakeBias = 0.5f;
                    if(!(data_values == null))
                    {
                        // Debug.Log("Dash Switches Cmd Message recieved: " + (0.5f + (float)data_values[2] / 100f));
                        carController.vehicleParams.brakeBias = 0.5f + (float)data_values[2] / 100f;
                    }
                }
            });
        }
    }

    void OnDestroy()
    {
        SimulatorROS2Node.RemoveSubscription<VehicleInputs>(vehicleInputsSubscriber);
    }

    void UpdateVehicleInputs(VehicleInputs msg)
    {
        carController.steerAngleCmd = msg.Steering_cmd/carController.vehicleParams.steeringRatio; // max 200
        float maxAngleAtwheel = Mathf.Abs(carController.vehicleParams.maxSteeringAngle/carController.vehicleParams.steeringRatio);
        carController.steerAngleCmd = Mathf.Clamp(carController.steerAngleCmd,-maxAngleAtwheel,maxAngleAtwheel);

        carController.throttleCmd = msg.Throttle_cmd / 100f;
        carController.throttleCmd = Mathf.Clamp(carController.throttleCmd,0f,1f);
        carController.brakeCmd = msg.Brake_cmd; // 1 pasca = 1* 0.001*0.54nm
        carController.brakeCmd = Mathf.Clamp(carController.brakeCmd,0f,carController.vehicleParams.maxBrakeKpa);
        carController.gearUp = false;
        carController.gearDown = false;
        if (msg.Gear_cmd > carController.gear)
        {
            carController.gearUp = true;
        }
        if (msg.Gear_cmd < carController.gear)
        {
            carController.gearDown = true;
        }
    }

}
}