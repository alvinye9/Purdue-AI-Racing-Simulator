using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ROS2;
using AWSIM;
using autonoma_msgs.msg;
using can_msgs.msg;
using System;

namespace Autonoma
{

public class CanSubscriber
{
	CanMessageDef msgDef;
	public delegate void OnSignalDataDelegate(List<double> signal_data);
	public OnSignalDataDelegate OnSignalData;

	ISubscription<Frame> sub;

	public CanSubscriber(String canMsgName, QoSSettings qos, OnSignalDataDelegate OnSignal)
	{
		OnSignalData = OnSignal;
		msgDef = CanDBCManager.GetMsgDef(canMsgName);
		sub = SimulatorROS2Node.CreateSubscription<Frame>("to_can_bus", OnMsg, qos.GetQoSProfile());
	}

	private void OnMsg(Frame msg)
	{
		if(msg.Id == msgDef.id)
		{
	        //Debug.Log("Deser Command " + msgDef.name, null);
	        //Debug.Log("parsing can data: " + CanFrameBits.ByteArrayToString(msg.Data), null);

			// old way
			//List<double> signal_data = msgDef.deserialize_data(msg.Data);

			// other old way
			//List<double> signal_data = msgDef.stream_deserialize_data(msg.Data);

			List<double> signal_data = msgDef.deser_data(msg.Data);

			OnSignalData(signal_data);
		}
		else
		{
	        //Debug.Log(msgDef.name + Convert.ToString(msg.Id) + " != " + Convert.ToString(msgDef.id), null);
			//Console.WriteLine("{0} != {1}", msg.Id, msgDef.id);
		}
	}

}

}