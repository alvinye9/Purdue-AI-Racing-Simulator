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

public class CarController : MonoBehaviour
{
    public VehicleParameters vehicleParams;
    public Rigidbody carBody;
    public float throttleCmd, brakeCmd, steerAngleCmd; // controller inputs
    public float steerAngleApplied, steerAngleAppliedPrev; // after the actuator dyn
    public float brakeApplied, brakeAppliedPrev ,thrApplied, thrAplliedPrev;  // after the actuator dyn
    private float[] steerAngleCmdBuf;
    private float[] steerAngleCmdBufPrev;
    private float[] brakeCmdBuf;
    private float[] brakeCmdBufPrev;
    public bool gearUp, gearDown , gearUpPrev, gearDownPrev;
    public float omegaRR,omega1,omega2;
    public int gear = 1;
    public float TEngine,TEnginePrev,rpmEngine;
    public float TAxle,TBrake;
    public Vector3 frontDownforce,frontDownforceGlobal;
    public Vector3 rearDownforce,rearDownforceGlobal;
    public Vector3 dragForce,dragForceGlobal;
    public Vector3 bankingForce, bankingForceGlobal;
    public Vector3 V;
    public bool physicalActuator = false;
    public VehicleState vehicleState;
    public Powertrain powertrain;
    public NoiseGenerator steerNoiseGenerator;
    public NoiseGenerator brakeNoiseGenerator;
    public NoiseGenerator throttleNoiseGenerator;
    // private string dataFilePath; //for changing gaussian noise on-the-fly
    void getState()
    {
        // take values from unity system and transform into VD coords
        V = HelperFunctions.unity2vehDynCoord( transform.InverseTransformDirection(carBody.velocity) );

        Vector3 pose = HelperFunctions.unity2vehDynCoord(-transform.eulerAngles);
        pose.x = Mathf.Abs(pose.x) > 180f ? pose.x - Mathf.Sign(pose.x)*360f : pose.x;
        pose.y = Mathf.Abs(pose.y) > 180f ? pose.y - Mathf.Sign(pose.y)*360f : pose.y;
        pose.z = Mathf.Abs(pose.z) > 180f ? pose.z - Mathf.Sign(pose.z)*360f : pose.z;
        
        Vector3 localAngularvelocity = transform.InverseTransformDirection(carBody.angularVelocity);
        Vector3 carAngularVel = HelperFunctions.unity2vehDynCoord(-localAngularvelocity);
        Vector3 position = HelperFunctions.unity2vehDynCoord(transform.position);
        vehicleState.pos[0] = position.x;
        vehicleState.pos[1] = position.y;
        vehicleState.pos[2] = position.z;
        vehicleState.V[0] = V.x;
        vehicleState.V[1] = V.y;
        vehicleState.V[2] = V.z;
        vehicleState.rollRate = carAngularVel.x;
        vehicleState.pitchRate = carAngularVel.y;
        vehicleState.yawRate = carAngularVel.z;
        vehicleState.roll = pose.x;
        vehicleState.pitch = pose.y;
        vehicleState.yaw = pose.z;
        vehicleState.omega[0] = omega1;
        vehicleState.omega[1] = omega2;
        vehicleState.omega[2] = omegaRR;
        vehicleState.omega[3] = omegaRR;    
    }

    void calcWheelStAngle()
    {   
        if (physicalActuator) 
        {
            // Debug.Log("Physical Actuator Used for Calculating Steering Angle");
            steerAngleCmdBufPrev = steerAngleCmdBuf;
            steerAngleCmdBuf = HelperFunctions.pureDelay(steerAngleCmd,steerAngleCmdBufPrev, vehicleParams.steeringDelay);
            steerAngleApplied = steerAngleCmdBuf[vehicleParams.steeringDelay-1];
            // Apply low pass filter and rate limiting to prevent abrupt changes in the applied steering angle
            steerAngleApplied = HelperFunctions.lowPassFirstOrder(steerAngleApplied,steerAngleAppliedPrev,vehicleParams.steeringBandwidth);
            steerAngleApplied = HelperFunctions.rateLimit(steerAngleApplied, steerAngleAppliedPrev , Mathf.Abs(vehicleParams.steeringRate/vehicleParams.steeringRatio)); //steerAngleApplied [rad]
            
            // Add Gaussian noise
            float steerNoise = (float)steerNoiseGenerator.NextGaussian();
            steerAngleApplied += steerNoise;

            // Debug.Log("Steer Noise: " + steerNoise);

            steerAngleAppliedPrev = steerAngleApplied;
        }
        else
        {
            Debug.Log("Physical Actuator NOT used for Calculating Steering Angle");
            steerAngleApplied = steerAngleCmd;
        }
    }

