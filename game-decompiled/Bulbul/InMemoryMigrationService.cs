using System.Collections.Generic;

namespace Bulbul;

public static class InMemoryMigrationService
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
		Clear<NoteDataV2>();
		Clear<MusicSettingV2>();
		Clear<LocalMusicSetting>();
		Clear<List<CalenderMonthlyData>>();
		Clear<TodoForCompletedFixed>();
	}
}
