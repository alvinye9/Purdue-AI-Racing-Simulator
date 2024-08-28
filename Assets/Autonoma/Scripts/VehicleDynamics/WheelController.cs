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
using System.IO;
using System;


public class WheelController : MonoBehaviour
{       
    public Rigidbody carBody;
    public CarController carController;
    public TyreParameters axleTyreParams;
    public GameObject mesh;
    public WheelController otherSideWheel;
    public bool wheelFL;
    public bool wheelFR;
    public bool wheelRL;
    public bool wheelRR;
    public float hitDistance = 0.55f;
    public float currLength = 0.24f;
    private float lastLength;
    public float springDeflection, springVelocity;
    public float springForce, damperForce,arbForce, Fx, Fy, Fz;
    public float sy, sx, S , syDir, sxDir, tanSy, tanSyPrev, sxPrev;
    public Vector3 wheelVelocity;
    public bool isHit;
    private Vector3 Ftotal;
    public float wheelStAngle, omega, omegaDot, wheelAngle, driveTorque;
    public float currTyreTemp,q1,q2,q3,q4,thermalScaling; 
    public float DxEffective,DyEffective;
    void setWheelPos()
    {
        if (wheelFL)
            transform.localPosition = carController.vehicleParams.w1pos;
        if (wheelFR)
            transform.localPosition = carController.vehicleParams.w2pos;
        if (wheelRL)
            transform.localPosition = carController.vehicleParams.w3pos;
        if (wheelRR)
            transform.localPosition = carController.vehicleParams.w4pos;  
    }
    void getSteering()
    {
        if (wheelFL || wheelFR)
        {
            wheelStAngle = carController.steerAngleApplied;
        }
        else
        {
            wheelStAngle = 0f;
        }
    }
    void calcOmega()
    {   
        float brakeBias; 
        float wheelInertia;
        if (wheelFL || wheelFR)
        {
            wheelInertia = axleTyreParams.wheelInertia;
            brakeBias = carController.vehicleParams.brakeBias;
        }
        else
        {   
            float gr = carController.vehicleParams.gearRatio[carController.gear-1];
            wheelInertia = axleTyreParams.wheelInertia + 0.5f*gr*gr*carController.vehicleParams.engineInertia;
            brakeBias = 1f-carController.vehicleParams.brakeBias;
        }
        // omegaDot = (driveTorque - 0.5f*carController.TBrake*brakeBias*Mathf.Sign(omega)
        omegaDot = (driveTorque - carController.TBrake*brakeBias*Mathf.Sign(omega)
                           - Fx*axleTyreParams.tyreRadius
                           - axleTyreParams.rollResForce*axleTyreParams.tyreRadius*Mathf.Sign(omega) )/wheelInertia;

        if (Mathf.Abs(omega) < 0.5f && carController.TBrake > 10f && driveTorque < 1f)
        {
            omega = 0;
        }
        else
        {
            omega += omegaDot*Time.fixedDeltaTime;
        }

        if (wheelRL || wheelRR)
        {
            omega = Mathf.Clamp(omega,-50f,carController.vehicleParams.maxEngineRpm*(2*Mathf.PI)/60f/carController.vehicleParams.gearRatio[carController.gear-1]);
        }
    }
    void calcArbForce()
    {
        float arbStiffness;
        if (wheelFL || wheelFR)
        {
            arbStiffness = carController.vehicleParams.kArbFront;
        }
        else
        {
            arbStiffness = carController.vehicleParams.kArbRear;
        }
        arbForce = arbStiffness * (this.springDeflection - otherSideWheel.springDeflection);
    }

    void calcFz()
    {
        lastLength = currLength;
        currLength = hitDistance -  axleTyreParams.tyreRadius;
        
        springDeflection = carController.vehicleParams.lSpring - currLength;
        springForce =  carController.vehicleParams.kSpring * springDeflection;

        springVelocity = (lastLength - currLength) / Time.fixedDeltaTime;
        damperForce =  carController.vehicleParams.cDamper * springVelocity;
        calcArbForce();
        Fz = springForce + damperForce + arbForce; 

        Fz = Mathf.Clamp(Fz,0f,Fz);
    }

