/* 
Copyright 2024 Purdue AI Racing
Copyright 2023 Autonoma Inc..

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
public class DriveMenuController : MonoBehaviour
{
    public TextMeshProUGUI FpsText;
	private float pollingTime = 0.25f;
	private float time;
	private int frameCount;

    public Button PauseButton;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PauseButton.onClick.AddListener( GameManager.Instance.UIManager.OnPauseMenuPressed );
    }
    void Update()
    {
    		// Update time.
		time += Time.deltaTime;

		// Count this frame.
		frameCount++;

		if (time >= pollingTime) {
			// Update frame rate.
			int frameRate = Mathf.RoundToInt((float)frameCount / time);
			FpsText.text = frameRate.ToString() + " fps";

			// Reset time and frame count.
			time -= pollingTime;
			frameCount = 0;
		}
    }
}
