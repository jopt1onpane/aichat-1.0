using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NestopiSystem;

public static class PrimitiveExtensions
{
	private static class Error
	{
		public static void ThrowIfRightIsGreater(int left, int right)
		{
			if (left < right)
			{
				throw new InvalidOperationException("The right number is greater than the left number.");
			}
		}

		public static void ThrowIfLeftIsGreater(int left, int right)
		{
			if (left > right)
			{
				throw new InvalidOperationException("The left number is greater than the left number.");
			}
		}
	}

	private static TimeZoneInfo jstTimeZone;

	public static string ReplaceCharAt(this string str, int index, char newChar)
	{
		Span<char> span = str.ToCharArray();
		span[index] = newChar;
		return span.ToString();
	}

	public static TResult With<TSource, TResult>(this TSource self, TResult result)
	{
		return result;
	}

	public static T With<T>(this object self, T result)
	{
		return result;
	}

	public static T With<T>(this object self)
	{
		return default(T);
	}

	public static int ToSign(this bool self)
	{
		if (!self)
		{
			return -1;
		}
		return 1;
	}

	public static bool IsNullOrDestroy(this object self)
	{
		if (self is UnityEngine.Object obj)
		{
			return obj == null;
		}
		return self == null;
	}

	public static bool IsEven(this int self)
	{
		return (self & 1) == 0;
	}

	public static int Repeat(this int self, int max)
	{
		return Repeat(self, 0, max);
	}

	public static int Repeat(int self, int start, int end)
	{
		Error.ThrowIfLeftIsGreater(start, end);
		int num = end - start + 1;
		return ((self - start) % num + num) % num + start;
	}

	public static bool IsNullOrEmpty(this string self)
	{
		return string.IsNullOrEmpty(self);
	}

	public static bool TrueExchange(this ref bool location1)
	{
		return BoolUtility.TrueExchange(ref location1);
	}

	public static DateTime ToJst(this DateTime dateTime)
	{
		if (jstTimeZone == null)
		{
			jstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
		}
		return TimeZoneInfo.ConvertTime(dateTime, jstTimeZone);
	}

	public static bool IsBetween(this DateTime self, DateTime startTime, DateTime endTime)
	{
		if (self >= startTime)
		{
			return self <= endTime;
		}
		return false;
	}

	public static bool IsSameDay(this DateTime self, DateTime target)
	{
		if (self.Year == target.Year && self.Month == target.Month)
		{
			return self.Day == target.Day;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasAllFlag<TEnum>(this TEnum self, TEnum target) where TEnum : struct, Enum
	{
		switch (Unsafe.SizeOf<TEnum>())
		{
		case 1:
		{
			byte num7 = Unsafe.As<TEnum, byte>(ref self);
			byte b = Unsafe.As<TEnum, byte>(ref target);
			return (num7 & b) == b;
		}
		case 2:
		{
			ushort num5 = Unsafe.As<TEnum, ushort>(ref self);
			ushort num6 = Unsafe.As<TEnum, ushort>(ref target);
			return (num5 & num6) == num6;
		}
		case 4:
		{
			uint num3 = Unsafe.As<TEnum, uint>(ref self);
			uint num4 = Unsafe.As<TEnum, uint>(ref target);
			return (num3 & num4) == num4;
		}
		case 8:
		{
			ulong num = Unsafe.As<TEnum, ulong>(ref self);
			ulong num2 = Unsafe.As<TEnum, ulong>(ref target);
			return (num & num2) == num2;
		}
		default:
			throw new InvalidOperationException("Unknown enum size");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasAnyFlag<TEnum>(this TEnum self, TEnum target) where TEnum : struct, Enum
	{
		return Unsafe.SizeOf<TEnum>() switch
		{
			1 => (Unsafe.As<TEnum, byte>(ref self) & Unsafe.As<TEnum, byte>(ref target)) != 0, 
			2 => (Unsafe.As<TEnum, short>(ref self) & Unsafe.As<TEnum, short>(ref target)) != 0, 
			4 => (Unsafe.As<TEnum, int>(ref self) & Unsafe.As<TEnum, int>(ref target)) != 0, 
			8 => (Unsafe.As<TEnum, long>(ref self) & Unsafe.As<TEnum, long>(ref target)) != 0, 
			_ => throw new InvalidOperationException("Unknown enum size"), 
		};
	}
}
