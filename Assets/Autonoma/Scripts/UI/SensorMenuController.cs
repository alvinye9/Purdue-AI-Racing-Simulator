/* 
Copyright 2023 Autonoma, Inc.

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
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class SensorMenuController : MonoBehaviour
{
    public ScenarioMenuController scenarioMenu;
    internal List<SensorPrefabController> prefabList = new List<SensorPrefabController>();
    public Button mainMenuButton;
    public Button saveSensorSetButton;
    public Button deleteSensorSetButton;
    public Button scenarioSetupButton;
    public Button sensorSetupButton;
    public Button vehSetupButton;
    public Button addSensorButton;

    public Toggle enableTopToggle;
    public Toggle enableBottomToggle;
    public Toggle enableVectorNavToggle;
    public Toggle enableCanToggle;
    public Toggle enableFLWheelToggle;
    public Toggle enableFRWheelToggle;
    public Toggle enableRLWheelToggle;
    public Toggle enableRRWheelToggle;
    public Toggle enableFrontDiffToggle;
    public Toggle enableRearDiffToggle;
    public TMP_Dropdown sensorSetDropdown;
    
    public TMP_InputField sensorSetNameInput;
    public TMP_InputField steerMeanInput;
    public TMP_InputField steerVarianceInput;
    public TMP_InputField steerSeedInput;

    public TMP_InputField brakeMeanInput;
    public TMP_InputField brakeVarianceInput;
    public TMP_InputField brakeSeedInput;

    public TMP_InputField throttleMeanInput;
    public TMP_InputField throttleVarianceInput;
    public TMP_InputField throttleSeedInput;

    public TMP_InputField posMeanInput;
    public TMP_InputField posVarianceInput;
    public TMP_InputField posSeedInput;

    public TMP_InputField velMeanInput;
    public TMP_InputField velVarianceInput;
    public TMP_InputField velSeedInput;

    public TMP_InputField accMeanInput;
    public TMP_InputField accVarianceInput;
    public TMP_InputField accSeedInput;

    public TMP_InputField headingMeanInput;
    public TMP_InputField headingVarianceInput;
    public TMP_InputField headingSeedInput;

    public TMP_InputField gyroMeanInput;
    public TMP_InputField gyroVarianceInput;
    public TMP_InputField gyroSeedInput;
    
    private void Awake() {}

    private void Start() 
    {
        // Update the GUI based on toggle values
        enableTopToggle.isOn = scenarioMenu.tmpSensorSet.EnableTop;
        enableBottomToggle.isOn = scenarioMenu.tmpSensorSet.EnableBottom;
        enableVectorNavToggle.isOn = scenarioMenu.tmpSensorSet.EnableVectorNav;
        enableCanToggle.isOn = scenarioMenu.tmpSensorSet.EnableCan;
        enableFLWheelToggle.isOn = scenarioMenu.tmpSensorSet.EnableFLWheel;
        enableFRWheelToggle.isOn = scenarioMenu.tmpSensorSet.EnableFRWheel;
        enableRLWheelToggle.isOn = scenarioMenu.tmpSensorSet.EnableRLWheel;
        enableRRWheelToggle.isOn = scenarioMenu.tmpSensorSet.EnableRRWheel;
        enableFrontDiffToggle.isOn = scenarioMenu.tmpSensorSet.EnableFrontDiff;
        enableRearDiffToggle.isOn = scenarioMenu.tmpSensorSet.EnableRearDiff;

        // Update the GUI Input Fields based on gaussian noise values if at least one values is not zero (default)
        if (scenarioMenu.tmpSensorSet.steerMean != 0f ||
            scenarioMenu.tmpSensorSet.steerVariance != 0f ||
            scenarioMenu.tmpSensorSet.steerSeed != 0 ||
            scenarioMenu.tmpSensorSet.brakeMean != 0f ||
            scenarioMenu.tmpSensorSet.brakeVariance != 0f ||
            scenarioMenu.tmpSensorSet.brakeSeed != 0 ||
            scenarioMenu.tmpSensorSet.throttleMean != 0f ||
            scenarioMenu.tmpSensorSet.throttleVariance != 0f ||
            scenarioMenu.tmpSensorSet.throttleSeed != 0)
            {
                steerMeanInput.text = scenarioMenu.tmpSensorSet.steerMean.ToString();
                steerVarianceInput.text = scenarioMenu.tmpSensorSet.steerVariance.ToString();
                steerSeedInput.text = scenarioMenu.tmpSensorSet.steerSeed.ToString();
                brakeMeanInput.text = scenarioMenu.tmpSensorSet.brakeMean.ToString();
                brakeVarianceInput.text = scenarioMenu.tmpSensorSet.brakeVariance.ToString();
                brakeSeedInput.text = scenarioMenu.tmpSensorSet.brakeSeed.ToString();
                throttleMeanInput.text = scenarioMenu.tmpSensorSet.throttleMean.ToString();
                throttleVarianceInput.text = scenarioMenu.tmpSensorSet.throttleVariance.ToString();
                throttleSeedInput.text = scenarioMenu.tmpSensorSet.throttleSeed.ToString();
            }

        mainMenuButton.onClick.AddListener( GameManager.Instance.UIManager.OnMainMenuPressed );
        saveSensorSetButton.onClick.AddListener( saveSensorSetButtonPressed );
        deleteSensorSetButton.onClick.AddListener ( deleteSensorSetButtonPressed );
        scenarioSetupButton.onClick.AddListener( scenarioSetupButtonPressed );
        vehSetupButton.onClick.AddListener( vehSetupButtonPressed );
        sensorSetupButton.onClick.AddListener( sensorSetupButtonPressed );
        addSensorButton.onClick.AddListener( addSensorButtonPressed  );

        sensorSetDropdown.onValueChanged.AddListener(delegate { chosenSensorSetChanged(sensorSetDropdown); } );
        enableTopToggle.onValueChanged.AddListener(delegate { enableTopToggleChanged(enableTopToggle); } );
        enableBottomToggle.onValueChanged.AddListener(delegate { enableBottomToggleChanged(enableBottomToggle); } );
        enableVectorNavToggle.onValueChanged.AddListener(delegate { enableVectorNavToggleChanged(enableVectorNavToggle); } );
        enableCanToggle.onValueChanged.AddListener(delegate { enableCanToggleChanged(enableCanToggle); } );

        enableFLWheelToggle.onValueChanged.AddListener(delegate { enableFLWheelToggleChanged(enableFLWheelToggle); } );
        enableFRWheelToggle.onValueChanged.AddListener(delegate { enableFRWheelToggleChanged(enableFRWheelToggle); } );
        enableRLWheelToggle.onValueChanged.AddListener(delegate { enableRLWheelToggleChanged(enableRLWheelToggle); } );
        enableRRWheelToggle.onValueChanged.AddListener(delegate { enableRRWheelToggleChanged(enableRRWheelToggle); } );

        enableFrontDiffToggle.onValueChanged.AddListener(delegate { enableFrontDiffToggleChanged(enableFrontDiffToggle); } );
        enableRearDiffToggle.onValueChanged.AddListener(delegate { enableRearDiffToggleChanged(enableRearDiffToggle); } );

        steerMeanInput.onEndEdit.AddListener(delegate { steerMeanInputChanged(steerMeanInput); } );
        steerVarianceInput.onEndEdit.AddListener(delegate { steerVarianceInputChanged(steerVarianceInput); } );
        steerSeedInput.onEndEdit.AddListener(delegate { steerSeedInputChanged(steerSeedInput); } );

        brakeMeanInput.onEndEdit.AddListener(delegate { brakeMeanInputChanged(brakeMeanInput); } );
        brakeVarianceInput.onEndEdit.AddListener(delegate { brakeVarianceInputChanged(brakeVarianceInput); } );
        brakeSeedInput.onEndEdit.AddListener(delegate { brakeSeedInputChanged(brakeSeedInput); } );

        throttleMeanInput.onEndEdit.AddListener(delegate { throttleMeanInputChanged(throttleMeanInput); } );
        throttleVarianceInput.onEndEdit.AddListener(delegate { throttleVarianceInputChanged(throttleVarianceInput); } );
        throttleSeedInput.onEndEdit.AddListener(delegate { throttleSeedInputChanged(throttleSeedInput); } );

        posMeanInput.onEndEdit.AddListener(delegate { posMeanInputChanged(posMeanInput); } );
        posVarianceInput.onEndEdit.AddListener(delegate { posVarianceInputChanged(posVarianceInput); } );
        posSeedInput.onEndEdit.AddListener(delegate { posSeedInputChanged(posSeedInput); } );
    
        velMeanInput.onEndEdit.AddListener(delegate { velMeanInputChanged(velMeanInput); } );
        velVarianceInput.onEndEdit.AddListener(delegate { velVarianceInputChanged(velVarianceInput); } );
        velSeedInput.onEndEdit.AddListener(delegate { velSeedInputChanged(velSeedInput); } );

        accMeanInput.onEndEdit.AddListener(delegate { accelMeanInputChanged(accMeanInput); } );
        accVarianceInput.onEndEdit.AddListener(delegate { accelVarianceInputChanged(accVarianceInput); } );
        accSeedInput.onEndEdit.AddListener(delegate { accelSeedInputChanged(accSeedInput); } );

        headingMeanInput.onEndEdit.AddListener(delegate { headingMeanInputChanged(headingMeanInput); } );
        headingVarianceInput.onEndEdit.AddListener(delegate { headingVarianceInputChanged(headingVarianceInput); } );
        headingSeedInput.onEndEdit.AddListener(delegate { headingSeedInputChanged(headingSeedInput); } );

        gyroMeanInput.onEndEdit.AddListener(delegate { gyroMeanInputChanged(gyroMeanInput); } );
        gyroVarianceInput.onEndEdit.AddListener(delegate { gyroVarianceInputChanged(gyroVarianceInput); } );
        gyroSeedInput.onEndEdit.AddListener(delegate { gyroSeedInputChanged(gyroSeedInput); } );

    }

    private void OnEnable() 
    {
        SensorPrefabController.OnSensorDeleted += handleSensorDeleted;
        
        if (scenarioMenu == null)
        {
            Debug.LogWarning("ScenarioMenuController instance is null");
        }
        else
        {
            fillSensorSetDropdown(scenarioMenu.sensorSetDropdown.value);
            chosenSensorSetChanged(sensorSetDropdown);
            saveSensorSetButtonPressed();
        }
    }

    private void OnDisable()
    {
        SensorPrefabController.OnSensorDeleted -= handleSensorDeleted;
    }

    private void fillSensorSetDropdown(int idx)
    {   
        sensorSetDropdown.ClearOptions(); 
        var reversedLoadedSensorSets = scenarioMenu.LoadedSensorSets.ToArray();
        Array.Reverse(reversedLoadedSensorSets);

        foreach(SensorSet sensorSetObj in reversedLoadedSensorSets)
        {
            var op = new TMP_Dropdown.OptionData(sensorSetObj.Name);
            sensorSetDropdown.options.Add(op);
        }
        sensorSetDropdown.value = idx;
        sensorSetDropdown.RefreshShownValue();
    }

    private void chosenSensorSetChanged(TMP_Dropdown dropdown)
    {
        int idx = dropdown.value;
        var reversedLoadedSensorSets = scenarioMenu.LoadedSensorSets.ToArray();
        Array.Reverse(reversedLoadedSensorSets);
        deleteSensorSetButton.interactable = (scenarioMenu.LoadedSensorSets.Count <= 1) ? false : true;
        scenarioMenu.tmpSensorSet = reversedLoadedSensorSets[idx];
        
        sensorSetNameInput.text = scenarioMenu.tmpSensorSet.Name;
    }

    private void enableTopToggleChanged(Toggle enableTopToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableTop = enableTopToggle.isOn;
    }
    private void enableBottomToggleChanged(Toggle enableBottomToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableBottom = enableBottomToggle.isOn;
    }
    private void enableVectorNavToggleChanged(Toggle enableVectorNavToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableVectorNav = enableVectorNavToggle.isOn;
    }
    private void enableCanToggleChanged(Toggle enableCanToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableCan = enableCanToggle.isOn;
    }

    private void enableFLWheelToggleChanged(Toggle enableFLWheelToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableFLWheel = enableFLWheelToggle.isOn;
    }
    private void enableFRWheelToggleChanged(Toggle enableFRWheelToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableFRWheel = enableFRWheelToggle.isOn;
    }
    private void enableRLWheelToggleChanged(Toggle enableRLWheelToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableRLWheel = enableRLWheelToggle.isOn;
    }
    private void enableRRWheelToggleChanged(Toggle enableRRWheelToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableRRWheel = enableRRWheelToggle.isOn;
    }
    private void enableFrontDiffToggleChanged(Toggle enableFrontDiffToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableFrontDiff = enableFrontDiffToggle.isOn;
    }
    private void enableRearDiffToggleChanged(Toggle enableRearDiffToggle)
    {
        updateTmpSensorSet();
        scenarioMenu.tmpSensorSet.EnableRearDiff = enableRearDiffToggle.isOn;
    }
    private void steerMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.steerMean = value;
            Debug.Log(value);
        }
    }
    private void steerVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.steerVariance = value;
            Debug.Log(value);
        }
    }
    private void steerSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.steerSeed = value;
            Debug.Log(value);
        }
    }
    private void brakeMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.brakeMean = value;
            Debug.Log(value);
        }
    }
    private void brakeVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.brakeVariance = value;
            Debug.Log(value);
        }
    }
    private void brakeSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.brakeSeed = value;
            Debug.Log(value);
        }
    }

    private void throttleMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.throttleMean = value;
            Debug.Log(value);
        }
    }
    private void throttleVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.throttleVariance = value;
            Debug.Log(value);
        }
    }
    private void throttleSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.throttleSeed = value;
            Debug.Log(value);
        }
    }
    private void posMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.posMean = value;
            Debug.Log(value);
        }
    }
    private void posVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.posVariance = value;
            Debug.Log(value);
        }
    }
    private void posSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.posSeed = value;
            Debug.Log(value);
        }
    }
    private void velMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.velMean = value;
            Debug.Log(value);
        }
    }
    private void velVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.velVariance = value;
            Debug.Log(value);
        }
    }
    private void velSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.velSeed = value;
            Debug.Log(value);
        }
    }
    private void accelMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.accelMean = value;
            Debug.Log(value);
        }
    }
    private void accelVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.accelVariance = value;
            Debug.Log(value);
        }
    }
    private void accelSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.accelSeed = value;
            Debug.Log(value);
        }
    }
    private void headingMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.headingMean = value;
            Debug.Log(value);
        }
    }
    private void headingVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.headingVariance = value;
            Debug.Log(value);
        }
    }
    private void headingSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.headingSeed = value;
            Debug.Log(value);
        }
    }
    private void gyroMeanInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.gyroMean = value;
            Debug.Log(value);
        }
    }
    private void gyroVarianceInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (float.TryParse(input.text, out float value))
        {
            scenarioMenu.tmpSensorSet.gyroVariance = value;
            Debug.Log(value);
        }
    }
    private void gyroSeedInputChanged(TMP_InputField input)
    {
        updateTmpSensorSet();
        if (int.TryParse(input.text, out int value))
        {
            scenarioMenu.tmpSensorSet.gyroSeed = value;
            Debug.Log(value);
        }
    }


    private void saveSensorSetButtonPressed()
    {
        int idx = (sensorSetNameInput.text == scenarioMenu.tmpSensorSet.Name) ? sensorSetDropdown.value : scenarioMenu.LoadedSensorSets.Count;

        updateTmpSensorSet();

        saveSensorSet(scenarioMenu.tmpSensorSet);

        fillSensorSetDropdown(0);
    }

    private void scenarioSetupButtonPressed()
    {
        updateTmpSensorSet();
        scenarioMenu.fillSensorSetDropdown(sensorSetDropdown.value);
        GameManager.Instance.UIManager.OnScenarioMenuPressed();
    }

    private void vehSetupButtonPressed()
    {
        updateTmpSensorSet();
        scenarioMenu.fillSensorSetDropdown(sensorSetDropdown.value);
        GameManager.Instance.UIManager.OnVehicleSetupMenuPressed();
    }

    private void sensorSetupButtonPressed() {}

    private void deleteSensorSetButtonPressed()
    {
        if ( sensorSetDropdown.value > 0 )
        {
            int idx = sensorSetDropdown.value;
            updateTmpSensorSet();
            deleteSensorSet(scenarioMenu.tmpSensorSet);
            fillSensorSetDropdown(idx-1);
            chosenSensorSetChanged(sensorSetDropdown);
        }
    }

    private void addSensorButtonPressed()
    {
        // Add sensor logic
    }

    private void updateTmpSensorSet()
    {   
        // Put all input data to tmpSensorSet object
        scenarioMenu.tmpSensorSet.Name = sensorSetNameInput.text;

        // Debug.Log("Name of Sensor Set: " + scenarioMenu.tmpSensorSet.Name);
    }

    private void saveSensorSet(SensorSet inputObj)
    {
        SaveDataManager.SaveSensorSet(inputObj);
        scenarioMenu.LoadedSensorSets = SaveDataManager.LoadAllSensorSets();
    }

    private void deleteSensorSet(SensorSet inputObj)
    {
        SaveDataManager.DeleteSensorSet(inputObj);
        scenarioMenu.LoadedSensorSets = SaveDataManager.LoadAllSensorSets();
    }

    private void handleSensorDeleted(SensorPrefabController prefab)
    {
        prefabList.Remove(prefab);
    }
}