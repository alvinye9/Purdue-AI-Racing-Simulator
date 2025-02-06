#!/bin/bash

# Set environment variables (modify as needed)
export ROS_DOMAIN_ID=0
export RMW_FASTRTPS_TRANSPORT=udp

find_unity_editor() {
    UNITY_EDITOR_PATH=$(ls -d /home/$USER/Unity/Hub/Editor/2021.3.45f1/Editor/Unity | tail -n 1)
    if [ -x "$UNITY_EDITOR_PATH" ]; then
        echo "$UNITY_EDITOR_PATH"
    else
        echo "Unity Editor not found in expected directory!" >&2
        exit 1
    fi
}

# Define Unity path and project

UNITY_PATH=${UNITY_PATH_OVERRIDE:-$(find_unity_editor)}
PROJECT_PATH="./"
LOG_FILE="build.log"

# Display message
echo "Starting Unity build process in batch mode..."
date

# Run Unity in batch mode
$UNITY_PATH -batchmode -nographics -quit \
    -projectPath $PROJECT_PATH \
    -buildTarget Linux64 \
    -executeMethod BuildScript.BuildLinux \
    -logFile $LOG_FILE

# Check for errors
if [ $? -eq 0 ]; then
    echo "Build completed successfully."
else
    echo "Build failed. Check $LOG_FILE for details."
    exit 1
fi

echo "Build process finished."
date


# /home/blackandgold/Unity/Hub/Editor/2021.3.45f1/Editor/Unity     -batchmode -nographics -quit     -projectPath ./Purdue-AI-Racing-Simulator     -buildTarget Linux64     -executeMethod BuildScript.BuildLinux     -logFile build.log