    void calcSySx()
    {
        // calculate non relax len versions of slips for validation purposes
        syDir = Mathf.Abs(wheelVelocity.z ) > 0.1f ? Mathf.Atan(-wheelVelocity.x / Mathf.Abs(wheelVelocity.z ) ) : syDir;
        sxDir = Mathf.Abs(wheelVelocity.z ) > 0.1f ? (omega*axleTyreParams.tyreRadius-wheelVelocity.z)/Mathf.Abs(wheelVelocity.z ) : sxDir ;
       
        float tansyDot = (-wheelVelocity.x - Mathf.Abs(wheelVelocity.z)*tanSyPrev) /axleTyreParams.relaxLenY;
        tanSy = tanSyPrev + tansyDot*Time.fixedDeltaTime;
        sy = Mathf.Atan(tanSy);
        
        float uSat = Mathf.Max(Mathf.Abs(wheelVelocity.z),1);
        float sxDot = (omega*axleTyreParams.tyreRadius - wheelVelocity.z - Mathf.Abs(wheelVelocity.z)*sxPrev )/axleTyreParams.relaxLenX ;
        sx = sxPrev + sxDot*Time.fixedDeltaTime;
        sx = Mathf.Clamp(sx,-1f,1f);

        tanSyPrev = tanSy;
        sxPrev = sx;
    }
    public Vector3 calcTyreForcesNonlinear()
    {
        S = Mathf.Sqrt(Mathf.Pow((sy),2) + Mathf.Pow(sx,2));
        float sy_S = sy/Mathf.Max(S,0.0001f);
        float sx_S = sx/Mathf.Max(S,0.0001f);

        float By = Mathf.Tan(Mathf.PI/(2f*axleTyreParams.Cy))/axleTyreParams.syPeak;
        float Bx = Mathf.Tan(Mathf.PI/(2f*axleTyreParams.Cx))/axleTyreParams.sxPeak;

        calcThermalScaling();
        float FzLoad = Mathf.Clamp(Fz,axleTyreParams.FzNom/3f,axleTyreParams.FzNom*3f);
        DyEffective = axleTyreParams.Dy + axleTyreParams.Dy2*(FzLoad-axleTyreParams.FzNom)/axleTyreParams.FzNom;
        DxEffective = axleTyreParams.Dx + axleTyreParams.Dx2*(FzLoad-axleTyreParams.FzNom)/axleTyreParams.FzNom;
        //DxEffective = Mathf.Clamp(DxEffective,DxEffective/2f,DxEffective*2f);
        //DyEffective = Mathf.Clamp(DyEffective,DyEffective/2f,DyEffective*2f);

        Fy = thermalScaling*Fz*sy_S*DyEffective*Mathf.Sin(axleTyreParams.Cy*Mathf.Atan(By*S));
        Fx = thermalScaling*Fz*sx_S*DxEffective*Mathf.Sin(axleTyreParams.Cx*Mathf.Atan(Bx*S));
   
        //Fx -= axleTyreParams.rollResForce * Mathf.Sign(omega);
        
        if (float.IsNaN(Fx) ) Fx = 0f; 
        if (float.IsNaN(Fy) ) Fy = 0f; 

        Vector3 localForceTotal = new Vector3(0f,0f,0f);
        localForceTotal = Fz  * transform.up + Fx * transform.forward + Fy * transform.right;
        return localForceTotal;
    }

    void calcThermalScaling()
    {
        if (GameManager.Instance.Settings.myVehSetup.IsThermalTyre)
        {
            thermalScaling = HelperFunctions.lut1D(axleTyreParams.numPointsFrictionMap,
                                axleTyreParams.thermalFrictionMapInput, axleTyreParams.thermalFrictionMapOutput, currTyreTemp);
        }
        else
        {
            thermalScaling = 1.0f;
        }
    }

