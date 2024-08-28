using System;
using UnityEngine;

namespace Autonoma
{

public class CanParser
{

	public static int ConvertToMTBitOrdering(uint bit, uint dlc)
	{
		// if(bit > -dlc * 8)
		// {
		// 	return -1;
		// }

		int msg_bit_length = (int)dlc * 8;
		int row = (int)bit / 8;
		int offset = (int)bit % 8;
		return (msg_bit_length - (row + 1) * 8) + offset;
	}

	public static int ConvertToMTBitOrdering(uint bit)
	{
		return ConvertToMTBitOrdering(bit, 8);
	}

	public static double Unpack(byte[] data, CanSignalDef signal)
	{
		sbyte word_size = 8;
		sbyte start_bit = (sbyte)signal.start_bit_index;

		if(signal.is_little_endian)
		{
			start_bit = (sbyte)ConvertToMTBitOrdering((uint)start_bit, (uint)signal.dlc);
		}
		else
		{
			start_bit = (sbyte)(ConvertToMTBitOrdering((uint)start_bit, (uint)signal.dlc) - 
				((int)signal.length - 1));
		}

		int bit = (int)(start_bit % 8);
		bool is_exactly_byte = ((bit + signal.length) % 8 == 0);
		uint num_bytes = (uint)((is_exactly_byte ? 0 : 1) + ((bit + (int)signal.length) / 8));

		int b = (int)word_size - ((int)start_bit / 8) - 1;
		int w = (int)signal.length;
		int mask_shift = bit;
		int right_shift = 0;

		uint unsigned_result = 0;

		for(uint i = 0; i < num_bytes; i++)
		{
			if((b < 0) || (b >= 8))
			{
				return -99999999.0;
			}

			int mask = 0xFF;
			if(w < 8)
			{
				mask >>= (8 - w);
			}
			mask <<= mask_shift;

			int extracted_byte = (data[b] & mask) >> mask_shift;
			unsigned_result |= (uint)(extracted_byte << (int)(8 * i - right_shift));

			if(!signal.is_little_endian)
			{
				if((b % word_size) == 0)
				{
					b += 2 * word_size - 1;
				}
				else
				{
					b--;
				}
			}
			else
			{
				b++;
			}

			w -= (8 - mask_shift);
			right_shift += mask_shift;
			mask_shift = 0;
		}

		double result = 0;
		if(!signal.is_unsigned)
		{
			if((unsigned_result & ((1 << ((int)signal.length) - 1))) != 0)
			{
				if(signal.length < 32)
				{
					uint sign_ext = (uint)(0xFFFFFFFF << (int)signal.length);
					unsigned_result |= sign_ext;

			        //Debug.Log("converting sign", null);
				}
			}
			result = (double)((int)unsigned_result); // Special Cast?
			//result = (double)ReinterpretCast<uint, float>(unsigned_result);
			//result = BitConverter.Int32BitsToSingle((int)unsigned_result);
		}
		else
		{
			result = (double)(unsigned_result); // Special Cast?
		}

		if((signal.scale != 1) || (signal.offset != 0))
		{
			result *= signal.scale;
			result += signal.offset;
		}

        //Debug.Log($" unpacked {result}: ", null);

		return result;
	}

	public static void Pack(ref byte[] data, double signal_value, CanSignalDef signal)
	{
		uint result = 0;
		double tmp = signal_value;

		if((signal.scale != 1) || (signal.offset != 0))
		{
			tmp -= signal.offset;
			tmp /= signal.scale;
		}

		if(!signal.is_unsigned)
		{
			int i = (int)tmp; // Special casts?
			uint u = (uint)i;
			result = u;
		}
		else
		{
			result = (uint)tmp;
		}

        // Debug.Log($" result {result}", null);

		sbyte word_size = 8;
		sbyte start_bit = (sbyte)signal.start_bit_index;

        // Debug.Log($" start bit {start_bit}, dlc {signal.dlc}, sig start {signal.start_bit_index}", null);

		if(signal.is_little_endian)
		{
			start_bit = (sbyte)ConvertToMTBitOrdering(signal.start_bit_index, signal.dlc);
		}
		else
		{
			start_bit = (sbyte)(ConvertToMTBitOrdering(signal.start_bit_index, signal.dlc)
				- ((int)signal.length - 1));
		}

        //Debug.Log($" start bit {start_bit}", null);

		int bit = (int)(start_bit % 8);
		bool is_exactly_byte = ((bit + signal.length) % 8) == 0;
		uint num_bytes = (uint)((is_exactly_byte ? 0 : 1) + ((bit + (int)signal.length) / 8));

		int b = (int)word_size - ((int)start_bit / 8) - 1;
		int w = (int)signal.length;

		int mask_shift = bit;
		int right_shift = 0;

		byte mask = 0xFF;
		uint extracted_byte;

        //Debug.Log($"Can Pack. start bit {start_bit}, nbytes {num_bytes}, b {b}", null);

		for(uint i = 0; i < num_bytes; i++)
		{
			if((b < 0) || (b >= 8))
			{
				return;
			}

			mask = 0xFF;

			if(w < 8)
			{
				mask >>= (8 - w);
			}

			mask <<= mask_shift;

			extracted_byte = (uint)(result >> (ushort)(8 * i - right_shift)) & 0xFF;

			//Debug.Log($"masked byte {data[b] & ~mask}", null);
			//Debug.Log($"shifted {extracted_byte << mask_shift}", null);

			data[b] = (byte)(data[b] & ~mask);
			data[b] |= (byte)((extracted_byte << mask_shift) & mask);

	        //Debug.Log($"data[b] {data[b]}", null);

			if(!signal.is_little_endian)
			{
				if((b % word_size) == 0)
				{
					b += 2 * word_size - 1;
				}
				else
				{
					b--;
				}
			}
			else
			{
				b++;
			}

			w -= (8 - mask_shift);
			right_shift += mask_shift;
			mask_shift = 0;
		}
	}

};

}