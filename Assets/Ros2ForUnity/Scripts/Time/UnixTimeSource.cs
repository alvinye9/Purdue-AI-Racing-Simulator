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

using System;
using UnityEngine;

namespace ROS2
{
    /// <summary>
    /// Acquires Unix time.
    /// </summary>
    public class UnixTimeSource : ITimeSource
    {
        public void GetTime(out int seconds, out uint nanoseconds)
        {
        // Get current DateTimeOffset
            DateTimeOffset now = DateTimeOffset.UtcNow;
    
            // Get Unix time in milliseconds
            long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
    
            // Calculate seconds and nanoseconds
            seconds = (int)(unixTimeMilliseconds / 1000);
            nanoseconds = (uint)((unixTimeMilliseconds % 1000) * 1000000);
            
        }
    }
}  // namespace ROS2