    void calcTyreTemp()
    {
        // Compute the heat generation
        q1 = axleTyreParams.p1 * Mathf.Abs(wheelVelocity.z) * (Mathf.Abs(Fx * sx) + Mathf.Abs(Fy * Mathf.Tan(sy)));
            
        q2 = Mathf.Abs(wheelVelocity.z) * (axleTyreParams.p2 * Mathf.Abs(Fx) + axleTyreParams.p3 * Mathf.Abs(Fy) + axleTyreParams.p4 * Mathf.Abs(Fz));

        // Compute the heat dissipation
        q3 = axleTyreParams.p5 * Mathf.Pow(Mathf.Clamp(Mathf.Abs(wheelVelocity.z),1f,Mathf.Abs(wheelVelocity.z)),axleTyreParams.p6) * (currTyreTemp - carController.vehicleParams.tAmb);

        // Compute the heat transfer to the track
        float isMoving = Mathf.Abs(wheelVelocity.z) > 5f ? 1f : 0f;
        q4 =  axleTyreParams.hT * axleTyreParams.ACp * (currTyreTemp - carController.vehicleParams.tTrack);

        // Compute the rate of change of tyre temperature
        float dT_s = (q1 + q2 - q3 - q4) / (axleTyreParams.mT * axleTyreParams.cT);

        currTyreTemp += dT_s*Time.fixedDeltaTime;

        currTyreTemp = Mathf.Clamp(currTyreTemp, 0.0f, 199.0f);
    }
    void Start()
    {   
        currTyreTemp = carController.vehicleParams.tAmb;
        setWheelPos();
        wheelAngle = 0.0f;

        string configFileName;
        if (wheelFL || wheelFR){
            configFileName = "FrontAxleTireParams.json";
        }
        else{
            configFileName = "RearAxleTireParams.json";
        }
        string fullPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PAIRSIM_config"), "Parameters/" + configFileName);
        CheckAndCreateDefaultFile(fullPath, wheelFL || wheelFR ? defaultFrontAxleParams : defaultRearAxleParams);
        LoadTyreParametersFromJson(fullPath);

    }
    void LoadTyreParametersFromJson(string filePath)
    {
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            TyreParametersConfig config = JsonUtility.FromJson<TyreParametersConfig>(json);
            ApplyTyreParameters(config);
        }
        else
        {
            Debug.LogError("Config file not found: " + fullPath);
        }
    }
