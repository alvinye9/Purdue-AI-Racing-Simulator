# How to Use Racing Simulator

A vehicle simulator for the Dallara AV-24. This simulator improves upon the original AWSIM developed by Autoware, and provides a high-fidelity testbed validated against real-world data. Purdue-AI-Racing-Simulator (PAIRSIM) interfaces with an autonomy stack to allow for testing of autonomous racing software with similar dynamics, sensors, and interface to the real car.

## Documentation
1. How to use racing simulator (this page)

2. [ROS2 Interface](RacingSim/ROS2Interface/index.md)

2. [Vehicle dynamics details](RacingSim/VehicleDynamics/index.md)

3. [Sensor details](RacingSim/Sensors/index.md)

## Downloading Simulator

There are two options to run racing simulations:

1. Download binary (recommended)

    Download the latest PAIRSIM racing simulator [here](https://github.com/alvinye9/Purdue-AI-Racing-Simulator/releases)
    
- Currently only compatible with Linux:

    - Double-click the downloaded executable (.x86_64)

    - The first time you may need to set the executable permissions with ```chmod +x PAIRSIM_vX.X.X.x86_64```

2. From the Unity Editor

    a. To run directly from the Unity editor, clone the repository and make sure you are on the `main` git branch.
    
    b. Follow the instructions in [Getting Started - Setup Unity Project](GettingStarted/SetupUnityProject/index.md).

    c. Download and import `.unitypackage` files

    [Download racetrack assets (unitypackage)](https://purdue0-my.sharepoint.com/:f:/g/personal/ye372_purdue_edu/EgUWzDfIwmtFgmzdY9WvvuwBONARgFEiRZvseWVwBZEYpw?e=lvH8AO){.md-button .md-button--primary}

    e. In Unity Editor, from the menu bar at the top, select `Assets -> Import Package -> Custom Package...` and navigate to each `.unitypackage` file. Ensure that you import each unity package, or the corresponding track will not be available after building your Unity project.
![](RacingSim/Overview/Image_import1.png)
<!-- ![](RacingSim/Overview/Image_import2.png) -->

## Scenario Setup (via GUI)
![](RacingSim/Overview/Image_main_menu.png)
In the main menu, click the 'Scenario Setup' button.
![](RacingSim/Overview/Image_scenario_menu_highlighted.png)

- <span style="color:pink;">PINK</span> Select a saved scenario from the dropdown or create a new one (scenario and vehicle setups are saved by default in your ~/PAIRSIM_config folder (Further Vehicle/Tire Parameter Configuration))

- <span style="color:pink;">PINK</span> Select a saved vehicle setup

- In the scenario setup, you can select number of cars and their colors (check out the custom Purdue Pete skin)
    - You can name your scenario and save it to use the same scenario settings later
    - When you have multiple scenarios saved, you can pick one from the left dropdown menu
    - The most recently saved scenario will be at the top of the dropdown for convenient testing
    - You can overwrite an existing configuration file of the same name

- <span style="color:purple;">PURPLE</span> The option to "Hot start raptor" will start the low level ECU in sys_state = 9 (as explained in [Sensors - Raptor](../Sensors/index.md))

- <span style="color:blue;">BLUE</span> Click "Select a track" dropdown to select a racetrack 

!!! info

    - To run other racelines that PAIRSIM currently does not have a track for, select the "GiantSkidpad" option

    - <span style="color:red;">RED</span> If using GiantSkidpad, enter the Latitude, Longitude, height [m], and yaw [deg] (NED) of the ego-vehicle origin regardless if that origin is in or out of the pitlane.

- <span style="color:orange;">ORANGE</span> If not spawning in pitlane, uncheck "Pitlane" (Skidpad and Monza do not currently support pitlane)

- <span style="color:green;">GREEN</span> Click "Drive" to start simulation with all configurations

- <span style="color:white;">WHITE</span> Select "ROS" for standard operation with autonomy stack, select "TELEOP" to allow control via keyboard. See [ Simulation Operation](#simulation-operation)

## Vehicle Setup (via GUI)
To view or create a setup, click the 'Vehicle Setup' button.
![](RacingSim/Overview/Image_vehicle_setup.png)
- Modify an existing setup or save a new one with the 'Save' button
- To fill in suggested values, press the 'Default' button
- Differential
    - Choose between a spool differential and a Limited Slip Differential (LSD)
- Steering
    - Choose between realistic or ideal steering actuation
    - Set the desired pure steering delay (s) and steering bandwidth (hz)
    - Set the max steering motor angle (deg) and max steering motor rate (deg/s)
    - Set the steering ratio
- Suspension
    - Set the anti-roll bar rates (N/m)
- Brakes
    - Set the brake constant (conversion from kPa to Nm of torque)
- Tires
    - Enable or disable thermal tire effects
    - Set ambient and track temperatures (Celsius)

## Sensor Setup (via GUI)

PAIRSIM has input fields under sensor setup to configure the gaussian noise values of the corresponding actuator/sensor values. (Default values are 0.0)

You can disable high-level or low-level sensors to simulate faults.

You can also configure the perception sensors.

![](RacingSim/Overview/Image_sensor_setup.png)

## Further Vehicle/Tire Parameter configuration (via config files)

### PART I. Loading Setup Configuration Files via GUI
Previously discussed was how to use the dropdowns on the left side of the PAIRSIM GUI to access configuration files (by default saved in ~/PAIRSIM_config)

![](RacingSim/Overview/Image_scenario_menu_1.png)

Note: Configuration files are saved seperately for Scenario/Vehicle/Sensor setups

### PART II. Editing Configuration Files Directly

Configuration file structure is created by default everytime PAIRSIM is run in `~/PAIRSIM_config`
The default file structure (recommended) is created as follows:

```
~/PAIRSIM_config
├── Parameters
├── Scenarios
│   └── Default.dat
├── SensorSets
│   └── Default IAC.dat
└── VehicleSetups
    └── Default AV24.dat
```

`.dat` files are human-readable and can be edited directly. To add a new `test.dat` scenario configuration file, simply create the file as follows:

```
~/PAIRSIM_config
├── Parameters
├── Scenarios
|   ├── Default.dat
│   └── test.dat
├── SensorSets
│   └── Default IAC.dat
└── VehicleSetups
    └── Default AV24.dat
```

Ensure that the "Name": section is the name of the configuration you want to see appear in the PAIRSIM GUI dropdown:
![](RacingSim/Overview/dat_file.png)

Return to the main menu and then go back to Scenario Setup. This will appear in the PAIRSIM GUI as:
![](RacingSim/Overview/config_update.png)

!!! warning

    - If you edit config files externally, you may need to restart PAIRSIM executable or go back to main menu from the Drive scene to see the changes reflect.
    
    - Also, the setup configuration parameters that cannot be changed directly via the GUI are still yet to be implemented; leave those in their default values.

### PART III: Additional Aero/Suspension/Brake/Geometric Parameter Configuration 

The main vehicle parameters can be changed under "Vehicle Setup".
Due to the large number of vehicle/tire parameters, they are not accessible through the PAIRSIM GUI. Instead they need to be directly changed via their configuration files. The default vehicle parameter file structure is as follows: 

```
~/PAIRSIM_config
├── Parameters
│   ├── AeroParams.json
│   ├── BrakeParams.json
│   ├── EngineParams.json
│   ├── FrontAxleTireParams.json
│   ├── GeometricParams.json
│   ├── RearAxleTireParams.json
│   └── SuspensionParams.json
```

!!! warning

    - You may add additional json files but PAIRSIM will only read from the default file names listed above. You must directly change those files to see changes update. To update the simulation with new parameters either return to the main menu or hit Pause -> Restart
    
    - Ensure that you do not use decimal numbers for parameters that were originally integers or vice-versa
    
    - See `VehicleParameters.cs` and `TyreParameters.cs` in the source code of PAIRSIM for parameter units and default values in case you forget.



## Simulation operation
To start a simulation, click 'Drive' from the 'Scenario Setup' menu.
![](RacingSim/Overview/Image_camera1.png)

- 'C' cycles through various camera angles
- Pause button -> Restart button relaunches the entire scenario
- The HUD shows steering motor input, throttle input, brake input, current gear, speed, rpm, ct_state, sys_state, tire tempuratures, and lap times.
- Inject Steering Disturbance button mimics realistic pulse of steering noise seen in real-life AV24 low-level actuator

![](RacingSim/Overview/Image_camera2.png)

### When not recieving input data from ROS environment
- Controls are 'arrow keys' or 'WASD' for throttle, steering and brake (must hold)
- 'tab' for gear up
- 'caps lock' or 'shift' for gear down

### When running external autonomy stack in ROS environment
- You can continue with standard AV24 startup proceedure
    - If you selected 'Raptor Hot Start' then you can immediately begin driving
    - If you did not, vehicle requires an orange flag and a ct_state = 5, followed by removing the orange flag to begin driving.
    - Low-level state machine is described in more detail in the [Sensors - Raptor](RacingSim/Sensors/index.md) section.

### Running with NPC Vehicle

PAIRSIM supports multi-vehicle simulation, with the adversarial vehicle controlled either via the high-level ROS2 interface (`/npc/vehicle_data`, `/npc/powertrain_data`, `/npc/race_control`, `/npc/vehicle_inputs`, `/npc/to_raptor`) OR (`/planning/ghost_vehicle/heading`, `/planning/ghost_vehicle/measurement`).

 `/planning/ghost_vehicle/heading` is a `std_msgs/Float32.msg` msg type containing the current ENU heading [deg] of the desired "ghost" vehicle.
 
 `/planning/ghost_vehicle/measurement` is a `std_msgs/Point32.msg` msg type containing the centroid position (x,y) of the desired "ghost" vehicle in the ego vehicle's local frame using conventional vehicle dynamics CRS (+x forward, +y left). The adversarial vehicle's +z direction will match the ego vehicle's by default and will be automatically rotated about the longitudinal axis in order to conform to the banked surface.

### Visualizing Raceline

PAIRSIM supports raceline visualization, where the raceline being sent by the autonomy stack will appear as a series of green dots. Toggle on/off on the top left side of the screen (see below). The raceline must be sent with the topic name `/planning/front_path/offset_path`, which is a `nav_msgs/Path` msg type containing a raceline represented as a series of `geometry_msgs/Pose` msgs in the ego vehicle's local frame using conventional vehicle dynamics CRS. To reduce lag, it is best to pass a local portion of the raceline rather than the entire raceline (no more than 100 poses, and the number of green dots will NOT necessarily match the number of poses).

![](RacingSim/Overview/Image_waypoints.png)

### Camera Sensors

The publishers for each of the 6 on-board camera sensors will publish a `sensor_msgs/Image.msg` if previously enabled in the Sensor Setup menu. All 6 cameras may consume a lot of resources so it is recommended you enable only a few at a time.

![](RacingSim/Overview/Image_cameras.png)
 
## Copyright and License

Copyright 2024 Purdue AI Racing

All code is licensed under the Apache 2.0 License. All assets are distributed under the CC BY-NC License.
