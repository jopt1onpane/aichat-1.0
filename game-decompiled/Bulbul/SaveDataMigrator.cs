using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class SaveDataMigrator
{
	[Inject]
	private SaveDataIO saveDataIO;

	public readonly string[] Versions = new string[4]
	{
		Path.Combine("Demo", "v1"),
		Path.Combine("Release", "v1"),
		Path.Combine("Demo", "v2"),
		Path.Combine("Release", "v2")
	};

	private int latestVersionIndex => Versions.Length - 1;

	public string LatestVersion => Versions[latestVersionIndex];

	public bool WasMigrated { get; private set; }

	public async UniTask<SlideFadeAnnounceDirection.AsyncPlayScope> Migrate()
	{
		if (!ES3.DirectoryExists("SaveData"))
		{
			return default(SlideFadeAnnounceDirection.AsyncPlayScope);
		}
		bool flag = false;
		string path = SteamUser.GetSteamID().ToString();
		string fullPath = new ES3Settings("SaveData").FullPath;
		string fullPath2 = new ES3Settings(BulbulConstant.CreateSaveDirectoryPath(LatestVersion)).FullPath;
		int num;
		string sourceDir;
		if (ExistsDirectory(Path.Combine(fullPath, "Release", "v2", path)))
		{
			num = 2;
			sourceDir = Path.Combine(fullPath, "Release", "v2", path);
		}
		else if (ExistsDirectory(Path.Combine(fullPath, "Release", "v1", path)))
		{
			num = 1;
			sourceDir = Path.Combine(fullPath, "Release", "v1", path);
		}
		else if (ExistsDirectory(Path.Combine(fullPath, "Demo", "v2", path)))
		{
			flag = true;
			num = 2;
			sourceDir = Path.Combine(fullPath, "Demo", "v2", path);
		}
		else
		{
			if (!ExistsDirectory(Path.Combine(fullPath, "Demo", "v1", path)))
			{
				return default(SlideFadeAnnounceDirection.AsyncPlayScope);
			}
			flag = true;
			num = 1;
			sourceDir = Path.Combine(fullPath, "Demo", "v1", path);
		}
		MigrationValidateHandle(sourceDir, fullPath2);
		if (flag || num == 1)
		{
			CopyDirectory(sourceDir, fullPath2);
		}
		if (num <= 1)
		{
			MigrateFrom_v1_To_v2();
		}
		MigrateFrom_v2_To();
		await UniTask.CompletedTask;
		return default(SlideFadeAnnounceDirection.AsyncPlayScope);
	}

	private void MigrateFrom_v1_To_v2()
	{
		WasMigrated = true;
		if (saveDataIO.TryLoad<NoteData>(out var result))
		{
			NoteList noteList = new NoteList
			{
				Titles = result.PageDic.Values.ToDictionary((PageData x) => x.UniqueID, (PageData x) => x.TitleText),
				PageOrderList = result.PageOrderList
			};
			List<PageDataV2> pageCache = result.PageDic.Values.Select((PageData x) => x.ToV2()).ToList();
			InMemoryMigrationService.SetData(new NoteDataV2(noteList, pageCache));
			saveDataIO.Delete<NoteData>();
		}
		else
		{
			saveDataIO.IsBackupBreakFile = true;
		}
		try
		{
			TodoForCompletedFixed todoForCompletedFixed = new TodoForCompletedFixed();
			List<CalenderMonthlyData> list = new List<CalenderMonthlyData>();
			string filePath = Path.Combine(BulbulConstant.SaveDirectoryPath, CalendarData.CalendarFileName);
			ES3File eS3File = new ES3File(filePath);
			string[] keys = eS3File.GetKeys();
			foreach (string text in keys)
			{
				if (!DateTime.TryParseExact(text.Replace("DiaryData", ""), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
				{
					continue;
				}
				CalendarDateData calendarDateData = eS3File.Load<CalendarDateData>(text);
				CalenderMonthlyData calenderMonthlyData = list.FirstOrDefault((CalenderMonthlyData x) => x.Year == dt.Year && x.Month == dt.Month);
				if (calenderMonthlyData == null)
				{
					calenderMonthlyData = new CalenderMonthlyData(dt.Year, dt.Month);
					list.Add(calenderMonthlyData);
				}
				calenderMonthlyData.DiaryList.TryAdd(dt.Day, calendarDateData);
				foreach (TodoData value in calendarDateData.CompleteTodoListDic.Values)
				{
					todoForCompletedFixed.TodoList.Add(value);
				}
			}
			if (list.Count != 0)
			{
				InMemoryMigrationService.SetData(list);
			}
			if (todoForCompletedFixed.TodoList.Count != 0)
			{
				InMemoryMigrationService.SetData(todoForCompletedFixed);
			}
			ES3.DeleteFile(filePath);
		}
		catch (FormatException)
		{
			saveDataIO.IsBackupBreakFile = true;
		}
		if (saveDataIO.TryLoad<MusicSetting>(out var result2))
		{
			InMemoryMigrationService.SetData(result2.ToV2());
			if (result2.LocalAudioDatas.Count > 0 && !saveDataIO.KeyExists<LocalMusicSetting>())
			{
				InMemoryMigrationService.SetData(new LocalMusicSetting(result2.LocalAudioDatas));
			}
			saveDataIO.Delete<MusicSetting>();
		}
		else
		{
			saveDataIO.IsBackupBreakFile = true;
		}
	}

	private void MigrateFrom_v2_To()
	{
		if (InMemoryMigrationService.TryGetData<MusicSettingV2>(out var data))
		{
			data.SaveReady();
			saveDataIO.Save(data);
		}
		if (InMemoryMigrationService.TryGetData<LocalMusicSetting>(out var data2))
		{
			saveDataIO.Save(data2);
		}
		if (InMemoryMigrationService.TryGetData<List<CalenderMonthlyData>>(out var data3))
		{
			foreach (CalenderMonthlyData item in data3)
			{
				SaveDataManager.Instance.SaveCalenderMonth(item);
			}
		}
		if (InMemoryMigrationService.TryGetData<TodoForCompletedFixed>(out var data4) && saveDataIO.TryLoad<TodoAllData>(out var result))
		{
			foreach (TodoData todo in data4.TodoList)
			{
				foreach (TodoListData value3 in result.TodoListDic.Values)
				{
					if (value3.TodoDic.TryGetValue(todo.UniqueID, out var value))
					{
						value.SetCompleteTodoDatetime(todo.Completed);
						break;
					}
				}
			}
			saveDataIO.Save(result);
		}
		if (saveDataIO.TryLoad<PlayerData>(out var result2) | saveDataIO.TryLoad<PointPurchaseData>(out var result3))
		{
			int point = result2?.Point ?? 0;
			if (!saveDataIO.TryLoad<PlayerDataV3>(out var result4))
			{
				result4 = result2?.ToV3() ?? new PlayerDataV3();
				WasMigrated = true;
			}
			if (!saveDataIO.TryLoad<PointPurchaseDataV3>(out var result5))
			{
				if (result3 == null)
				{
					result3 = new PointPurchaseData();
				}
				result5 = result3.ToV3(point);
				WasMigrated = true;
			}
			saveDataIO.Save(result4);
			saveDataIO.Save(result5);
			saveDataIO.RenameMigratedFile<PlayerData>();
			saveDataIO.RenameMigratedFile<PointPurchaseData>();
		}
		if (saveDataIO.TryLoad<TodoAllData>(out var result6))
		{
			foreach (KeyValuePair<ulong, TodoListData> item2 in result6.TodoListDic)
			{
				item2.Deconstruct(out var _, out var value2);
				TodoListData todoListData = value2;
				string prefix = todoListData.UniqueID.ToString();
				if (!saveDataIO.FileExists<TodoListData>(prefix))
				{
					SaveDataManager.Instance.SaveTodoList(todoListData);
				}
			}
			saveDataIO.RenameMigratedFile<TodoAllData>();
			WasMigrated = true;
		}
		if (saveDataIO.TryLoadFile<LocalMusicSetting>("", ".es3", out var result7))
		{
			string fullName = result7.FullName;
			string text = Path.ChangeExtension(fullName, ".dat");
			if (!File.Exists(text))
			{
				File.Move(fullName, text);
			}
			else
			{
				result7.Delete();
			}
			WasMigrated = true;
		}
	}

	private static void CopyDirectory(string sourceDir, string destDir, bool overwrite = true)
	{
		if (Directory.Exists(sourceDir))
		{
			Directory.CreateDirectory(destDir);
			string[] files = Directory.GetFiles(sourceDir);
			foreach (string obj in files)
			{
				string fileName = Path.GetFileName(obj);
				string destFileName = Path.Combine(destDir, fileName);
				File.Copy(obj, destFileName, overwrite);
			}
			files = Directory.GetDirectories(sourceDir);
			foreach (string obj2 in files)
			{
				string fileName2 = Path.GetFileName(obj2);
				string destDir2 = Path.Combine(destDir, fileName2);
				CopyDirectory(obj2, destDir2, overwrite);
			}
		}
	}

	private static bool ExistsReleaseDirectory(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		try
		{
			string searchPattern = SteamUser.GetSteamID().ToString();
			string[] directories = Directory.GetDirectories(path, "Release", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				string[] directories2 = Directory.GetDirectories(directories[i], searchPattern, SearchOption.AllDirectories);
				for (int j = 0; j < directories2.Length; j++)
				{
					if (Directory.EnumerateFiles(directories2[j], "*", SearchOption.AllDirectories).Any())
					{
						return true;
					}
				}
			}
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	private bool ExistsDirectory(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		try
		{
			if (Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any())
			{
				return true;
			}
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	private void MigrateValidation(string sourceDir, string destDir)
	{
		long num = new DirectoryInfo(sourceDir).EnumerateFiles("*", SearchOption.AllDirectories).Sum((FileInfo f) => f.Length);
		if (new DriveInfo(Path.GetPathRoot(destDir)).AvailableFreeSpace < num * 2)
		{
			throw new InsufficientStorageException();
		}
	}

	private void MigrationValidateHandle(string sourceDir, string destDir)
	{
		try
		{
			MigrateValidation(sourceDir, destDir);
		}
		catch (InsufficientStorageException)
		{
			UnityEngine.Object.FindFirstObjectByType<SlideFadeAnnounceDirection>(FindObjectsInactive.Include).CreatePlayScopeAsync(SlideFadeAnnounceDirection.AnnounceType.MigrationInsufficientStorage).Forget();
			throw;
		}
	}
}
