using System;

namespace NestopiSystem;

public static class SpanExtensions
{
	public static Span<T> Take<T>(this Span<T> span, int count)
	{
		if (count > 0)
		{
			Span<T> span2 = span;
			return span2.Slice(0, Math.Min(count, span.Length));
		}
		return Span<T>.Empty;
	}

	public static ReadOnlySpan<T> Take<T>(this ReadOnlySpan<T> span, int count)
	{
		if (count > 0)
		{
			ReadOnlySpan<T> readOnlySpan = span;
			return readOnlySpan.Slice(0, Math.Min(count, span.Length));
		}
		return ReadOnlySpan<T>.Empty;
	}

	public static int Count<TSource>(this Span<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource arg = span2[i];
			if (predicate(arg))
			{
				num++;
			}
		}
		return num;
	}

	public static int Count<TSource>(this ReadOnlySpan<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource arg = readOnlySpan[i];
			if (predicate(arg))
			{
				num++;
			}
		}
		return num;
	}

	public static int Count<TSource, TState>(this Span<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource arg = span2[i];
			if (predicate(arg, state))
			{
				num++;
			}
		}
		return num;
	}

	public static int Count<TSource, TState>(this ReadOnlySpan<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return 0;
		}
		int num = 0;
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource arg = readOnlySpan[i];
			if (predicate(arg, state))
			{
				num++;
			}
		}
		return num;
	}

	public static TSource FirstOrDefault<TSource>(this Span<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return default(TSource);
		}
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource val = span2[i];
			if (predicate(val))
			{
				return val;
			}
		}
		return default(TSource);
	}

	public static TSource FirstOrDefault<TSource>(this ReadOnlySpan<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return default(TSource);
		}
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource val = readOnlySpan[i];
			if (predicate(val))
			{
				return val;
			}
		}
		return default(TSource);
	}

	public static TSource FirstOrDefault<TSource, TState>(this Span<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return default(TSource);
		}
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource val = span2[i];
			if (predicate(val, state))
			{
				return val;
			}
		}
		return default(TSource);
	}

	public static TSource FirstOrDefault<TSource, TState>(this ReadOnlySpan<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return default(TSource);
		}
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource val = readOnlySpan[i];
			if (predicate(val, state))
			{
				return val;
			}
		}
		return default(TSource);
	}

	public static bool Any<TSource, TState>(this Span<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return false;
		}
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource arg = span2[i];
			if (predicate(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any<TSource, TState>(this ReadOnlySpan<TSource> span, Func<TSource, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return false;
		}
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource arg = readOnlySpan[i];
			if (predicate(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any<TSource, TState>(this Span<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return false;
		}
		Span<TSource> span2 = span;
		for (int i = 0; i < span2.Length; i++)
		{
			TSource arg = span2[i];
			if (predicate(arg, state))
			{
				return true;
			}
		}
		return false;
	}

	public static bool Any<TSource, TState>(this ReadOnlySpan<TSource> span, TState state, Func<TSource, TState, bool> predicate)
	{
		if (span.IsEmpty)
		{
			return false;
		}
		ReadOnlySpan<TSource> readOnlySpan = span;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			TSource arg = readOnlySpan[i];
			if (predicate(arg, state))
			{
				return true;
			}
		}
		return false;
	}
}