    void calcBrakeTorque()
    {   
        if (physicalActuator)
        {
            brakeCmdBufPrev = brakeCmdBuf;
            brakeCmdBuf = HelperFunctions.pureDelay(brakeCmd,brakeCmdBufPrev, vehicleParams.brakeDelay); // buffer the incoming commands
            brakeApplied = brakeCmdBuf[vehicleParams.brakeDelay-1]; // select the latest of the buffer for the delay
            brakeApplied = HelperFunctions.lowPassFirstOrder(brakeApplied,brakeAppliedPrev, vehicleParams.brakeBandwidth);
            brakeApplied = HelperFunctions.rateLimit(brakeApplied, brakeAppliedPrev , vehicleParams.brakeRate); //brakeApplied [%]

            // Add Gaussian noise
            float brakeNoise = (float)brakeNoiseGenerator.NextGaussian();
            brakeApplied += brakeNoise;

            brakeAppliedPrev = brakeApplied;
            TBrake = brakeApplied * vehicleParams.brakeKpaToNm;
        }
        else
        {   brakeApplied = brakeCmd;
            TBrake = brakeCmd * vehicleParams.brakeKpaToNm;
        }
    }

    void gearShifts()
    {
        if (gearUp && !gearUpPrev && gear < vehicleParams.numGears)
            gear++;
        if (gearDown && !gearDownPrev && gear > 0)
            gear--;  

        gear = Mathf.Clamp(gear,1,vehicleParams.numGears);    

        gearUpPrev = gearUp;
        gearDownPrev = gearDown;
    }

    void calcEngineTorque()
    {        
        //TEngine = throttleCmd * HelperFunctions.lut1D(VehParams.engineMapLen,VehParams.engineMapRevs,VehParams.engineMapTorque,rpmEngine);
        float saturatedRpmEngine = Mathf.Clamp(rpmEngine,vehicleParams.minEngineMapRpm,vehicleParams.maxEngineRpm);
        thrApplied = HelperFunctions.lowPassFirstOrder(throttleCmd,thrAplliedPrev, vehicleParams.throttleBandwidth);
        thrApplied = HelperFunctions.rateLimit(thrApplied, thrAplliedPrev , vehicleParams.throttleRate);

        // Add Gaussian noise
        float throttleNoise = (float)throttleNoiseGenerator.NextGaussian();
        thrApplied += throttleNoise;
            
        thrAplliedPrev = thrApplied;

        float torquePercentage = HelperFunctions.lut1D(vehicleParams.numPointsThrottleMap,
                                vehicleParams.throttleMapInput, vehicleParams.throttleMapOutput, thrApplied);

        TEngine = torquePercentage *( (float)(vehicleParams.enginePoly[0]*Mathf.Pow(saturatedRpmEngine,2) 
                + vehicleParams.enginePoly[1]*saturatedRpmEngine + vehicleParams.enginePoly[2]) + vehicleParams.engineFrictionTorque);
        if (rpmEngine > 1050)
        {
            TEngine = TEngine - vehicleParams.engineFrictionTorque;
        }
        if (TEngine > TEnginePrev )
        {
            TEngine = HelperFunctions.rateLimit(TEngine, TEnginePrev , vehicleParams.torqueRate);
        }
        TAxle = TEngine * vehicleParams.gearRatio[gear-1];
        TEnginePrev= TEngine;
    }
    void applyAeroForces()
    {
        float aeroForce = -0.6f*vehicleParams.Af*V.x*V.x;
        frontDownforce.y  = aeroForce*vehicleParams.ClF;
        rearDownforce.y = aeroForce*vehicleParams.ClR;
        dragForce.z = aeroForce*vehicleParams.Cd*Mathf.Sign(V.x);
        frontDownforceGlobal = transform.TransformDirection(frontDownforce);
        rearDownforceGlobal  = transform.TransformDirection(rearDownforce);
        dragForceGlobal = transform.TransformDirection(dragForce);

        carBody.AddForceAtPosition(frontDownforceGlobal, transform.TransformPoint(vehicleParams.frontDownforcePos));
        carBody.AddForceAtPosition(rearDownforceGlobal, transform.TransformPoint(vehicleParams.rearDownforcePos));
        carBody.AddForceAtPosition(dragForceGlobal, transform.TransformPoint(vehicleParams.dragForcePos));
    } 

