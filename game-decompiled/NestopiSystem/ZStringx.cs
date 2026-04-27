using System;
using Cysharp.Text;

namespace NestopiSystem;

public static class ZStringx
{
	public static string Format(string format, ReadOnlySpan<string> args)
	{
		if (args.Length == 0)
		{
			return format;
		}
		if (args.Length == 1)
		{
			return ZString.Format(format, args[0]);
		}
		if (args.Length == 2)
		{
			return ZString.Format(format, args[0], args[1]);
		}
		if (args.Length == 3)
		{
			return ZString.Format(format, args[0], args[1], args[2]);
		}
		if (args.Length == 4)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3]);
		}
		if (args.Length == 5)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4]);
		}
		if (args.Length == 6)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5]);
		}
		if (args.Length == 7)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
		}
		if (args.Length == 8)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
		}
		if (args.Length == 9)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
		}
		if (args.Length == 10)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
		}
		if (args.Length == 11)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
		}
		if (args.Length == 12)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
		}
		if (args.Length == 13)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
		}
		if (args.Length == 14)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
		}
		if (args.Length == 15)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
		}
		return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
	}

	public static string Format(ReadOnlySpan<char> format, ReadOnlySpan<string> args)
	{
		if (args.Length == 0)
		{
			return format.ToString();
		}
		if (args.Length == 1)
		{
			return ZString.Format(format, args[0]);
		}
		if (args.Length == 2)
		{
			return ZString.Format(format, args[0], args[1]);
		}
		if (args.Length == 3)
		{
			return ZString.Format(format, args[0], args[1], args[2]);
		}
		if (args.Length == 4)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3]);
		}
		if (args.Length == 5)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4]);
		}
		if (args.Length == 6)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5]);
		}
		if (args.Length == 7)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
		}
		if (args.Length == 8)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
		}
		if (args.Length == 9)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
		}
		if (args.Length == 10)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
		}
		if (args.Length == 11)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
		}
		if (args.Length == 12)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
		}
		if (args.Length == 13)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
		}
		if (args.Length == 14)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
		}
		if (args.Length == 15)
		{
			return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
		}
		return ZString.Format(format, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
	}
}
