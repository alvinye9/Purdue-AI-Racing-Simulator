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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using ROS2;

public class SpawnManager : MonoBehaviour 
{
    [SerializeField]
    private GameObject vehiclePrefab;
    public GameObject npcVehiclePrefab;
    public GameObject npc2VehiclePrefab;
    public GameObject kentuckyPrefab;
    public GameObject imsPrefab;
    public GameObject imsBankedPrefab;
    public Material[] materials;
    public RaceControlMenuController raceControlMenu;
    private TrackParams trackParams;
    public GlobalCameraManager globalCameraManager;
    private AssetBundle myLoadedAssetBundle = null;



    private void Awake()
    {
        globalCameraManager = FindObjectOfType<GlobalCameraManager>();
    }

    private void Start()
    {
        trackParams = GameManager.Instance.Settings.myTrackParams; //default track params are from the map's respective .asset file
        Debug.Log("Track Name: " + trackParams.TrackName);
        Debug.Log("LAT_ORIGIN: " + trackParams.LAT_ORIGIN);
        Debug.Log("LON_ORIGIN: " + trackParams.LON_ORIGIN);
        Debug.Log("HEIGHT_ORIGIN: " + trackParams.HEIGHT_ORIGIN);  

        trackParams.carSpawnPositions.RemoveRange(1, trackParams.carSpawnPositions.Count - 1); //remove all but the first spawn position

        trackParams.populateStartPositions();

        SpawnEnvironment();

        bool isPit = GameManager.Instance.Settings.myScenarioObj.IsPit;

        SpawnVehicle(0, isPit); //only spawn one ego vehicle

        if(GameManager.Instance.Settings.myScenarioObj.NumCars == 2) //spawn first NPC vehicle
        {
            SpawnNPCVehicle(1, isPit); 
        }

        if(GameManager.Instance.Settings.myScenarioObj.NumCars == 3) //spawn first and second NPC vehicle
        {
            SpawnNPCVehicle(1, isPit);
            SpawnNPC2Vehicle(2, isPit); 
        }      
        
    }

    private void OnDestroy()
    {
        if (myLoadedAssetBundle != null)
        {
            myLoadedAssetBundle.Unload(true);
        }
    }