    void applyBankingForces() //SHOULD ONLY BE USED ON MAPS WITHOUT accurate BANKING 
    {
        // float theta = 0.16057f; //9.2 degrees banking
        float theta = 0f;
        float g = 9.81f;
        // float mu = 0.7f;
        float forceApplied = (float)(-1.0f * (vehicleParams.mass * g * Math.Sin(theta))); // - (vehicleParams.mass * g * mu * Math.Cos(theta))); //force needing to simulate banking
        bankingForce.x = forceApplied; //applied in left lateral direction to car's motion
        bankingForceGlobal = transform.TransformDirection(bankingForce); //transform to global frame
        if( GameManager.Instance.Settings.myTrackParams.TrackName.Equals("IMS")){
            // Debug.Log("Applying Banking Force: " + forceApplied);
            carBody.AddForceAtPosition(bankingForceGlobal, transform.TransformPoint(vehicleParams.centerOfMass));
        }
    } 

    void Start()
    {
        //Gaussian Noise Simulators
        float steerMean = GameManager.Instance.Settings.mySensorSet.steerMean;
        float steerVariance = GameManager.Instance.Settings.mySensorSet.steerVariance;
        int steerSeed = GameManager.Instance.Settings.mySensorSet.steerSeed;
        steerNoiseGenerator = new NoiseGenerator(steerMean, steerVariance, steerSeed);

        float brakeMean = GameManager.Instance.Settings.mySensorSet.brakeMean;
        float brakeVariance = GameManager.Instance.Settings.mySensorSet.brakeVariance;
        int brakeSeed = GameManager.Instance.Settings.mySensorSet.brakeSeed;
        brakeNoiseGenerator = new NoiseGenerator(brakeMean, brakeVariance, brakeSeed);

        float throttleMean = GameManager.Instance.Settings.mySensorSet.throttleMean;
        float throttleVariance = GameManager.Instance.Settings.mySensorSet.throttleVariance;
        int throttleSeed = GameManager.Instance.Settings.mySensorSet.throttleSeed;
        throttleNoiseGenerator = new NoiseGenerator(throttleMean, throttleVariance, throttleSeed);

        string aeroConfigFileName = "AeroParams.json";
        string fullAeroPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + aeroConfigFileName);
        CheckAndCreateDefaultFile(fullAeroPath, defaultAeroParams);

