/* 
Copyright 2025 Purdue AI Racing
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
using VehicleDynamics;

namespace Autonoma
{
public class RaceControlData : MonoBehaviour
{
    public RaceControl rc = new RaceControl();
    public RaptorSM sm;
    void Start(){}
    void Update()
    {
        //do not use race control GUI in test mode
        if(!(GameManager.Instance.Settings.myScenarioObj.ModeSwitchState)){
            sm.current_flag = (int)rc.VehicleFlag;
        }
    }
}
}