//check to see if params file already exists, if not write the file
    void CheckAndCreateDefaultFile(string filePath, TyreParametersConfig defaultParams)
    {
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string json = JsonUtility.ToJson(defaultParams, true);
            File.WriteAllText(filePath, json);
        }
    }
    //this function will override the scriptable object
    void ApplyTyreParameters(TyreParametersConfig config)
    {
        axleTyreParams.FzNom = (float)config.FzNom;
        axleTyreParams.Dy = (float)config.Dy;
        axleTyreParams.Dy2 = (float)config.Dy2;
        axleTyreParams.Cy = (float)config.Cy;
        axleTyreParams.syPeak = (float)config.syPeak;
        axleTyreParams.relaxLenY = (float)config.relaxLenY;
        axleTyreParams.Dx = (float)config.Dx;
        axleTyreParams.Dx2 = (float)config.Dx2;
        axleTyreParams.Cx = (float)config.Cx;
        axleTyreParams.sxPeak = (float)config.sxPeak;
        axleTyreParams.relaxLenX = (float)config.relaxLenX;
        axleTyreParams.rollResForce = (float)config.rollResForce;
        axleTyreParams.wheelInertia = (float)config.wheelInertia;
        axleTyreParams.tyreRadius = (float)config.tyreRadius;
        axleTyreParams.p1 = (float)config.p1;
        axleTyreParams.p2 = (float)config.p2;
        axleTyreParams.p3 = (float)config.p3;
        axleTyreParams.p4 = (float)config.p4;
        axleTyreParams.p5 = (float)config.p5;
        axleTyreParams.p6 = (float)config.p6;
        axleTyreParams.p7 = (float)config.p7;
        axleTyreParams.mT = (float)config.mT;
        axleTyreParams.cT = (float)config.cT;
        axleTyreParams.hT = (float)config.hT;
        axleTyreParams.ACp = (float)config.ACp;
        axleTyreParams.numPointsFrictionMap = config.numPointsFrictionMap;
        axleTyreParams.thermalFrictionMapInput = ConvertDoubleArrayToFloatArray(config.thermalFrictionMapInput);
        axleTyreParams.thermalFrictionMapOutput = ConvertDoubleArrayToFloatArray(config.thermalFrictionMapOutput);
    }
    // Conversion function to convert double arrays to float arrays
    float[] ConvertDoubleArrayToFloatArray(double[] doubleArray)
    {
        return Array.ConvertAll(doubleArray, item => (float)item);
    }

    [System.Serializable]
    public class TyreParametersConfig
    {
        public double FzNom;
        public double Dy;
        public double Dy2;
        public double Cy;
        public double syPeak;
        public double relaxLenY;
        public double Dx;
        public double Dx2;
        public double Cx;
        public double sxPeak;
        public double relaxLenX;
        public double rollResForce;
        public double wheelInertia;
        public double tyreRadius;
        public double p1, p2, p3, p4, p5, p6, p7, mT, cT, hT, ACp;
        public int numPointsFrictionMap;
        public double[] thermalFrictionMapInput;
        public double[] thermalFrictionMapOutput;
    }
    private TyreParametersConfig defaultFrontAxleParams = new TyreParametersConfig
    {
        FzNom = 1700,
        Dy = 1.5,
        Dy2 = -0.2,
        Cy = 1.5,
        syPeak = 0.08725,
        relaxLenY = 0.1,
        Dx = 1.2,
        Dx2 = -0.2,
        Cx = 1.3,
        sxPeak = 0.07,
        relaxLenX = 0.1,
        rollResForce = 60,
        wheelInertia = 2,
        tyreRadius = 0.326,
        p1 = 0.001,
        p2 = 0.0001,
        p3 = 0.0001,
        p4 = 0.00001,
        p5 = 0.001,
        p6 = 1.2,
        p7 = 0.0003,
        mT = 10,
        cT = 3,
        hT = 12,
        ACp = 0.01,
        numPointsFrictionMap = 5,
        thermalFrictionMapInput = new double[] {0.0, 30.0, 70.0, 100.0, 130.0},
        thermalFrictionMapOutput = new double[] {0.8, 0.9, 1.0, 1.0, 0.8}
    };
    private TyreParametersConfig defaultRearAxleParams = new TyreParametersConfig
    {
        FzNom = 2200,
        Dy = 1.55,
        Dy2 = -0.2,
        Cy = 1.6,
        syPeak = 0.078525,
        relaxLenY = 0.1,
        Dx = 1.7,
        Dx2 = -0.2,
        Cx = 1.4,
        sxPeak = 0.06,
        relaxLenX = 0.1,
        rollResForce = 60,
        wheelInertia = 2,
        tyreRadius = 0.326,
        p1 = 0.001,
        p2 = 0.0001,
        p3 = 0.0001,
        p4 = 0.00001,
        p5 = 0.001,
        p6 = 1.2,
        p7 = 0.0003,
        mT = 10,
        cT = 3,
        hT = 12,
        ACp = 0.01,
        numPointsFrictionMap = 5,
        thermalFrictionMapInput = new double[] {0.0, 30.0, 70.0, 100.0, 130.0},
        thermalFrictionMapOutput = new double[] {0.8, 0.9, 1.0, 1.0, 0.8}
    };
    void FixedUpdate() 
    {   
        getSteering();
        calcOmega();
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelStAngle);
        isHit = Physics.Raycast( transform.position, -transform.up, out RaycastHit hit,  carController.vehicleParams.lSpring + axleTyreParams.tyreRadius);       
        if (isHit)
        {   
            wheelVelocity = transform.InverseTransformDirection(carBody.GetPointVelocity(hit.point));
            hitDistance = hit.distance;
            calcFz();
            calcSySx();
            Ftotal = calcTyreForcesNonlinear();
            
        }
        else
        {
            Ftotal = new Vector3(0f,0f,0f);
        }
        carBody.AddForceAtPosition(Ftotal, hit.point);
        calcTyreTemp();
    }
    void Update()
    {
        bool isHit2 = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit,  carController.vehicleParams.lSpring + axleTyreParams.tyreRadius);
        float tyrePos = -carController.vehicleParams.lSpring;
        if (isHit2)
        {
            Debug.DrawRay(hit.point, transform.forward * Fx * 0.001f, Color.red);
            Debug.DrawRay(hit.point, transform.right * Fy * 0.001f, Color.blue);
            Debug.DrawRay(hit.point, transform.up * Fz * 0.001f, Color.green);
            tyrePos = -currLength;
        }
        // update viusals of the mesh for wheel rotation and susp deflection
        mesh.transform.localPosition = new Vector3(0, tyrePos, 0); 
        mesh.transform.Rotate(Vector3.right, Time.deltaTime*180/(Mathf.PI)*omega);
    }
}