        string suspensionConfigFileName = "SuspensionParams.json";
        string fullSuspensionPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + suspensionConfigFileName);
        CheckAndCreateDefaultFile(fullSuspensionPath, defaultSuspensionParams);

        string brakeConfigFileName = "BrakeParams.json";
        string fullBrakePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + brakeConfigFileName);
        CheckAndCreateDefaultFile(fullBrakePath, defaultBrakeParams);

        string engineConfigFileName = "EngineParams.json";
        string fullEnginePath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + engineConfigFileName);
        CheckAndCreateDefaultFile(fullEnginePath, defaultEngineParams);

        string geometricConfigFileName = "GeometricParams.json";
        string fullGeometricPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + geometricConfigFileName);
        CheckAndCreateDefaultFile(fullGeometricPath, defaultGeometricParams);

        LoadAeroParametersFromJson(fullAeroPath);
        LoadSuspensionParametersFromJson(fullSuspensionPath);
        LoadBrakeParametersFromJson(fullBrakePath);
        LoadEngineParametersFromJson(fullEnginePath);
        //Steering need not be loaded in from json since it's handled in Vehicle Setup
        LoadGeometricParametersFromJson(fullGeometricPath);


        UpdateVehicleParamsFromMenu();

        initializeBuffers();
        carBody = GetComponent<Rigidbody>();
        carBody.mass = vehicleParams.mass;
        carBody.inertiaTensor = vehicleParams.Inertia;
        carBody.centerOfMass = vehicleParams.centerOfMass;
    }


    void UpdateVehicleParamsFromMenu()
    {
        vehicleParams.brakeKpaToNm = GameManager.Instance.Settings.myVehSetup.BrakeConstant;
        vehicleParams.kArbFront = GameManager.Instance.Settings.myVehSetup.FrontRollBarRate;
        vehicleParams.kArbRear = GameManager.Instance.Settings.myVehSetup.RearRollBarRate;
        vehicleParams.maxSteeringAngle = (GameManager.Instance.Settings.myVehSetup.MaxSteeringAngle > 0) ? GameManager.Instance.Settings.myVehSetup.MaxSteeringAngle : 200f;
        vehicleParams.rearSolidAxle = !GameManager.Instance.Settings.myVehSetup.IsLSD;
        vehicleParams.steeringBandwidth = (GameManager.Instance.Settings.myVehSetup.SteeringBW > 0) ? GameManager.Instance.Settings.myVehSetup.SteeringBW : 5f;
        vehicleParams.steeringDelaySec = (GameManager.Instance.Settings.myVehSetup.SteeringDelay > 0) ? GameManager.Instance.Settings.myVehSetup.SteeringDelay : 0.01f;
        vehicleParams.steeringRate = (GameManager.Instance.Settings.myVehSetup.MaxSteeringRate > 0) ? GameManager.Instance.Settings.myVehSetup.MaxSteeringRate : 360f;
        vehicleParams.steeringRatio = -1f*Mathf.Abs(GameManager.Instance.Settings.myVehSetup.SteeringRatio);
        vehicleParams.tAmb = GameManager.Instance.Settings.myVehSetup.AmbientTemp;
        vehicleParams.tTrack = GameManager.Instance.Settings.myVehSetup.TrackTemp;
        physicalActuator = !GameManager.Instance.Settings.myVehSetup.IsIdealSteering;
        vehicleParams.calcDepVars();
    }

    void initializeBuffers()
    {
        steerAngleCmdBuf = new float[vehicleParams.steeringDelay];
        steerAngleCmdBufPrev = new float[vehicleParams.steeringDelay];
        brakeCmdBuf = new float[vehicleParams.brakeDelay];
        brakeCmdBufPrev = new float[vehicleParams.brakeDelay];
    }
    
    void Update() 
    {         
        Debug.DrawRay(transform.TransformPoint(vehicleParams.frontDownforcePos), frontDownforceGlobal*0.001f, Color.yellow);
        Debug.DrawRay(transform.TransformPoint(vehicleParams.rearDownforcePos), rearDownforceGlobal*0.001f, Color.red);
        Debug.DrawRay(transform.TransformPoint(vehicleParams.dragForcePos), dragForceGlobal*0.001f, Color.green);
    }
    
    void FixedUpdate()
    {
        getState();
        gearShifts();
        calcWheelStAngle();
        calcBrakeTorque();
        calcEngineTorque();
        applyAeroForces();
        // applyBankingForces();
    }

    public float GetSpeed()
    {
        float[] v = vehicleState.V;
        return new Vector3(v[0], v[1], v[2]).magnitude;
    }

    float[] ConvertDoubleArrayToFloatArray(double[] doubleArray)
    {
        return Array.ConvertAll(doubleArray, item => (float)item);
    }

