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
using System.Collections.Generic;
using autonoma_msgs.msg;

namespace Autonoma
{
public class VehicleDataPublisher : Publisher<VehicleData>
{
    public string modifiedRosNamespace = "";
    public string modifiedTopicName = "/vehicle_data";
    public float modifiedFrequency = 100f;
    public string modifiedFrameId = "";

    public CanPublisher canWheelPotentiometerPublisher;
    public CanPublisher canWheelStrainGuagePublisher;

    public CanPublisher canRRTirePressurePublisher;
    public CanPublisher canRLTirePressurePublisher;
    public CanPublisher canFRTirePressurePublisher;
    public CanPublisher canFLTirePressurePublisher;

    public CanPublisher canMiscReportPublisher;
    public CanPublisher canAccelReportPublisher;
    public CanPublisher canSteerReportPublisher;
    public CanPublisher canSteerReportExtdPublisher;
     public CanPublisher canSteerReportExtd2Publisher;

    public CanPublisher canRRTireTempPublisher;
    public CanPublisher canRLTireTempPublisher;
    public CanPublisher canFRTireTempPublisher;
    public CanPublisher canFLTireTempPublisher;

    public CanPublisher canBrakePressureReportPublisher;
    public CanPublisher canBrakeReportExtdPublisher;
    public CanPublisher canBrakeReportExtd2Publisher;
    public CanPublisher canWheelSpeedReportPublisher;
    public CanPublisher canDashSwitchesCmdPublisher;


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

        canWheelPotentiometerPublisher = new CanPublisher("wheel_potentiometer_data", rosNamespace, qosSettings);
        canWheelStrainGuagePublisher = new CanPublisher("wheel_strain_gauge", rosNamespace, qosSettings);

        canRRTirePressurePublisher = new CanPublisher("Tire_Pressure_RR", rosNamespace, qosSettings);
        canRLTirePressurePublisher = new CanPublisher("Tire_Pressure_RL", rosNamespace, qosSettings);
        canFRTirePressurePublisher = new CanPublisher("Tire_Pressure_FR", rosNamespace, qosSettings);
        canFLTirePressurePublisher = new CanPublisher("Tire_Pressure_FL", rosNamespace, qosSettings);

        canRRTireTempPublisher = new CanPublisher("Tire_Temp_RR_1", rosNamespace, qosSettings);
        canRLTireTempPublisher = new CanPublisher("Tire_Temp_RL_1", rosNamespace, qosSettings);
        canFRTireTempPublisher = new CanPublisher("Tire_Temp_FR_1", rosNamespace, qosSettings);
        canFLTireTempPublisher = new CanPublisher("Tire_Temp_FL_1", rosNamespace, qosSettings);

