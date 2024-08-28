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

public class LapTimer : MonoBehaviour
{
    public TrackPositionFinder trackPosition;

    public int minIdxPrev;
    public List<float> laptimes;
    public float currLaptime;

    public bool newLapDetected;
    public RaceControl raceControl;

    public delegate void OnLapCompletedDelegate(float lapTime);
    public OnLapCompletedDelegate onLapCompleted;


    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currLaptime += Time.fixedDeltaTime;
        detectNewLap();


    }

    void detectNewLap()
    {
        if (trackPosition.minIdx < 200 && minIdxPrev > trackPosition.maxIdx - 200)
        {
        
            newLapDetected = true;
            if (laptimes.Count == 0)
            {
                laptimes.Add(300f);
            }
            else
            {
                if(GameManager.Instance.Settings.useLapTimeInterpolationAdjustment)
                {
                    // get car V to interp lap time
                    float dist = trackPosition.minDist;
                    // subtract from laptime dist / speed
                    float speed = GetVehicleForLap().GetSpeed();
                    if(speed > 0.0f)
                    {
                        float adjustment = dist / speed;
                        if(adjustment > 0.0 && adjustment < 0.1) // prevent erroneous adjustments
                        {
                            currLaptime -= adjustment;
                        }
                    }
                    //Debug.Log($"adjusted lap time by {adjustment}s");
                }

                if ( currLaptime > 5f )
                {
                    laptimes.Add(currLaptime);
                    onLapCompleted(currLaptime); //FIXME, might cause issues..
                }
            }
          
         
            currLaptime = 0f;
            if(raceControl != null)
            {
                raceControl.Laps = (byte)laptimes.Count;
                raceControl.LapDistance = trackPosition.minIdx;
            }

            bool isPractice = GameManager.Instance.Settings.isPracticeRun;
            int maxLaps = GameManager.Instance.Settings.maxLaps;

            if(!isPractice && maxLaps > 0 && laptimes.Count - 1 == maxLaps)
            {
                Debug.Log($"Reset due to max laps reached ({maxLaps})", this);
                GameManager.Instance.OnResetEvent(GameResetReason.RaceComplete);
            }

        }
        else
        {
            newLapDetected = false;
        }
        if(raceControl != null)
        {
            raceControl.LapTime = currLaptime;
        }
        minIdxPrev = trackPosition.minIdx;
    }

    CarController GetVehicleForLap()
    {
        return FindObjectsOfType<CarController>()[0];
    }
}
