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
using System;
using UnityEngine;
using ROS2;
using AWSIM;
using autonoma_msgs.msg;
using can_msgs.msg;

namespace Autonoma
{
    public class CanPublisher
    {
        public string canMsgName;
        CanMessageDef msgDef;
        Frame msg;
        static IPublisher<Frame> publisher;

        public uint counter = 0;

        public CanPublisher(String canMsgName, String rosNamespace, QoSSettings qos)
        {
            this.canMsgName = canMsgName;
            msg = new Frame();
            msgDef = CanDBCManager.GetMsgDef(canMsgName);
            msg.Id = (uint)msgDef.id;
            msg.Is_extended = false;
            msg.Is_rtr = false;
            msg.Is_error = false;
            msg.Dlc = msgDef.length;

            String fullTopicName = "from_can_bus";

            if(publisher == null)
            {
                publisher = SimulatorROS2Node.CreatePublisher<Frame>(fullTopicName, qos.GetQoSProfile());
            }
        }

        public float GetCounterNext()
        {
            return counter++;
        }

        public void Publish(List<double> values)
        {
            if(values.Count != msgDef.signals.Count)
            {
                Debug.Log("CAN signal count mismatch " + Convert.ToString(msgDef.signals.Count)  + " in def" + 
                    "for can pub " + canMsgName, null);
            }

            // old way
            //msgDef.serialize_data(values, ref msg);

            // other old way
            //msgDef.stream_serialize_data(values, ref msg);

            msgDef.ser_data(values, ref msg);

            var header = msg as MessageWithHeader;
            SimulatorROS2Node.UpdateROSTimestamp(ref header);
            publisher.Publish(msg);
        }
    }
}