        canMiscReportPublisher = new CanPublisher("misc_report", rosNamespace, qosSettings);
        canAccelReportPublisher = new CanPublisher("accelerator_report", rosNamespace, qosSettings);
        canSteerReportPublisher = new CanPublisher("steering_report", rosNamespace, qosSettings);
        canSteerReportExtdPublisher = new CanPublisher("steering_report_extd", rosNamespace, qosSettings);
        canSteerReportExtd2Publisher = new CanPublisher("steering_report_extd_2", rosNamespace, qosSettings); //steering_report_extd_2
        canBrakePressureReportPublisher = new CanPublisher("brake_pressure_report", rosNamespace, qosSettings);
        canBrakeReportExtdPublisher = new CanPublisher("brake_report_extd", rosNamespace, qosSettings);
        canBrakeReportExtd2Publisher = new CanPublisher("brake_report_extd_2", rosNamespace, qosSettings);
        canWheelSpeedReportPublisher = new CanPublisher("wheel_speed_report", rosNamespace, qosSettings);

    }
    protected override void OnPublishMessage()
    {
        canWheelPotentiometerPublisher.Publish(new List<double>{
            vehSim.rr_damper_linear_potentiometer, 
            vehSim.rl_damper_linear_potentiometer,
            vehSim.fr_damper_linear_potentiometer,
            vehSim.fl_damper_linear_potentiometer
        });

        canWheelStrainGuagePublisher.Publish(new List<double>{
            vehSim.rr_wheel_load,
            vehSim.rl_wheel_load,
            vehSim.fr_wheel_load,
            vehSim.fr_wheel_load
        });

        canRRTireTempPublisher.Publish(new List<double>{
            vehSim.rr_tire_temperature,
            vehSim.rr_tire_temperature,
            vehSim.rr_tire_temperature,
            vehSim.rr_tire_temperature
        });

        canRLTireTempPublisher.Publish(new List<double>{
            vehSim.rl_tire_temperature,
            vehSim.rl_tire_temperature,
            vehSim.rl_tire_temperature,
            vehSim.rl_tire_temperature
        });

        canFRTireTempPublisher.Publish(new List<double>{
            vehSim.fr_tire_temperature,
            vehSim.fr_tire_temperature,
            vehSim.fr_tire_temperature,
            vehSim.fr_tire_temperature
        });

        canFLTireTempPublisher.Publish(new List<double>{
            vehSim.fl_tire_temperature,
            vehSim.fl_tire_temperature,
            vehSim.fl_tire_temperature,
            vehSim.fl_tire_temperature
        });

        canRRTirePressurePublisher.Publish(new List<double>{
            vehSim.rr_tire_pressure_gauge,
            vehSim.rl_tire_pressure
        });

        canRLTirePressurePublisher.Publish(new List<double>{
            vehSim.rl_tire_pressure_gauge,
            vehSim.rl_tire_pressure
        });

        canFRTirePressurePublisher.Publish(new List<double>{
            vehSim.fr_tire_pressure_gauge,
            vehSim.fr_tire_pressure
        });

        canFLTirePressurePublisher.Publish(new List<double>{
            vehSim.fl_tire_pressure_gauge,
            vehSim.fl_tire_pressure
        });

        canMiscReportPublisher.Publish(new List<double>{
            vehSim.battery_voltage,
            vehSim.safety_switch_state,
            vehSim.mode_switch_state ? 1.0 : 0.0, // probably has roundoff issues
            vehSim.sys_state,
            canMiscReportPublisher.GetCounterNext()// rolling counter missing here?
        });


        canAccelReportPublisher.Publish(new List<double>{
            canAccelReportPublisher.GetCounterNext(), // counter
            vehSim.accel_pedal_output
        });

        canSteerReportPublisher.Publish(new List<double>{ //FIXME 
            canSteerReportPublisher.GetCounterNext(), // counter   
            0.0f, //  static_friction_compensation 
            0.0f //   commanded_steering_rate
        });
        canSteerReportExtdPublisher.Publish(new List<double>{ //FIXME 
            vehSim.steering_wheel_angle,   
            vehSim.steering_wheel_angle, // should be second sensor 
            vehSim.steering_wheel_angle //  average of first two
        }); 

        canSteerReportExtd2Publisher.Publish(new List<double>{ //FIXME
            0.0f, //motor_duty_cycle_cmd 
            0.0f, //motor_duty_cycle_fbk
            0.0f, //motor_current_fbk 
            0.0f, //sbw_ecu_voltage
            0.0f, //sbw_ecu_temp
            0.0f, //sbw_error_code
            0.0f //sbw_motor_torque_estimate
        }); 

        //Debug.Log($"publishing steer angle {vehSim.steering_wheel_angle}");

        canBrakePressureReportPublisher.Publish(new List<double>{
            canBrakePressureReportPublisher.GetCounterNext(), // counter
            vehSim.rear_brake_pressure,
            vehSim.front_brake_pressure
        });

        canBrakeReportExtdPublisher.Publish(new List<double>{ //FIXME
            0.0f, //F_brk_pos_cmd
            0.0f, //F_brk_pos_fbk 
            0.0f, //R_brk_pos_cmd 
            0.0f //R_brk_pos_fbk 
        });

        canBrakeReportExtd2Publisher.Publish(new List<double>{ //FIXME
            0.0f, //f_brake_act_force 
            0.0f //r_brake_act_force
        });

        canWheelSpeedReportPublisher.Publish(new List<double>{
            vehSim.ws_rear_left,
            vehSim.ws_front_right,
            vehSim.ws_front_left,
            vehSim.ws_rear_right
        });
        
    }
    public VehicleDataSimulator vehSim;
    public RaptorSM sm;
    public override void fillMsg()
    {
        msg.Fl_tire_temperature = vehSim.fl_tire_temperature;
        msg.Fl_damper_linear_potentiometer = vehSim.fl_damper_linear_potentiometer;
        msg.Fl_tire_pressure = vehSim.fl_tire_pressure;
        msg.Fl_tire_pressure_gauge = vehSim.fl_tire_pressure_gauge;
        msg.Fl_wheel_load = vehSim.fl_wheel_load;
        msg.Fl_brake_temp = vehSim.fl_brake_temp;

        msg.Fr_tire_temperature = vehSim.fr_tire_temperature;
        msg.Fr_damper_linear_potentiometer = vehSim.fr_damper_linear_potentiometer;
        msg.Fr_tire_pressure = vehSim.fr_tire_pressure;
        msg.Fr_tire_pressure_gauge = vehSim.fr_tire_pressure_gauge;
        msg.Fr_wheel_load = vehSim.fr_wheel_load;
        msg.Fr_brake_temp = vehSim.fr_brake_temp;

        msg.Rl_tire_temperature = vehSim.rl_tire_temperature;
        msg.Rl_damper_linear_potentiometer = vehSim.rl_damper_linear_potentiometer;
        msg.Rl_tire_pressure = vehSim.rl_tire_pressure;
        msg.Rl_tire_pressure_gauge = vehSim.rl_tire_pressure_gauge;
        msg.Rl_wheel_load = vehSim.rl_wheel_load;
        msg.Rl_brake_temp = vehSim.rl_brake_temp;

        msg.Rr_tire_temperature = vehSim.rr_tire_temperature;
        msg.Rr_damper_linear_potentiometer = vehSim.rr_damper_linear_potentiometer;
        msg.Rr_tire_pressure = vehSim.rr_tire_pressure;
        msg.Rr_tire_pressure_gauge = vehSim.rr_tire_pressure_gauge;
        msg.Rr_wheel_load = vehSim.rr_wheel_load;
        msg.Rr_brake_temp = vehSim.rr_brake_temp;

        msg.Battery_voltage = vehSim.battery_voltage;
        msg.Safety_switch_state = vehSim.safety_switch_state;
        msg.Mode_switch_state = vehSim.mode_switch_state;
        msg.Accel_pedal_input = vehSim.accel_pedal_input;
        msg.Accel_pedal_output = vehSim.accel_pedal_output;
        msg.Front_brake_pressure = vehSim.front_brake_pressure;
        msg.Rear_brake_pressure = vehSim.rear_brake_pressure;
        msg.Steering_wheel_angle = vehSim.steering_wheel_angle;
        msg.Steering_wheel_angle_cmd = vehSim.steering_wheel_angle_cmd;
        msg.Steering_wheel_torque = vehSim.steering_wheel_torque;
        msg.Sys_state = vehSim.sys_state;

        msg.Ws_front_left = vehSim.ws_front_left;
        msg.Ws_front_right = vehSim.ws_front_right;
        msg.Ws_rear_left = vehSim.ws_rear_left;
        msg.Ws_rear_right = vehSim.ws_rear_right;

    }
} // end of class
} // end of autonoma namespace