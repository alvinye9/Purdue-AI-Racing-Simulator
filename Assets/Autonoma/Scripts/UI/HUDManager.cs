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
using UnityEngine.UI;
using TMPro;
using VehicleDynamics;
using Autonoma;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    public GameObject car;
    public WheelController[] wheels;
    public CarController carController;
    public TrackPositionFinder trackPosFinder;
    public VehicleDataSimulator vehDataSim;
    public Image brakeBar, throttleBar, tachBar, FLImage, FRImage, RLImage, RRImage;
    public TMP_Text gearText, tachometerText, wheelText, speedometerText,
        ctText, sysText, FLText, FRText, RLText, RRText, TTempText, ATempText, steeringBiasText;
    public Transform wheelTransform;
    public float WheelStart, WheelProgress, rpmPrev, hudRpm;
    public Button resetButton;
    public int FLTemp, FRTemp, RLTemp, RRTemp;
    public LapTimer lapTimer;
    public float hudMPH;
    
    public TMP_InputField throttleMean;
    public TMP_InputField throttleVariance;
    public TMP_InputField brakeMean;
    public TMP_InputField brakeVariance;
    public TMP_InputField steerMean;
    public TMP_InputField steerVariance;

    public Button injectPulseButton;

    void Start()
    {
        resetButton.onClick.AddListener( resetButtonPressed  );
        injectPulseButton.onClick.AddListener( injectPulseButtonPressed  );

        steerMean.text = carController.steerNoiseGenerator.mean.ToString();
        steerVariance.text = carController.steerNoiseGenerator.variance.ToString();
        throttleMean.text = carController.throttleNoiseGenerator.mean.ToString();
        throttleVariance.text = carController.throttleNoiseGenerator.variance.ToString();
        brakeMean.text = carController.brakeNoiseGenerator.mean.ToString();
        brakeVariance.text = carController.brakeNoiseGenerator.variance.ToString();

        steerMean.onEndEdit.AddListener(delegate { steerMeanChanged(steerMean); } );
        steerVariance.onEndEdit.AddListener(delegate { steerVarianceChanged(steerVariance); } );
        brakeMean.onEndEdit.AddListener(delegate { brakeMeanChanged(brakeMean); } );
        brakeVariance.onEndEdit.AddListener(delegate { brakeVarianceChanged(brakeVariance); } );
        throttleMean.onEndEdit.AddListener(delegate { throttleMeanChanged(throttleMean); } );
        throttleVariance.onEndEdit.AddListener(delegate { throttleVarianceChanged(throttleVariance); } );

    }
    
    void resetButtonPressed()
    {
        //lapTimer.laptimes.Clear();
        float currPosX =  (float)GameManager.Instance.Settings.myTrackParams.trackInfo.innerX[trackPosFinder.minIdx]; 
        float currPosY =  (float)GameManager.Instance.Settings.myTrackParams.trackInfo.innerY[trackPosFinder.minIdx]; 
        float currPosZ =  (float)GameManager.Instance.Settings.myTrackParams.trackInfo.innerZ[trackPosFinder.minIdx]; 
        float currHeading =  (float)GameManager.Instance.Settings.myTrackParams.trackInfo.heading[trackPosFinder.minIdx]; 
        car.transform.position = new Vector3(currPosX, currPosY + 1f, currPosZ);
        car.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Vector3 currRotation = new Vector3(0f,currHeading,0f);
        car.transform.Rotate(currRotation);

        car.GetComponent<Rigidbody>().velocity = new Vector3(0f,0f,0f);
        car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        car.GetComponent<CarController>().gear = 1;
        car.GetComponent<CarController>().TEngine = 0f;
        car.GetComponent<CarController>().thrApplied = 0f;

        foreach (WheelController wheel in wheels)
        {
            wheel.omega = 0f;
            wheel.omegaDot = 0f;
            wheel.Fx = 0f;
            wheel.driveTorque = 0f;
        }
    }

    // Method called in Car Controller
    void injectPulseButtonPressed()
    {
        if (carController != null)
        {
            carController.InjectPulse(); // Call the InjectPulse method from CarController
        }
        else
        {
            Debug.LogError("CarController reference is missing.");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetButtonPressed();
        }
        hudRpm = HelperFunctions.lowPassFirstOrder(carController.rpmEngine,rpmPrev, 15f);
        rpmPrev = hudRpm;
        float avgRearOmega = 0.5f*(wheels[2].omega + wheels[3].omega );
        //float avgRearOmega = 0.5f;
        float hudSpeed = avgRearOmega*carController.vehicleParams.rearTyreParams.tyreRadius;
 
        hudSpeed = Mathf.Clamp(hudSpeed,0f,hudSpeed);
        hudMPH = hudSpeed;
        speedometerText.text =  ((int)(hudSpeed*3.6f*0.621371192f)).ToString();

        tachometerText.text =  ((int)hudRpm).ToString();

        wheelTransform.localRotation = Quaternion.Euler(Vector3.forward * (WheelStart - carController.steerAngleApplied*carController.vehicleParams.steeringRatio * -1f  ) );
        wheelText.text =  ((int)(carController.steerAngleApplied*carController.vehicleParams.steeringRatio)).ToString();
        steeringBiasText.text = "Steering bias: " + ((double)(carController.vehicleParams.steeringBias)).ToString() + " deg";
        gearText.text = carController.gear.ToString();

        brakeBar.fillAmount = carController.brakeApplied/100.0f;
        throttleBar.fillAmount = carController.thrApplied;

        tachBar.fillAmount = hudRpm/7500;
        if (hudRpm > 6500 && hudRpm < 7000)
        {
            tachBar.color = new Color(1f, 1f, 0f, 1f);
        }
        else if (hudRpm >= 7000 && hudRpm < 7400)
        {
            tachBar.color = new Color(1f, 0f, 0f, 1f);
        }
        else if (hudRpm >= 7400)
        {
            tachBar.color = new Color(1f, 0f, 1f, 1f);
        }
        else
        {
            tachBar.color = new Color(1f, 1f, 1f, 1f);
        }

        ctText.text = vehDataSim.ct_state.ToString();
        sysText.text = vehDataSim.sys_state.ToString();

        FLTemp = (int)wheels[0].currTyreTemp;
        FRTemp = (int)wheels[1].currTyreTemp;
        RLTemp = (int)wheels[2].currTyreTemp;
        RRTemp = (int)wheels[3].currTyreTemp;
        FLText.text = FLTemp.ToString();
        FRText.text = FRTemp.ToString();
        RLText.text = RLTemp.ToString();
        RRText.text = RRTemp.ToString();


        Color coldColor = new Color (0f, 0f, 1f, .9f);
        Color goodColor = new Color (0f, 1f, 0f, .9f);
        Color hotColor = new Color (1f, 0f, 0f, .9f);
        FLImage.color = (FLTemp < 100) ? Color.Lerp (coldColor, goodColor, FLTemp*1.1f/100f) : 
            Color.Lerp (goodColor, hotColor, (FLTemp-100)*.04f);
        FRImage.color = (FRTemp < 100) ? Color.Lerp (coldColor, goodColor, FRTemp*1.1f/100f) :
            Color.Lerp (goodColor, hotColor, (FRTemp-100)*.04f);
        RLImage.color = (RLTemp < 100) ? Color.Lerp (coldColor, goodColor, RLTemp*1.1f/100f) :
            Color.Lerp (goodColor, hotColor, (RLTemp-100)*.04f);
        RRImage.color = (RRTemp < 100) ? Color.Lerp (coldColor, goodColor, RRTemp*1.1f/100f) :
            Color.Lerp (goodColor, hotColor, (RRTemp-100)*.04f);

        TTempText.text = carController.vehicleParams.tTrack.ToString()+" C";
        ATempText.text = carController.vehicleParams.tAmb.ToString()+" C";
    }   

    private void steerMeanChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.steerNoiseGenerator.mean = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }
    private void steerVarianceChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.steerNoiseGenerator.variance = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }
    private void brakeMeanChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.brakeNoiseGenerator.mean = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }
    private void brakeVarianceChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.brakeNoiseGenerator.variance = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }
    private void throttleMeanChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.throttleNoiseGenerator.mean = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }
    private void throttleVarianceChanged(TMP_InputField input)
    {
        if (float.TryParse(input.text, out float value))
        {
            carController.throttleNoiseGenerator.variance = value;
            Debug.Log(value);
        }
        else
        {
            Debug.LogError("Failed to Parse value: " + input.text);
        }

    }

}