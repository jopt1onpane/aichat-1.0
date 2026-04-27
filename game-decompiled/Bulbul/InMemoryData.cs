using System;
using System.Collections.Generic;
using Bulbul.Mobile;
using Bulbul.Web;

namespace Bulbul;

public static class InMemoryData
{
	private static class Cache<T>
	{
		public static T Data;

		public static bool HasData;
	}

	public static bool TryGetData<T>(out T data)
	{
		data = default(T);
		if (!Cache<T>.HasData)
		{
			return false;
		}
		data = Cache<T>.Data;
		return true;
	}

	public static T GetOrSet<T>(Func<T> defaultValueFunc)
	{
		if (Cache<T>.HasData)
		{
			return Cache<T>.Data;
		}
		Cache<T>.HasData = true;
		return Cache<T>.Data = defaultValueFunc();
	}

	public static void SetData<T>(T data)
	{
		Cache<T>.HasData = true;
		Cache<T>.Data = data;
	}

	public static void Clear<T>()
	{
		Cache<T>.HasData = false;
		Cache<T>.Data = default(T);
	}

	public static void ClearAll()
	{
		Clear<List<AccountType>>();
		Clear<NewsState>();
		Clear<UnlockProducts>();
		Clear<ReviewState>();
		Clear<ShopState>();
	}
}
