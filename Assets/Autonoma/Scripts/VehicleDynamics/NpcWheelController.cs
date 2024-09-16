using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VehicleDynamics;
using System.IO;
using System;

public class NpcWheelController : MonoBehaviour
{       
    public Rigidbody carBody;
    public CarController carController;
    public GameObject mesh; // Wheel mesh for visual representation
    public MeshCollider wheelCollider; // MeshCollider for the wheel
    public bool wheelFL;
    public bool wheelFR;
    public bool wheelRL;
    public bool wheelRR;

    void Start()
    {   
        // Assign MeshCollider to the wheel, make sure it's convex
        wheelCollider = GetComponent<MeshCollider>();
        if (wheelCollider != null)
        {
            wheelCollider.convex = true; // Convex colliders are needed for most interactions
        }

        // // Set Rigidbody to kinematic to prevent physics-based movements
        // if (carBody != null)
        // {
        //     carBody.isKinematic = true;
        // }

        // Set initial wheel position based on its type
        setWheelPos();
    }

    // Function to set the initial wheel position
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

    void Update()
    {
        // Update the visual representation of the wheel
        UpdateWheelMeshPosition();
    }

    // Function to update the wheel's visual position based on mesh
    void UpdateWheelMeshPosition()
    {
        // Set mesh to represent only its visual without any dynamic changes
        mesh.transform.localPosition = new Vector3(0, 0, 0);
        mesh.transform.rotation = Quaternion.identity;
    }
}
