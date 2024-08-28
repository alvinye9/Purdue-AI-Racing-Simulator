using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text.RegularExpressions;
using System.Globalization;
using can_msgs.msg;

namespace Autonoma
{

public class CanMessageDef
{
    private static String RgxString = @"BO_ (\d*) ([a-z_A-Z\d]*): (\d) ([a-zA-Z_]*)";
    private static RegexOptions RgxOptions = RegexOptions.Compiled | 
        RegexOptions.Multiline;
    private static Regex Rgx = new Regex(RgxString, RgxOptions);

    public ulong id;
    public String name;
    public byte length;
    public String sender;
    public List<CanSignalDef> signals;

    public CanMessageDef(Match entry_match)
    {
        Match message_match = Rgx.Match(entry_match.Value);
        int i = 1;
        id = ulong.Parse(message_match.Groups[i++].Value);
        name = message_match.Groups[i++].Value;
        length =  8;//(byte)ushort.Parse(message_match.Groups[i++].Value);
        sender = message_match.Groups[i++].Value;
        signals = CanSignalDef.Parse(entry_match);

        foreach(CanSignalDef s in signals)
        {
            s.dlc = length;
        }
    }

    public void ser_data(List<double> signal_data, ref Frame out_frame)
    {
        byte[] data_field = new byte[8];
        for(int i = 0; i < signals.Count; i++)
        {
            CanParser.Pack(ref data_field, signal_data[i], signals[i]);
        }        

        for(int i = 0; i < 8; i++)
        {
            out_frame.Data[i] = data_field[i];
        }
    }

    public List<double> deser_data(byte[] data)
    {
        List<double> out_values = new List<double>();

        for(int i = 0; i < signals.Count; i++)
        {
            double v = CanParser.Unpack(data, signals[i]);
            //Debug.Log(name + $" unpacked: {v}", null);
            out_values.Add(v);
        }

        return out_values;
    }

    public void print()
    {
        Console.WriteLine("ID: {0}, Name: {1}, LEN: {2}, SENDER {3}",
            id, name, length, sender);
        foreach(CanSignalDef s in signals)
        {
            s.print();
        }
    }

    public int get_little_endian_signal_count()
    {
        int ct = 0;
        foreach(CanSignalDef s in signals)
        {
            if(s.is_little_endian)
            {
                ct++;
            }
        }
        return ct;
    }

    public int get_big_endian_signal_count()
    {
        int ct = 0;
        foreach(CanSignalDef s in signals)
        {
            if(!s.is_little_endian)
            {
                ct++;
            }
        }
        return ct;
    }
}

public class CanDBCMessage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

}
