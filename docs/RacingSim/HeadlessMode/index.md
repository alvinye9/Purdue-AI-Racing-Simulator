# How to Run PAIRSIM in Headless Mode (Batch Mode)

Headless mode is useful for when the application needs to run without a GUI.

## Downloading Headless Simulator 

There are two options to run a headless simulation:

!!! info

    - In your working terminal make sure you DO NOT source ros2, or the ros2-for-unity plugin will not work


1. Download binary (only compatible with Linux) (recommended)

    a. Download the latest PAIRSIM headless build [here](https://github.com/alvinye9/Purdue-AI-Racing-Simulator/releases)

    b. Navigate to the directory containing the `.x86_64` executable file

    c. The first time you may need to set the executable permissions with ```chmod +x PAIRSIM_batch.x86_64```

    d. Configure your desired parameters in the configuration files `~PAIRSIM_config/`, as discussed [below](index#important-configuration-parameters)
    
    e. Run ```./PAIRSIM_batch.x86_64 -batchmode -nographics``` 

2. Build and Run From the Unity Project

    a. Finish setting up the project as detailed in [Step 2](../../index#downloading-simulator)

    b. Close out of Unity Editor
    
    c. Navigate to the project's root directory, run the convenience script ```headless_build.sh```. 

    d. Configure your desired parameters in the configuration files `~PAIRSIM_config/`, as discussed [below](index#important-configuration-parameters)

    e. The build is now located in the `./Build/Linux` directory. Run it with ```./Build/Linux/PAIRSIM_batch.x86_64 -batchmode -nographics```

## Important Configuration Parameters

For an overview on how PAIRSIM configuration files work, see [here](../../index#part-ii-editing-configuration-files-directly).

The following configuration parameters must be set for the headless simulation to work properly:

```
~/PAIRSIM_config
└── Scenarios
    └── Default.data
```

In the Scenarios configuration file ensure that the following value is set:

```
"ModeSwitchState": true,
```

This ensures that PAIRSIM uses an external race control source, as described [here](index#headless-race-control)

### Map List

To select your racetrack, ensure that the following value is set to your desird track:

```
"SelectedTrack": 0,
```

The following is the list of racetrack enumerations:

|value|racetrack|
|:--|:--|
|0| Autodromo Nazionale Monza |
|1| Giant Skidpad |
|2| Las Vegas Motor Speedway |
|3| Autodromo Nazionale Monza (Low-Poly) |
|4| Kentucky Speedway |
|5| Indianapolis Motor Speedway (No Banking)|
|6| Indianapolis Motor Speedway |

If using Giant Skidpad, ensure that the LAT/LON/HEIGHT/YAW origin values are set in the configuration file 

```
  "lat_input": 0.0,
  "lon_input": 0.0,
  "height_input": 0.0,
  "yaw_input": 0.0,
```


## Headless Race Control

When running headless, the PAIRSIM GUI can no longer be used to issue race control flags, and instead needs to take race control commands from an external flag spoofer via the following ROS2 topics: `/flag_spoofer/vehicle_flag` and `/flag_spoofer/track_flag`. Both of these topics are [std_msgs/UInt8.msg](http://docs.ros.org/en/melodic/api/std_msgs/html/msg/UInt8.html). To enumerate flags, PAIRSIM uses Marelli race control by default.


### Track Flag List

The following are the track flag enumerations:

|value|track flag|
|:--|:--|
|3| Red |
|9| Full-Course Yellow |
|1| Green |
|37| Waving Green |
|40| G40 |
|60| G60 |
|80| G80 |
|100| G100 |
|120| G120 |
|130| G130 |
|140| G140 |
|145| G145 |
|150| G150 |
|155| G155 |
|160| G160 |
|165| G165 |
|170| G170 |
|175| G175 |
|180| G180 |
|185| G185 |
|190| G190 |


### Vehicle Flag List

The following are the vehicle flag enumerations:

|value|vehicle flag|
|:--|:--|
|0| None |
|25| Orange |
|7| Yellow |
|34| Stop |
|4| Black |
|33| Engine Kill |
|36| Attacker |
|35| Defender |

 
## Copyright and License

Copyright 2024 Purdue AI Racing

All code is licensed under the Apache 2.0 License. All assets are distributed under the CC BY-NC License.
