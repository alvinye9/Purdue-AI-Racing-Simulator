// Copyright 2024 Purdue AI Racing
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