    public void SpawnVehicle(int idx, bool isPit)
    {
        // Debug.Log("Spawning Ego Vehicle: " + idx);
        string trackName = GameManager.Instance.Settings.myTrackParams.TrackName+".prefab";
        
        Vector3 spawnPosition = trackParams.carSpawnPositions[idx]; 

        if(isPit){ // shift spawn position to pitlane
            Debug.Log("Track: " + trackName);
            float dx = 0f;
            float dy = 0f;
            float dz = 0f;
            if(trackName.Equals("LVMS.prefab")){
                dx = 53.37f;
                dy = -1.5f;
                dz = -59.44f;
            } 
            else if (trackName.Equals("Kentucky Speedway.prefab")){
                dx = 75f;
                dy = -1.9f;
                dz = -45f;

            }  
            else if (trackName.Equals("IMS.prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            }
            else if (trackName.Equals("IMS (banked).prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            }    
            float newX = spawnPosition.x + dx;
            float newY = spawnPosition.y + dy; 
            float newZ = spawnPosition.z + dz;
            spawnPosition = new Vector3(newX, newY, newZ);
        }
        else{
            // Debug.Log("NOT INSIDE PITLANE");        
        }

        GameObject vehicleInstance = Instantiate(vehiclePrefab, 
            spawnPosition,
            transform.rotation);

        vehicleInstance.transform.Rotate(trackParams.carRotation);

        raceControlMenu.rosCars.Add(vehicleInstance);

        Material[] mats = vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials;
        mats[0] = materials[(int) (GameManager.Instance.Settings.myScenarioObj.Cars[idx].Color) ];
        vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials = mats;
        
        GameObject[] vehicleCameras = vehicleInstance.transform.Find("Cameras").GetComponent<CameraList>().cameras;

        for(int i = 0; i < vehicleCameras.Length; i++) 
        {
            globalCameraManager.allCarCameraList.Add(new CarCameraPair(vehicleCameras[i], vehicleInstance));
        }

        // Handle the enabling/disabling of Publishers based on ControlType
        bool isROS = (GameManager.Instance.Settings.myScenarioObj.Cars[idx].ControlType == ControlType.ROS);
        var vehiclePublishers = vehicleInstance.GetComponentsInChildren<Autonoma.IPublisherBase>();
        // foreach (var pub in vehiclePublishers)
        // {
        // //    Debug.Log("Activating Ego-Vehicle Publishers");
        //    //pub.ToggleActive(isROS);
        //     // pub.ToggleActive(true);
        // }

        // Handle the enabling/disabling of Inputs based on ControlType
        Autonoma.VehicleInputSubscriber[] vehicleSubscribers = vehicleInstance.GetComponentsInChildren<Autonoma.VehicleInputSubscriber>();
        KeyboardInputs[] keyboardInputs = vehicleInstance.GetComponentsInChildren<KeyboardInputs>();
        // foreach (KeyboardInputs ki in keyboardInputs)
        // {
        //     //ki.gameObject.SetActive(!isROS);
        //     // ki.gameObject.SetActive(true);
        // }
        // foreach (Autonoma.VehicleInputSubscriber vi in vehicleSubscribers)
        // {
        //     //vi.gameObject.SetActive(isROS);
        //     // vi.gameObject.SetActive(true);
        // }

        // Disable Sensor Publishers in instantiated Vehicle
        Transform novatelTopTransform = vehicleInstance.transform.Find("URDF/base_link/Novatel PWRPAK7 Top");
        if (novatelTopTransform != null)
        {
            if (!GameManager.Instance.Settings.mySensorSet.EnableTop)
            {
                novatelTopTransform.gameObject.SetActive(false);
            }
            else
            {
                novatelTopTransform.gameObject.SetActive(true);
            }
        }
        else{
            Debug.LogWarning("Novatel PWRPAK7 Top object not found");
            
        }

        Transform novatelBottomTransform = vehicleInstance.transform.Find("URDF/base_link/Novatel PWRPAK7 Bottom");
        if (novatelBottomTransform != null)
        {
            if (!GameManager.Instance.Settings.mySensorSet.EnableBottom)
            {
                novatelBottomTransform.gameObject.SetActive(false);
            }
            else
            {
                novatelBottomTransform.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Novatel PWRPAK7 Bottom object not found");
            
        }

        Transform vectorNavTransform = vehicleInstance.transform.Find("URDF/base_link/Vectornav VN300");
        if (vectorNavTransform != null)
        {
            if (!GameManager.Instance.Settings.mySensorSet.EnableVectorNav)
            {
                vectorNavTransform.gameObject.SetActive(false);
            }
            else
            {
                vectorNavTransform.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Vectornav VN300 object not found");
            
        }

        Transform raptorTransform = vehicleInstance.transform.Find("URDF/base_link/Vehicle Sensors");
        if (raptorTransform != null)
        {
            if (!GameManager.Instance.Settings.mySensorSet.EnableCan)
            {
                raptorTransform.gameObject.SetActive(false);
            }
            else
            {
                raptorTransform.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Vehicle Sensors object not found");
            
        }

        // Disable the WheelController scripts in the instantiated vehicle
        Transform wheelControllerTransform = vehicleInstance.transform.Find("Wheels/FrontLeft");
        if (wheelControllerTransform != null)
        {
            var wheelController = wheelControllerTransform.GetComponent<WheelController>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableFLWheel)
            {
                wheelController.enabled = false;
            }
            else{
                wheelController.enabled = true;
            }
        }

        Transform FRTransform= vehicleInstance.transform.Find("Wheels/FrontRight");
        if (FRTransform!= null)
        {
            var wheelController = FRTransform.GetComponent<WheelController>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableFRWheel)
            {
                wheelController.enabled = false;
            }
            else{
                wheelController.enabled = true;
            }
        }

        Transform RLTransform= vehicleInstance.transform.Find("Wheels/RearLeft");
        if (RLTransform!= null)
        {
            var wheelController = RLTransform.GetComponent<WheelController>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableRLWheel)
            {
                wheelController.enabled = false;
            }
            else{
                wheelController.enabled = true;
            }
        }
        Transform RRTransform= vehicleInstance.transform.Find("Wheels/RearRight");
        if (RRTransform!= null)
        {
            var wheelController = RRTransform.GetComponent<WheelController>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableRRWheel)
            {
                wheelController.enabled = false;
            }
            else{
                wheelController.enabled = true;
            }
        }
        Transform FrontDiffTransform= vehicleInstance.transform.Find("FrontDifferential");
        if (FrontDiffTransform!= null)
        {
            var diffController = FrontDiffTransform.GetComponent<Differential>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableFrontDiff)
            {
                diffController.enabled = false;
            }
            else{
                diffController.enabled = true;
            }
        }
        Transform RearDiffTransform= vehicleInstance.transform.Find("RearDifferential");
        if (RearDiffTransform!= null)
        {
            var diffController = RearDiffTransform.GetComponent<Differential>();
            if (!GameManager.Instance.Settings.mySensorSet.EnableRearDiff)
            {
                diffController.enabled = false;
            }
            else{
                diffController.enabled = true;
            }
        }
    }

    public void SpawnNPCVehicle(int idx, bool isPit)
    {
        string trackName = GameManager.Instance.Settings.myTrackParams.TrackName+".prefab";
        Vector3 spawnPosition = trackParams.carSpawnPositions[idx]; 

        if(isPit){ 
            Debug.Log("INSIDE PITLANE");
            float dx = 0f;
            float dy = 0f;
            float dz = 0f;
            if(trackName.Equals("LVMS.prefab")){ 
                dx = 53.37f;
                dy = -1.5f;
                dz = -59.44f;
            } 
            else if (trackName.Equals("Kentucky Speedway.prefab")){
                dx = 75f;
                dy = -1.9f;
                dz = -45f;
            } 
            else if (trackName.Equals("IMS.prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            } 
            else if (trackName.Equals("IMS (banked).prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            }   
            float newX = spawnPosition.x + dx;
            float newY = spawnPosition.y + dy; 
            float newZ = spawnPosition.z + dz;
            spawnPosition = new Vector3(newX, newY, newZ);
        }

        GameObject vehicleInstance = Instantiate(npcVehiclePrefab, 
            spawnPosition,
            transform.rotation);

        vehicleInstance.transform.Rotate(trackParams.carRotation);

        raceControlMenu.rosCars.Add(vehicleInstance);

        Material[] mats = vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials;
        mats[0] = materials[(int) (GameManager.Instance.Settings.myScenarioObj.Cars[idx].Color) ];
        vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials = mats;

        GameObject[] vehicleCameras = vehicleInstance.transform.Find("Cameras").GetComponent<CameraList>().cameras;

        for(int i = 0; i < vehicleCameras.Length; i++) 
        {
            globalCameraManager.allCarCameraList.Add(new CarCameraPair(vehicleCameras[i], vehicleInstance));
        }

        // Activate all NPC publishers
        bool isROS = (GameManager.Instance.Settings.myScenarioObj.Cars[idx].ControlType == ControlType.ROS);
        var vehiclePublishers = vehicleInstance.GetComponentsInChildren<Autonoma.IPublisherBase>();
        foreach (var pub in vehiclePublishers)
        {
            pub.ToggleActive(true);
        }
        Autonoma.VehicleInputSubscriber[] vehicleSubscribers = vehicleInstance.GetComponentsInChildren<Autonoma.VehicleInputSubscriber>();
        KeyboardInputs[] keyboardInputs = vehicleInstance.GetComponentsInChildren<KeyboardInputs>();
        foreach (KeyboardInputs ki in keyboardInputs)
        {
            ki.gameObject.SetActive(true);
        }
        foreach (Autonoma.VehicleInputSubscriber vi in vehicleSubscribers)
        {
            vi.gameObject.SetActive(true);
        }

    }
    public void SpawnNPC2Vehicle(int idx, bool isPit)
    {
        string trackName = GameManager.Instance.Settings.myTrackParams.TrackName+".prefab";
        Vector3 spawnPosition = trackParams.carSpawnPositions[idx]; 

        if(isPit){ 
            Debug.Log("INSIDE PITLANE");
            float dx = 0f;
            float dy = 0f;
            float dz = 0f;
            if(trackName.Equals("LVMS.prefab")){ 
                dx = 53.37f;
                dy = -1.5f;
                dz = -59.44f;
            }
            else if (trackName.Equals("Kentucky Speedway.prefab")){
                dx = 75f;
                dy = -1.9f;
                dz = -45f;
            }
            else if (trackName.Equals("IMS.prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            }   
            else if (trackName.Equals("IMS (banked).prefab")){
                dx = 17f;
                dy = 0f;
                dz = 0f;

            } 
            float newX = spawnPosition.x + dx;
            float newY = spawnPosition.y + dy; 
            float newZ = spawnPosition.z + dz;
            spawnPosition = new Vector3(newX, newY, newZ);
        }

        GameObject vehicleInstance = Instantiate(npc2VehiclePrefab, 
            spawnPosition,
            transform.rotation);
        vehicleInstance.transform.Rotate(trackParams.carRotation);
        raceControlMenu.rosCars.Add(vehicleInstance);

        Material[] mats = vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials;
        mats[0] = materials[(int) (GameManager.Instance.Settings.myScenarioObj.Cars[idx].Color) ];
        vehicleInstance.transform.Find("Models").Find("Body").Find("Chassis").GetComponent<MeshRenderer>().materials = mats;

        GameObject[] vehicleCameras = vehicleInstance.transform.Find("Cameras").GetComponent<CameraList>().cameras;

        for(int i = 0; i < vehicleCameras.Length; i++) 
        {
            globalCameraManager.allCarCameraList.Add(new CarCameraPair(vehicleCameras[i], vehicleInstance));
        }

        bool isROS = (GameManager.Instance.Settings.myScenarioObj.Cars[idx].ControlType == ControlType.ROS);
        var vehiclePublishers = vehicleInstance.GetComponentsInChildren<Autonoma.IPublisherBase>();
        foreach (var pub in vehiclePublishers)
        {
            pub.ToggleActive(true);
        }

        // Handle the enabling/disabling of Inputs based on ControlType
        Autonoma.VehicleInputSubscriber[] vehicleSubscribers = vehicleInstance.GetComponentsInChildren<Autonoma.VehicleInputSubscriber>();
        KeyboardInputs[] keyboardInputs = vehicleInstance.GetComponentsInChildren<KeyboardInputs>();
        foreach (KeyboardInputs ki in keyboardInputs)
        {
            ki.gameObject.SetActive(true);
        }
        foreach (Autonoma.VehicleInputSubscriber vi in vehicleSubscribers)
        {
            vi.gameObject.SetActive(true);
        }
    }
    public void SpawnEnvironment()
    {
        string path = Application.streamingAssetsPath;
        string bundleName;

        string trackName;
        if(GameManager.Instance.Settings.myTrackParams.TrackName == "Kentucky Speedway" || GameManager.Instance.Settings.myTrackParams.TrackName == "IMS" || GameManager.Instance.Settings.myTrackParams.TrackName == "IMS (banked)")
        {
            trackName = "LVMS.prefab"; //just use LVMS prefab as a placeholder for custom tracks not in v2 unity package
        }
        else
        {
            trackName = GameManager.Instance.Settings.myTrackParams.TrackName+".prefab";
        }
        
        bool isBundleLoaded = false;

        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                bundleName = "osx_racetracks.v2";
                break;

            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                bundleName = "linux_racetracks.v2";
                break;

            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                bundleName = "windows_racetracks.v2";
                break;

            default:
                Debug.LogError("Unsupported platform!");
                return;
        }

        foreach (var bundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (bundle.name == bundleName)
            {
                Debug.Log("AssetBundle is already loaded.");
                isBundleLoaded = true;
                break;
            }
        }

        if (!isBundleLoaded)
        {
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(path, bundleName));
            
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            GameObject track = myLoadedAssetBundle.LoadAsset<GameObject>(trackName); //trackName is read from tracklist

            //specify position, rotation, and scale of new track instance
            if(trackName.Equals("LVMS.prefab"))
            {
                if(GameManager.Instance.Settings.myTrackParams.TrackName == "Kentucky Speedway")
                {
                    // Vector3 position = new Vector3(440f, -1.9f, -342f);
                    Vector3 position = new Vector3(438f, -1.9f, -346f);
                    Vector3 scale = new Vector3(0.97f, 1.08f, 0.96f); 
                    Quaternion rotation = Quaternion.Euler(0f, 129f, 0f); 
                    GameObject instantiatedTrack = Instantiate(kentuckyPrefab, position, rotation); //for new prefabs
                    instantiatedTrack.transform.localScale = scale;
                }
                else if(GameManager.Instance.Settings.myTrackParams.TrackName == "IMS")
                {
                    Vector3 position = new Vector3(-43.5f, -0.5f, 977f);
                    Quaternion rotation = Quaternion.Euler(0f, -91f, 0f); 

                    Vector3 scale = new Vector3(1f, 1f, 1f); 
                    GameObject instantiatedTrack = Instantiate(imsPrefab, position, rotation); //for new prefabs
                    instantiatedTrack.transform.localScale = scale;
                }
                else if(GameManager.Instance.Settings.myTrackParams.TrackName == "IMS (banked)")
                {
                    Vector3 position = new Vector3(-43.5f, -0.5f, 977f);
                    Quaternion rotation = Quaternion.Euler(0f, -91f, 0f); 

                    Vector3 scale = new Vector3(1f, 1f, 1f); 
                    GameObject instantiatedTrack = Instantiate(imsBankedPrefab, position, rotation); //for new prefabs
                    instantiatedTrack.transform.localScale = scale;
                }
                else{
                    // Vector3 position = new Vector3(262f, 9f, 368f); //(Car Rotation 107 deg) (based on the PAIR vegas2.csv raceline)
                    Vector3 position = new Vector3(205f, 9.2f, -257f);  //(Car Rotation 230 deg NED) to start at starting line
                    Quaternion rotation = Quaternion.Euler(0, 180f, 0);  
                    Vector3 scale = new Vector3(0.8025f, 0.8025f, 0.8025f); 
                    GameObject instantiatedTrack = Instantiate(track, position, rotation);
                    instantiatedTrack.transform.localScale = scale;
                }
            }

            else
            {
                Debug.Log("INSTANTIATING TRACK: "+ trackName);
                Instantiate(track);
            }


        }
    }

}