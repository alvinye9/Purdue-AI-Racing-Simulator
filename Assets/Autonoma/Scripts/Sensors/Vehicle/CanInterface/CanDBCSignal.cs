using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Autonoma
{

public class CanSignalDef
{
    private static String RgxString = 
        @" SG_ ([a-z_A-Z\d]+) : (\d+)\|(\d+)@(\d)([-+]) \((-?[\d.]+),(-?[\d.]+)\) \[(-?[\d.]+)\|([\d.]+)\] ""(.*)"" ([a-zA-Z_]+)";
    private static RegexOptions RgxOptions = RegexOptions.Compiled | 
        RegexOptions.Multiline;
    private static Regex Rgx = new Regex(RgxString, RgxOptions);

    public String name;
    public ushort start_bit_index;
    public ushort length;
    public ushort dlc;
    public bool is_little_endian;
    public bool is_unsigned;
    public float scale;
    public float offset;
    public float min_value;
    public float max_value;
    public String units;
    public String receiver;

    public CanSignalDef(GroupCollection rgx_groups)
    {
        int i = 1;
        name = rgx_groups[i++].Value;
        start_bit_index = UInt16.Parse(rgx_groups[i++].Value);
        length = UInt16.Parse(rgx_groups[i++].Value);
        is_little_endian = rgx_groups[i++].Value == "1";
        is_unsigned = rgx_groups[i++].Value == "+";
        scale = float.Parse(rgx_groups[i++].Value, CultureInfo.InvariantCulture.NumberFormat);
        offset = float.Parse(rgx_groups[i++].Value, CultureInfo.InvariantCulture.NumberFormat);
        min_value = float.Parse(rgx_groups[i++].Value, CultureInfo.InvariantCulture.NumberFormat);
        max_value = float.Parse(rgx_groups[i++].Value, CultureInfo.InvariantCulture.NumberFormat);
        units = rgx_groups[i++].Value;
        receiver = rgx_groups[i++].Value;
    }

    public static List<CanSignalDef> Parse(Match entry_match)
    {
        List<CanSignalDef> signals = new List<CanSignalDef>();
        MatchCollection matches = Rgx.Matches(entry_match.Value);
        foreach(Match m in matches)
        {
            signals.Add(new CanSignalDef(m.Groups));
        }
        return signals;
    }

    public ulong get_value_bits(double v)
    {
        double unscaled = (v - offset) / scale;
        ulong bits = (ulong)unscaled;
        return bits;
    }

    /*
            This must be done to avoid having to deal with
            floating point representation (eg, sign bit, mantissa, etc)
    */
    public byte[] value_to_bytes(double v)
    {
        double unscaled = (v - offset) / scale;
        ulong bits = (ulong)unscaled;

        //Debug.Log(name + " value: " + Convert.ToString(bits), null);
        //Debug.Log(name + " value bits: " + Convert.ToString((long)bits, 2), null);

        byte[] bytes = BitConverter.GetBytes(bits);

        if(!is_little_endian)
        {
            //bytes = BitStream.reverse_bit_order(bytes, length);
        }

        //Debug.Log(name + " value: " + BitStream.get_byte_string(bytes), null);
        //Debug.Log(name + " value bits: " + BitStream.get_byte_string(bytes), null);

        return bytes;
    }

    public double bytes_to_value(ulong bytes)
    {
        double v = get_unscaled_value((double)bytes);
        //Debug.Log(name + " value: " + Convert.ToString(v), null);
        return v;
    }

    public double get_scaled_value(double raw_value)
    {
        return (raw_value - offset) / scale;
    }

    public double get_unscaled_value(double scaled_value)
    {
        return (scaled_value * scale) + offset;
    }

    public double get_bits_value(ulong bits)
    {
        //Debug.Log("unreversed raw bits " + name + Convert.ToString((long)bits, 2).PadLeft(length, '0'), null);
       

        //Debug.Log("reveresed raw bits " + name + Convert.ToString((long)bits, 2).PadLeft(length, '0'), null);
        //Debug.Log("scale " + Convert.ToString(scale), null);
        //Debug.Log("offset " + Convert.ToString(offset), null);
        double base_value = (double)bits;
       // Debug.Log("unscaled value " + name + " " + Convert.ToString(base_value));
        double scaled = (base_value * scale) + offset;

        //Debug.Log(name + " value: " + Convert.ToString(scaled), null);

        return scaled;
    }

    public void print()
    {
        Console.WriteLine("\tNAME: {0}, SBI: {1}, LEN: {2}, ILE: {3}, SIGNED: {4}, SCALE: {5}, OFFSET: {6}, MIN: {7}, MAX: {8}, UNITS {9}, RX {10}",
            name, start_bit_index, length, is_little_endian, is_unsigned, scale, offset, min_value, max_value, units, receiver);
    }
}

public class CanDBCSignal : MonoBehaviour
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