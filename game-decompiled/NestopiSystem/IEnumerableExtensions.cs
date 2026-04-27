using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.Pool;

namespace NestopiSystem;

public static class IEnumerableExtensions
{
	private class ListDummy<T>
	{
		internal T[] Items;
	}

	private static readonly Random random = new Random();

	public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> self, int first = 0)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		int i = first;
		foreach (T item in self)
		{
			yield return (item: item, index: i);
			int num = i + 1;
			i = num;
		}
	}

	public static IEnumerable<T> Loop<T>(this IEnumerable<T> source, int count)
	{
		for (int i = 0; i < count; i++)
		{
			foreach (T item in source)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
	{
		T[] arr = source.ToArray();
		int arrCount = arr.Length;
		for (int i = 0; i < count; i++)
		{
			yield return arr[i % arrCount];
		}
	}

	public static T RepeatableIndexed<T>(this IReadOnlyList<T> source, int index)
	{
		return source[index % source.Count];
	}

	public static bool InBounded<T>(this IReadOnlyCollection<T> self, int index)
	{
		if (self == null || !self.Any())
		{
			return false;
		}
		if (0 <= index)
		{
			return index < self.Count;
		}
		return false;
	}

	public static void ClearFast<T>(this List<T> list)
	{
		if (list.Count * 2 < list.Capacity)
		{
			list.RemoveRange(0, list.Count);
		}
		else
		{
			list.Clear();
		}
	}

	public static void Move<T>(this T[] source, int sourceIndex, int destinationIndex)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (sourceIndex < 0 || sourceIndex >= source.Length)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destinationIndex < 0 || destinationIndex >= source.Length)
		{
			throw new ArgumentOutOfRangeException("destinationIndex");
		}
		if (sourceIndex != destinationIndex)
		{
			T val = source[sourceIndex];
			if (sourceIndex < destinationIndex)
			{
				Array.Copy(source, sourceIndex + 1, source, sourceIndex, destinationIndex - sourceIndex);
			}
			else
			{
				Array.Copy(source, destinationIndex, source, destinationIndex + 1, sourceIndex - destinationIndex);
			}
			source[destinationIndex] = val;
		}
	}

	public static void Move<T>(this T[] source, Index sourceIndex, Index destinationIndex)
	{
		int length = source.Length;
		source.Move(sourceIndex.GetOffset(length), destinationIndex.GetOffset(length));
	}

	public static void Move<T>(this IList<T> list, int sourceIndex, int destinationIndex)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (sourceIndex < 0 || sourceIndex >= list.Count)
		{
			throw new ArgumentOutOfRangeException("sourceIndex");
		}
		if (destinationIndex < 0 || destinationIndex >= list.Count)
		{
			throw new ArgumentOutOfRangeException("destinationIndex");
		}
		if (sourceIndex == destinationIndex)
		{
			return;
		}
		if (sourceIndex < destinationIndex)
		{
			for (int i = sourceIndex; i < destinationIndex; i++)
			{
				int index = i;
				IList<T> list2 = list;
				int index2 = i + 1;
				T val = list[i + 1];
				T val2 = list[i];
				T val3 = (list[index] = val);
				val3 = (list2[index2] = val2);
			}
			return;
		}
		for (int num = sourceIndex; num > destinationIndex; num--)
		{
			int index2 = num;
			IList<T> list2 = list;
			int index = num - 1;
			T val2 = list[num - 1];
			T val = list[num];
			T val3 = (list[index2] = val2);
			val3 = (list2[index] = val);
		}
	}

	public static bool AllContent<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target)
	{
		return !source.Except(target).Any();
	}

	public static int IndexOf<T>(this IReadOnlyList<T> source, T item)
	{
		for (int i = 0; i < source.Count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(source[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public static IEnumerable<(TKey, TValue)> ToTuple<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source)
	{
		foreach (var (item, item2) in source)
		{
			yield return (item, item2);
		}
	}

	public static Queue<T> ToQueue<T>(this IEnumerable<T> source)
	{
		return new Queue<T>(source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> AsSpan<T>(this List<T> self)
	{
		return MemoryExtensions.AsSpan(Unsafe.As<ListDummy<T>>(self).Items, 0, self.Count);
	}

	public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
	{
		if (chunkSize <= 0)
		{
			throw new ArgumentException("Chunk size must be greater than 0.", "chunkSize");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			yield return GetChunk(enumerator, chunkSize);
		}
	}

	private static IEnumerable<T> GetChunk<T>(IEnumerator<T> enumerator, int chunkSize)
	{
		int num;
		do
		{
			yield return enumerator.Current;
			num = chunkSize - 1;
			chunkSize = num;
		}
		while (num > 0 && enumerator.MoveNext());
	}

	public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] source)
	{
		return MemoryExtensions.AsSpan(source);
	}

	public static bool TryAsSpan<T>(this IEnumerable<T> source, out Span<T> span)
	{
		if (source is T[] array)
		{
			span = MemoryExtensions.AsSpan(array);
			return true;
		}
		if (source is List<T> self)
		{
			span = self.AsSpan();
			return true;
		}
		span = Span<T>.Empty;
		return false;
	}

	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
	{
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (enumerator.MoveNext())
		{
			return enumerator.Current;
		}
		return defaultValue;
	}

	public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				return item;
			}
		}
		return defaultValue;
	}

	public static TSource FirstOrDefault<TSource, TState>(this IEnumerable<TSource> source, TState state, Func<TSource, TState, bool> predicate)
	{
		return source.FirstOrDefault(state, predicate, default(TSource));
	}

	public static TSource FirstOrDefault<TSource, TState>(this IEnumerable<TSource> source, TState state, Func<TSource, TState, bool> predicate, TSource defaultValue)
	{
		foreach (TSource item in source)
		{
			if (predicate(item, state))
			{
				return item;
			}
		}
		return defaultValue;
	}

	public static bool TryGetFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource first)
	{
		foreach (TSource item in source)
		{
			if (predicate(item))
			{
				first = item;
				return true;
			}
		}
		first = default(TSource);
		return false;
	}

	public static bool TryGetFirst<TSource, TState>(this IEnumerable<TSource> source, TState state, Func<TSource, TState, bool> predicate, out TSource first)
	{
		foreach (TSource item in source)
		{
			if (predicate(item, state))
			{
				first = item;
				return true;
			}
		}
		first = default(TSource);
		return false;
	}

	public static bool TryGetFirst<TSource>(this IEnumerable<TSource> source, out TSource first)
	{
		using (IEnumerator<TSource> enumerator = source.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				TSource current = enumerator.Current;
				first = current;
				return true;
			}
		}
		first = default(TSource);
		return false;
	}

	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, TSource defaultValue)
	{
		using IEnumerator<TSource> enumerator = source.Reverse().GetEnumerator();
		if (enumerator.MoveNext())
		{
			return enumerator.Current;
		}
		return defaultValue;
	}

	public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, TSource defaultValue)
	{
		foreach (TSource item in source.Reverse())
		{
			if (predicate(item))
			{
				return item;
			}
		}
		return defaultValue;
	}

	public static int IndexOf<T>(this IList<T> source, T item, IEqualityComparer<T> equalityComparer)
	{
		for (int i = 0; i < source.Count; i++)
		{
			if (equalityComparer.Equals(item, source[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static IEnumerable<(T1, T2)> JoinTuple<T1, T2>(this IEnumerable<T1> source1, IEnumerable<T2> source2)
	{
		using IEnumerator<T1> e1 = source1.GetEnumerator();
		using IEnumerator<T2> e2 = source2.GetEnumerator();
		while (e1.MoveNext() && e2.MoveNext())
		{
			yield return (e1.Current, e2.Current);
		}
	}

	public static IEnumerable<(T1, T2, T3)> JoinTuple<T1, T2, T3>(this IEnumerable<T1> source1, IEnumerable<T2> source2, IEnumerable<T3> source3)
	{
		using IEnumerator<T1> e1 = source1.GetEnumerator();
		using IEnumerator<T2> e2 = source2.GetEnumerator();
		using IEnumerator<T3> e3 = source3.GetEnumerator();
		while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
		{
			yield return (e1.Current, e2.Current, e3.Current);
		}
	}

	public static IEnumerable<(T1, T2)> OuterJoinTuple<T1, T2>(this IEnumerable<T1> source1, IEnumerable<T2> source2)
	{
		using IEnumerator<T1> e1 = source1.GetEnumerator();
		using IEnumerator<T2> e2 = source2.GetEnumerator();
		while (true)
		{
			bool flag = e1.MoveNext();
			bool flag2 = e2.MoveNext();
			if (flag || flag2)
			{
				T1 item = (flag ? e1.Current : default(T1));
				T2 item2 = (flag2 ? e2.Current : default(T2));
				yield return (item, item2);
				continue;
			}
			break;
		}
	}

	public static IEnumerable<(T1, T2, T3)> OuterJoinTuple<T1, T2, T3>(this IEnumerable<T1> source1, IEnumerable<T2> source2, IEnumerable<T3> source3)
	{
		using IEnumerator<T1> e1 = source1.GetEnumerator();
		using IEnumerator<T2> e2 = source2.GetEnumerator();
		using IEnumerator<T3> e3 = source3.GetEnumerator();
		while (true)
		{
			bool flag = e1.MoveNext();
			bool flag2 = e2.MoveNext();
			bool flag3 = e3.MoveNext();
			if (flag || flag2 || flag3)
			{
				T1 item = (flag ? e1.Current : default(T1));
				T2 item2 = (flag2 ? e2.Current : default(T2));
				T3 item3 = (flag3 ? e3.Current : default(T3));
				yield return (item, item2, item3);
				continue;
			}
			break;
		}
	}

	public static IEnumerable<T> Concat<T>(this IEnumerable<T> self, T last)
	{
		foreach (T item in self)
		{
			yield return item;
		}
		yield return last;
	}

	public static IEnumerable<T> Concat<T>(this T first, IEnumerable<T> items)
	{
		yield return first;
		foreach (T item in items)
		{
			yield return item;
		}
	}

	public static IEnumerable<TSource> ZipAlternate<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> source2)
	{
		return source.Zip(source2, (TSource x, TSource y) => new TSource[2] { x, y }).SelectMany((TSource[] xs) => xs);
	}

	public static IEnumerable<TSource> ConcatAlternate<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> source2)
	{
		using IEnumerator<TSource> e1 = source.GetEnumerator();
		using IEnumerator<TSource> e2 = source2.GetEnumerator();
		while (true)
		{
			bool flag = e1.MoveNext();
			bool e2Next = e2.MoveNext();
			if (flag || e2Next)
			{
				if (flag)
				{
					yield return e1.Current;
				}
				if (e2Next)
				{
					yield return e2.Current;
				}
				continue;
			}
			break;
		}
	}

	public static T Select<T>(this ReadOnlySpan<T> table)
	{
		if (table.Length != 0)
		{
			return table[random.Next(0, table.Length)];
		}
		return default(T);
	}

	public static T Select<T>(this Span<T> table)
	{
		return ((ReadOnlySpan<T>)table).Select();
	}

	public static T Select<T>(this IEnumerable<T> table)
	{
		if (table.TryAsSpan(out var span))
		{
			return ((ReadOnlySpan<T>)span).Select();
		}
		if (table is IReadOnlyList<T> readOnlyList)
		{
			if (readOnlyList.Count != 0)
			{
				return readOnlyList[random.Next(0, readOnlyList.Count)];
			}
			return default(T);
		}
		T[] array = table.ToArray();
		if (array.Length != 0)
		{
			return array[random.Next(0, array.Length)];
		}
		return default(T);
	}

	public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
	{
		if (source is IList<T> source2)
		{
			return source2.Shuffle();
		}
		T[] array = source.ToArray();
		array.Shuffle();
		return array;
	}

	public static IList<T> Shuffle<T>(this IList<T> source)
	{
		if (source.TryAsSpan(out var span))
		{
			span.Shuffle();
			return source;
		}
		for (int i = 0; i < source.Count; i++)
		{
			int num = random.Next(0, source.Count);
			int index = i;
			int index2 = num;
			T val = source[num];
			T val2 = source[i];
			T val3 = (source[index] = val);
			val3 = (source[index2] = val2);
		}
		return source;
	}

	public static T[] Shuffle<T>(this T[] source)
	{
		MemoryExtensions.AsSpan(source).Shuffle();
		return source;
	}

	public static List<T> Shuffle<T>(this List<T> source)
	{
		source.AsSpan().Shuffle();
		return source;
	}

	public static Memory<T> Shuffle<T>(this Memory<T> source)
	{
		source.Span.Shuffle();
		return source;
	}

	public static Span<T> Shuffle<T>(this Span<T> source)
	{
		for (int i = 0; i < source.Length; i++)
		{
			int index = random.Next(0, source.Length);
			ref T reference = ref source[i];
			ref T reference2 = ref source[index];
			T val = source[index];
			T val2 = source[i];
			reference = val;
			reference2 = val2;
		}
		return source;
	}

	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
	{
		HashSet<TKey> keys;
		using (CollectionPool<HashSet<TKey>, TKey>.Get(out keys))
		{
			foreach (TSource item in source)
			{
				if (keys.Add(selector(item)))
				{
					yield return item;
				}
			}
		}
	}

	private static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, ISet<TKey> sharedSet, Func<TSource, TKey> selector)
	{
		foreach (TSource item in source)
		{
			if (sharedSet.Add(selector(item)))
			{
				yield return item;
			}
		}
	}

	public static (IEnumerable<TSource>, IEnumerable<TSource>) DistinctBy<TSource, TKey>(this IEnumerable<TSource> source1, IEnumerable<TSource> source2, Func<TSource, TKey> selector)
	{
		HashSet<TKey> value;
		using (CollectionPool<HashSet<TKey>, TKey>.Get(out value))
		{
			return (source1.DistinctBy(value, selector), source2.DistinctBy(value, selector));
		}
	}

	public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
	{
		HashSet<TKey> set = new HashSet<TKey>(second, comparer);
		foreach (TSource item in first)
		{
			if (set.Remove(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
	{
		HashSet<TKey> set = new HashSet<TKey>(second, comparer);
		foreach (TSource item in first)
		{
			if (set.Add(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static List<T> Slide<T>(this List<T> self, int vec)
	{
		if (vec < 0)
		{
			return self.LeftSlide();
		}
		return self.RightSlide();
	}

	public static List<T> Slide<T>(this List<T> self, float vec)
	{
		if (!(vec >= 0f))
		{
			return self.LeftSlide();
		}
		return self.RightSlide();
	}

	public static List<T> RightSlide<T>(this List<T> self)
	{
		T value = self[self.Count - 1];
		foreach (int item in Enumerable.Range(0, self.Count - 1).Reverse())
		{
			self[item + 1] = self[item];
		}
		self[0] = value;
		return self;
	}

	public static List<T> LeftSlide<T>(this List<T> self)
	{
		T value = self[0];
		foreach (int item in Enumerable.Range(1, self.Count - 1))
		{
			self[item - 1] = self[item];
		}
		self[self.Count - 1] = value;
		return self;
	}

	public static T[] Slide<T>(this T[] self, int vec)
	{
		if (vec < 0)
		{
			return self.LeftSlide();
		}
		return self.RightSlide();
	}

	public static T[] Slide<T>(this T[] self, float vec)
	{
		if (!(vec >= 0f))
		{
			return self.LeftSlide();
		}
		return self.RightSlide();
	}

	public static T[] RightSlide<T>(this T[] self)
	{
		T val = self[^1];
		foreach (int item in Enumerable.Range(0, self.Length - 1).Reverse())
		{
			self[item + 1] = self[item];
		}
		self[0] = val;
		return self;
	}

	public static T[] LeftSlide<T>(this T[] self)
	{
		T val = self[0];
		foreach (int item in Enumerable.Range(1, self.Length - 1))
		{
			self[item - 1] = self[item];
		}
		self[^1] = val;
		return self;
	}
}