// ============ Configuration File Functions ===============
    [System.Serializable]
    public class AeroParametersConfig
    {
        public double Af;
        public double ClF;
        public double ClR;
        // public Vector3 frontDownforcePos; //dependent variable
        // public Vector3 rearDownforcePos; //dependent variable
        public double Cd;
        public Vector3 dragForcePos;
    }
    private AeroParametersConfig defaultAeroParams = new AeroParametersConfig
    {
        Af = 1.0, 
        ClF = 0.65, 
        ClR = 1.18,
        // frontDownforcePos = new Vector3(0.0f, 0.0f, 1.7f), //dependent variable
        // rearDownforcePos = new Vector3(0.0f, 0.0f, -1.3f), //dependent variable
        Cd = 0.8581, 
        dragForcePos = new Vector3(0.0f, 0.0f, 0.0f) 
    };
    void LoadAeroParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            AeroParametersConfig Params = JsonUtility.FromJson<AeroParametersConfig>(jsonData);
            ApplyAeroParameters(Params);
        }
        else
        {
            Debug.LogError("Aerodynamic parameters file not found: " + fullPath);
        }
    }
    void ApplyAeroParameters(AeroParametersConfig config)
    {
        vehicleParams.Af = (float)config.Af;
        vehicleParams.ClF = (float)config.ClF;
        vehicleParams.ClR = (float)config.ClR;
        // vehicleParams.frontDownforcePos = config.frontDownforcePos; //dependent variable
        // vehicleParams.rearDownforcePos = config.rearDownforcePos; //dependent variable
        vehicleParams.Cd = (float)config.Cd;
        vehicleParams.dragForcePos = config.dragForcePos;
    }

    [System.Serializable]
    public class SuspensionParametersConfig
    {
        // public double kArbFront; //configured in Vehicle Setup
        // public double kArbRear; //configured in Vehicle Setup
        public double kSpring;
        public double cDamper;
        public double lSpring;
    }
    private SuspensionParametersConfig defaultSuspensionParams = new SuspensionParametersConfig
    {
        // kArbFront = 463593.0, //configured in Vehicle Setup
        // kArbRear = 358225.0, //configured in Vehicle Setup
        kSpring = 200000.0, 
        cDamper = 8000.0,
        lSpring = 0.3
    };

    void LoadSuspensionParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            SuspensionParametersConfig Params = JsonUtility.FromJson<SuspensionParametersConfig>(jsonData);
            ApplySuspensionParameters(Params);
        }
        else
        {
            Debug.LogError("Suspension parameters file not found: " + fullPath);
        }
    }

    void ApplySuspensionParameters(SuspensionParametersConfig config)
    {
        // vehicleParams.kArbFront = (float)config.kArbFront; //configured in Vehicle Setup
        // vehicleParams.kArbRear = (float)config.kArbRear; //configured in Vehicle Setup
        vehicleParams.kSpring = (float)config.kSpring; 
        vehicleParams.cDamper = (float)config.cDamper;
        vehicleParams.lSpring = (float)config.lSpring;

    }

    [System.Serializable]
    public class BrakeParametersConfig
    {
        // public int brakeDelay; //dependent variable
        public double brakeDelaySec;
        public double maxBrakeKpa;
        // public double brakeKpaToNm; //configured in Vehicle Setup (Brake Constant)
        public double brakeBias;
        public double brakeBandwidth;
        // public double brakeRate;  //dependent variable
    }
    private BrakeParametersConfig defaultBrakeParams = new BrakeParametersConfig
    {
        // brakeDelay = 20, // dependent variable
        brakeDelaySec = 0.04, 
        maxBrakeKpa = 6000.0, 
        // brakeKpaToNm = 1.0, //configured in Vehicle Setup (Brake Constant)
        brakeBias = 0.50, //AV21 default was 0.54
        brakeBandwidth = 5.0, 
        // brakeRate = 180000.0  //dependent variable
    };

    void ApplyBrakeParameters(BrakeParametersConfig config)
    {
        // vehicleParams.brakeDelay = config.brakeDelay; //dependent variable
        vehicleParams.brakeDelaySec = (float)config.brakeDelaySec;
        vehicleParams.maxBrakeKpa = (float)config.maxBrakeKpa; 
        // vehicleParams.brakeKpaToNm = (float)config.brakeKpaToNm; //configured in Vehicle Setup (Brake Constant)
        vehicleParams.brakeBias = (float)config.brakeBias;
        vehicleParams.brakeBandwidth = (float)config.brakeBandwidth;
        // vehicleParams.brakeRate = (float)config.brakeRate; //dependent variable 

    }

    void LoadBrakeParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            BrakeParametersConfig Params = JsonUtility.FromJson<BrakeParametersConfig>(jsonData);
            ApplyBrakeParameters(Params);
        }
        else
        {
            Debug.LogError("Brake parameters file not found: " + fullPath);
        }
    }

    [System.Serializable]
    public class EngineParametersConfig
    {
        public int numGears;
        public double[] gearRatio;
        public double[] enginePoly;
        public double throttleBandwidth;
        public double throttleRate;
        public double torqueRate;
        public int numPointsThrottleMap;
        public double[] throttleMapInput;
        public double[] throttleMapOutput;
        public double maxEngineRpm;
        public double minEngineMapRpm;
        public double engineFrictionTorque;
        public double engineInertia;
        public double frontDifferentialDamping;
        public double rearDifferentialDamping;
        // public bool rearSolidAxle; //configured in Vehicle Setup (is LSD))
    }

    private EngineParametersConfig defaultEngineParams = new EngineParametersConfig
    {
        numGears = 6,
        gearRatio = new double[] {
            8.75, 
            5.625, 
            4.1427, 
            3.3462, 
            2.88, 
            2.666667},
        enginePoly = new double[] {
            -0.0000456821, 
            0.489643, 
            -754.247},
        throttleBandwidth = 1.0, 
        throttleRate = 50.0, 
        torqueRate = 300.0,  
        numPointsThrottleMap = 4,
        throttleMapInput = new double[] {
            0.0, 
            0.2, 
            0.5, 
            1.0},
        throttleMapOutput = new double[] {
            0.0,
            0.4,
            0.7, 
            1.0},
        maxEngineRpm = 7500.0,
        minEngineMapRpm = 3000.0,
        engineFrictionTorque = 30.0,
        engineInertia = 0.2,
        frontDifferentialDamping = 0.0,
        rearDifferentialDamping = 5.0,
        // rearSolidAxle = false //configured in Vehicle Setup (is LSD))
    };

    void LoadEngineParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            EngineParametersConfig Params = JsonUtility.FromJson<EngineParametersConfig>(jsonData);
            ApplyEngineParameters(Params);
        }
        else
        {
            Debug.LogError("Engine parameters file not found: " + fullPath);
        }
    }

    void ApplyEngineParameters(EngineParametersConfig config)
    {
        vehicleParams.numGears = config.numGears;
        vehicleParams.gearRatio = ConvertDoubleArrayToFloatArray(config.gearRatio);
        vehicleParams.enginePoly = config.enginePoly;
        vehicleParams.throttleBandwidth = (float)config.throttleBandwidth;
        vehicleParams.throttleRate = (float)config.throttleRate; 
        vehicleParams.torqueRate = (float)config.torqueRate; 
        vehicleParams.numPointsThrottleMap = config.numPointsThrottleMap;
        vehicleParams.throttleMapInput = ConvertDoubleArrayToFloatArray(config.throttleMapInput);
        vehicleParams.throttleMapOutput = ConvertDoubleArrayToFloatArray(config.throttleMapOutput);
        vehicleParams.maxEngineRpm = (float)config.maxEngineRpm;
        vehicleParams.minEngineMapRpm = (float)config.minEngineMapRpm;
        vehicleParams.engineFrictionTorque = (float)config.engineFrictionTorque;
        vehicleParams.engineInertia = (float)config.engineInertia;
        vehicleParams.frontDifferentialDamping = (float)config.frontDifferentialDamping;
        vehicleParams.rearDifferentialDamping = (float)config.frontDifferentialDamping;
        // vehicleParams.rearSolidAxle = config.rearSolidAxle; //configured in Vehicle Setup (is LSD))

    }

    [System.Serializable]
    public class GeometricParametersConfig
    {
        public double mass;
        public Vector3 centerOfMass; 
        public Vector3 Inertia;
        public double lf;
        public double lr;
        public double twf;
        public double twr;
        // public Vector3 w1pos; //dependent variable
        // public Vector3 w2pos; //dependent variable
        // public Vector3 w3pos; //dependent variable
        // public Vector3 w4pos; //dependent variable
    }

    private GeometricParametersConfig defaultGeometricParams = new GeometricParametersConfig
    {
        mass = 790.0, 
        centerOfMass = new Vector3(0f, 0.2f, 0f),
        Inertia = new Vector3(550f, 800f, 265f), 
        lf = 1.7, 
        lr = -1.3, 
        twf = 0.838,
        twr = 0.79,
        // w1pos = new Vector3(-0.838f, 0.626f, 1.7f), //dependent variable
        // w2pos = new Vector3(0.838f, 0.626f, 1.7f), //dependent variable
        // w3pos = new Vector3(-0.79f, 0.626f, -1.3f), //dependent variable
        // w4pos = new Vector3(0.79f, 0.626f, -1.3f) //dependent variable
    };

    void LoadGeometricParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            GeometricParametersConfig Params = JsonUtility.FromJson<GeometricParametersConfig>(jsonData);
            ApplyGeometricParameters(Params);
        }
        else
        {
            Debug.LogError("Geometric parameters file not found: " + fullPath);
        }
    }

    void ApplyGeometricParameters(GeometricParametersConfig config)
    {
        vehicleParams.mass = (float)config.mass; 
        vehicleParams.centerOfMass = config.centerOfMass;
        vehicleParams.Inertia = config.Inertia; 
        vehicleParams.lf = (float)config.lf; 
        vehicleParams.lr = (float)config.lr; 
        vehicleParams.twf = (float)config.twf; 
        vehicleParams.twr = (float)config.twr;
        // vehicleParams.w1pos = config.w1pos; //dependent variable
        // vehicleParams.w2pos = config.w2pos; //dependent variable
        // vehicleParams.w3pos = config.w3pos; //dependent variable
        // vehicleParams.w4pos = config.w4pos; //dependent variable

    }

    //check to see if params file already exists, if not write the file
    void CheckAndCreateDefaultFile<T>(string filePath, T defaultParams)
    {
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string json = JsonUtility.ToJson(defaultParams, true);
            File.WriteAllText(filePath, json);
        }
    }
}
