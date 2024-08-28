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

public class TrackPositionFinder : MonoBehaviour
{
    public Transform car;
    public TrackInfo trackInfo;
    public float minDist;
    public float currHeading;
    public float lateralError;
    public int minIdx;
    public Vector3 minPos;
    public Vector3 carPos;
    public float dxMin;
    public float dzMin;
    public float maxIdx; 

    void Start()
    {
        trackInfo = GameManager.Instance.Settings.myTrackParams.trackInfo;
        maxIdx = trackInfo.innerX.Count;
    }

    void Update()
    {   
        minDist = 9999;
        carPos = car.position;

        for (int i = 0; i<trackInfo.innerX.Count-1; i++)
        {   
            float dx = carPos.x - (float)trackInfo.innerX[i];
            float dz = carPos.z - (float)trackInfo.innerZ[i];
            float dist = Mathf.Sqrt(dx*dx + dz*dz);
   
            if (dist < minDist)
            {
                minDist = dist;
                minIdx = i;
                dxMin = dx;
                dzMin = dz;
            }
        }

        /*lateralError = dxMin * Mathf.Cos ((float)trackInfo.heading[minIdx] * Mathf.PI/180f) 
                     - dzMin * Mathf.Sin ((float)trackInfo.heading[minIdx] * Mathf.PI/180f);
    */
    }

    int GetClosestPositionIndex(Vector3 pos)
    {
        int ci = 0;
        float md = 999999f;
        for(int i = 0; i < trackInfo.innerX.Count - 1; i++)
        {
            float dx = pos.x - (float)trackInfo.innerX[i];
            float dz = pos.z - (float)trackInfo.innerZ[i];
            float d = Mathf.Sqrt(dx*dx + dz*dz);
            if(d < md)
            {
                md = d;
                ci = i;
            }
        }
        return ci;
    }

    float GetDistanceBetweenIndecies(int a, int b)
    {
        float dx = (float)trackInfo.innerX[a] - (float)trackInfo.innerX[b];
        float dz = (float)trackInfo.innerZ[a] - (float)trackInfo.innerZ[b];
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    public float GetCarRelativeTrackDistance(Vector3 pos)
    {
        int ri = GetClosestPositionIndex(pos);
        int i = minIdx;

        if(ri == i)
        {
            return 0.0f;
        }

        int i0 = i;
        int i1 = i;
        float d0 = 0.0f;
        float d1 = 0.0f;
        int ct = 0;
        while(ct < trackInfo.innerX.Count)
        {
            int i0n = (i0 + 1) % trackInfo.innerX.Count;
            int i1n = ((i1 - 1) + trackInfo.innerX.Count) % trackInfo.innerX.Count;
            d0 += GetDistanceBetweenIndecies(i0, i0n);
            d1 += GetDistanceBetweenIndecies(i1, i1n);

            if(i0n == ri)
            {
                return d0;
            }
            else if(i1n == ri)
            {
                return -d1;
            }
            i0 = i0n;
            i1 = i1n;
            ct ++;
        }
        return 0;
    }

    public Vector3 GetAheadPoint(float distance)
    {
        int next = distance > 0.0f ? (minIdx + 1) % trackInfo.innerX.Count :
            ((minIdx - 1) + trackInfo.innerX.Count) % trackInfo.innerX.Count;
        if(distance > 0.0f)
        {
            while(GetDistanceBetweenIndecies(minIdx, next) < distance)
            {
                next = (next + 1) % trackInfo.innerX.Count;
            }
        }
        else
        {
            while(GetDistanceBetweenIndecies(minIdx, next) < distance)
            {
                next = ((next - 1) + trackInfo.innerX.Count) % trackInfo.innerX.Count;
            }
        }
        int I = next;
        return new Vector3((float)trackInfo.innerX[I], 
            (float)trackInfo.innerY[I], (float)trackInfo.innerZ[I]);
    }
}
