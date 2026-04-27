using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using NestopiSystem.DIContainers;
using UnityEngine;

namespace Bulbul;

public class SaveDataManager
{
	private static SaveDataManager _instance;

	private readonly Dictionary<(int year, int month), HabitAllMonthlyData> _habitMonthlyDataDic = new Dictionary<(int, int), HabitAllMonthlyData>();

	private readonly Dictionary<Type, ThrottledExecutor> _saveThrottledExecutors = new Dictionary<Type, ThrottledExecutor>();

	private const float ThrottleInterval = 1f;

	private SaveDataIO _saveDataIO;

	private SaveDataMigrator _savedataMigrator;

	public static SaveDataManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SaveDataManager();
			}
			return _instance;
		}
	}

	public static bool HasInstance => _instance != null;

	public AccountData AccountData { get; private set; } = new AccountData();

	public FilesSnapshot FilesSnapshot { get; private set; } = new FilesSnapshot(new List<FileMeta>(), 0L);

	public PlayerDataV3 PlayerData { get; private set; } = new PlayerDataV3();

	public LevelData LevelData => PlayerData.LevelData;

	public PomodoroData PomodoroData { get; private set; } = new PomodoroData();

	public TodoAllData TodoAllData { get; private set; } = new TodoAllData();

	public HabitAllHeaderData AllHabitHeaderData { get; private set; } = new HabitAllHeaderData();

	public HabitAllDeadPeriodData AllHabitDeadPeriodData { get; private set; } = new HabitAllDeadPeriodData();

	public NoteDataV2 NoteData { get; private set; } = new NoteDataV2();

	public SettingData SettingData { get; private set; } = new SettingData();

	public StoryData StoryData { get; private set; } = new StoryData();

	public HeroineData HeroineData { get; private set; } = new HeroineData();

	public CalendarData CalendarData { get; private set; } = new CalendarData();

	public MusicSettingV2 MusicSetting { get; private set; } = new MusicSettingV2();

	public LocalMusicSetting LocalMusicSetting { get; private set; } = new LocalMusicSetting();

	public EnviromentData EnviromentData { get; private set; } = new EnviromentData();

	public EnviromentPresetsData EnviromentPresetsData { get; private set; } = new EnviromentPresetsData();

	public DecorationsData DecorationSaveData { get; private set; } = new DecorationsData();

	public DecorationPresetsData DecorationPresetsData { get; private set; } = new DecorationPresetsData();

	public ScenarioProgressData ScenarioProgressData { get; private set; } = new ScenarioProgressData();

	public EnvironmentProgressData EnvironmentProgressData { get; private set; } = new EnvironmentProgressData();

	public DecorationProgressData DecorationProgressData { get; private set; } = new DecorationProgressData();

	public PointPurchaseDataV3 PointPurchaseData { get; private set; } = new PointPurchaseDataV3();

	public CollaborationSaveData CollaborationSaveData { get; private set; } = new CollaborationSaveData();

	public LimitedTimeEventSaveData LimitedTimeEventSaveData { get; private set; } = new LimitedTimeEventSaveData();

	public AutoTimeWindowChangeData AutoTimeWindowChangeData { get; private set; } = new AutoTimeWindowChangeData();

	public AchievementSaveData AchievementSaveData { get; private set; } = new AchievementSaveData();

	private SaveDataIO saveDataIO => _saveDataIO ?? (_saveDataIO = ProjectLifetimeScope.Resolve<SaveDataIO>());

	private SaveDataMigrator savedataMigrator => _savedataMigrator ?? (_savedataMigrator = ProjectLifetimeScope.Resolve<SaveDataMigrator>());

	public async UniTask Load()
	{
		Debug.LogDeveloperCheck("[Entry]⇒ Start SaveData Migrate");
		SlideFadeAnnounceDirection.AsyncPlayScope migrateMessageScope = await savedataMigrator.Migrate();
		object obj = null;
		int num = 0;
		object obj4;
		try
		{
			Debug.LogDeveloperCheck("[Entry]⇒ Finished SaveData Migrate");
			LoadPlayerData();
			PomodoroData = saveDataIO.Load<PomodoroData>() ?? new PomodoroData();
			PomodoroData.LoadSetup();
			LoadTodoAllData();
			AllHabitHeaderData = saveDataIO.Load<HabitAllHeaderData>() ?? new HabitAllHeaderData();
			AllHabitHeaderData.LoadSetup();
			_saveThrottledExecutors.TryAdd(typeof(HabitAllHeaderData), new ThrottledExecutor(SaveHabitHeaders, 1f));
			AllHabitDeadPeriodData = saveDataIO.Load<HabitAllDeadPeriodData>() ?? new HabitAllDeadPeriodData();
			AllHabitDeadPeriodData.LoadSetup();
			_saveThrottledExecutors.TryAdd(typeof(HabitAllDeadPeriodData), new ThrottledExecutor(SaveHabitDeadPeriods, 1f));
			if (InMemoryMigrationService.TryGetData<NoteDataV2>(out var data))
			{
				NoteData = data;
				SaveNoteList();
				SavePageCache(NoteData?.PageCache);
			}
			else
			{
				NoteList noteList = saveDataIO.Load<NoteList>() ?? new NoteList();
				NoteData = new NoteDataV2(noteList);
			}
			LoadSetting();
			StoryData = saveDataIO.Load<StoryData>() ?? new StoryData();
			StoryData.LoadSetup();
			HeroineData = saveDataIO.Load<HeroineData>() ?? new HeroineData();
			HeroineData.LoadSetup();
			if (InMemoryMigrationService.TryGetData<MusicSettingV2>(out var data2))
			{
				MusicSetting = data2;
				SaveMusicSetting();
			}
			else
			{
				MusicSetting = saveDataIO.Load<MusicSettingV2>() ?? new MusicSettingV2();
				MusicSetting.LoadSetup();
			}
			if (InMemoryMigrationService.TryGetData<LocalMusicSetting>(out var data3))
			{
				LocalMusicSetting = data3;
				SaveLocalMusicSetting();
			}
			else
			{
				LocalMusicSetting = saveDataIO.Load<LocalMusicSetting>() ?? new LocalMusicSetting();
			}
			EnviromentData = saveDataIO.Load<EnviromentData>() ?? new EnviromentData();
			_saveThrottledExecutors[EnviromentData.GetType()] = new ThrottledExecutor(SaveEnviroment, 1f);
			EnviromentPresetsData = saveDataIO.Load<EnviromentPresetsData>() ?? new EnviromentPresetsData();
			_saveThrottledExecutors[EnviromentPresetsData.GetType()] = new ThrottledExecutor(SaveEnviromentPreset, 1f);
			DecorationSaveData = saveDataIO.Load<DecorationsData>();
			if (DecorationSaveData == null)
			{
				DecorationSaveData = new DecorationsData();
			}
			else
			{
				DecorationSaveData.LoadSetup();
			}
			_saveThrottledExecutors[DecorationSaveData.GetType()] = new ThrottledExecutor(SaveDecoration, 1f);
			DecorationPresetsData = saveDataIO.Load<DecorationPresetsData>();
			if (DecorationPresetsData == null)
			{
				DecorationPresetsData = new DecorationPresetsData();
			}
			else
			{
				DecorationPresetsData.LoadSetup();
			}
			_saveThrottledExecutors[DecorationPresetsData.GetType()] = new ThrottledExecutor(SaveDecorationPreset, 1f);
			PointPurchaseData = saveDataIO.Load<PointPurchaseDataV3>() ?? new PointPurchaseDataV3();
			CollaborationSaveData = saveDataIO.Load<CollaborationSaveData>();
			if (CollaborationSaveData == null)
			{
				CollaborationSaveData = new CollaborationSaveData();
			}
			else
			{
				CollaborationSaveData.LoadSetup();
			}
			LimitedTimeEventSaveData = saveDataIO.Load<LimitedTimeEventSaveData>();
			if (LimitedTimeEventSaveData == null)
			{
				LimitedTimeEventSaveData = new LimitedTimeEventSaveData();
			}
			else
			{
				LimitedTimeEventSaveData.LoadSetup();
			}
			if (InMemoryMigrationService.TryGetData<List<CalenderMonthlyData>>(out var data4))
			{
				foreach (CalenderMonthlyData item in data4)
				{
					SaveCalenderMonth(item);
				}
			}
			if (InMemoryMigrationService.TryGetData<TodoForCompletedFixed>(out var data5))
			{
				foreach (TodoData todo in data5.TodoList)
				{
					foreach (TodoListData value2 in TodoAllData.TodoListDic.Values)
					{
						if (value2.TodoDic.TryGetValue(todo.UniqueID, out var value))
						{
							value.SetCompleteTodoDatetime(todo.Completed);
							break;
						}
					}
				}
			}
			ScenarioProgressData = saveDataIO.Load<ScenarioProgressData>() ?? new ScenarioProgressData();
			AutoTimeWindowChangeData = saveDataIO.Load<AutoTimeWindowChangeData>() ?? new AutoTimeWindowChangeData();
			EnvironmentProgressData = saveDataIO.Load<EnvironmentProgressData>() ?? new EnvironmentProgressData();
			DecorationProgressData = saveDataIO.Load<DecorationProgressData>() ?? new DecorationProgressData();
			AchievementSaveData = saveDataIO.Load<AchievementSaveData>() ?? new AchievementSaveData();
			if (saveDataIO.IsBackupBreakFile)
			{
				SlideFadeAnnounceDirection announceDirection = UnityEngine.Object.FindFirstObjectByType<SlideFadeAnnounceDirection>(FindObjectsInactive.Include);
				if ((bool)announceDirection)
				{
					await migrateMessageScope.DisposeAsync();
					announceDirection.InitSetup();
					SlideFadeAnnounceDirection.AsyncPlayScope scope = await announceDirection.CreatePlayScopeAsync(SlideFadeAnnounceDirection.AnnounceType.SaveDataBrokenBackup);
					object obj2 = null;
					try
					{
						await UniTask.WaitForSeconds(1);
						await UniTask.WaitUntil(() => Input.anyKeyDown);
					}
					catch (object obj3)
					{
						obj2 = obj3;
					}
					await ((IAsyncDisposable)scope/*cast due to .constrained prefix*/).DisposeAsync();
					obj4 = obj2;
					if (obj4 != null)
					{
						ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
					}
				}
			}
			Application.quitting += OnApplicationQuit;
			num = 1;
		}
		catch (object obj3)
		{
			obj = obj3;
		}
		await ((IAsyncDisposable)migrateMessageScope/*cast due to .constrained prefix*/).DisposeAsync();
		obj4 = obj;
		if (obj4 != null)
		{
			ExceptionDispatchInfo.Capture((obj4 as Exception) ?? throw obj4).Throw();
		}
		if (num != 1)
		{
		}
	}

	public void SaveAll()
	{
		SavePlayerData();
		SavePomodoroData();
		SaveTodo();
		SaveHabitHeaders();
		SaveNoteList();
		SavePageCache(NoteData?.PageCache);
		SaveSetting();
		SaveStory();
		SaveHeroineData();
		SaveMusicSetting();
		SaveLocalMusicSetting();
		SaveEnviroment();
		SaveEnviromentPreset();
		SaveDecoration();
		SaveDecorationPreset();
		SaveScenarioProgressData();
		SaveEnvironmentProgressData();
		SaveDecorationProgressData();
		SavePointPurchaseData();
		SaveCollaborationData();
		SaveLimitedTimeEventData();
		SaveAutoTimeWindowChangeData();
		SaveAchievementSaveData();
	}

	public void DeleteAllSaveData(bool excludeLocalData)
	{
		if (excludeLocalData)
		{
			saveDataIO.DeleteAllExcludeLocalData();
		}
		else
		{
			saveDataIO.DeleteAll();
			AccountData = new AccountData();
			LocalMusicSetting = new LocalMusicSetting();
			DeleteSafeSaveKey();
		}
		FilesSnapshot = new FilesSnapshot(new List<FileMeta>(), 0L);
		PlayerData = new PlayerDataV3();
		PomodoroData = new PomodoroData();
		TodoAllData = new TodoAllData();
		AllHabitHeaderData = new HabitAllHeaderData();
		AllHabitDeadPeriodData = new HabitAllDeadPeriodData();
		_habitMonthlyDataDic.Clear();
		NoteData = new NoteDataV2();
		SettingData = new SettingData();
		StoryData = new StoryData();
		HeroineData = new HeroineData();
		CalendarData = new CalendarData();
		MusicSetting = new MusicSettingV2();
		LocalMusicSetting = new LocalMusicSetting();
		EnviromentData = new EnviromentData();
		EnviromentPresetsData = new EnviromentPresetsData();
		DecorationSaveData = new DecorationsData();
		DecorationPresetsData = new DecorationPresetsData();
		ScenarioProgressData = new ScenarioProgressData();
		EnvironmentProgressData = new EnvironmentProgressData();
		DecorationProgressData = new DecorationProgressData();
		PointPurchaseData = new PointPurchaseDataV3();
		CollaborationSaveData = new CollaborationSaveData();
		LimitedTimeEventSaveData = new LimitedTimeEventSaveData();
		AchievementSaveData = new AchievementSaveData();
		_saveThrottledExecutors.Clear();
	}

	public void SaveAccountData()
	{
		saveDataIO.SaveJson(AccountData);
	}

	public void LoadAccountData()
	{
		AccountData = (saveDataIO.TryLoadJson<AccountData>(out var result) ? result : new AccountData());
	}

	public void SavePlayerData()
	{
		PlayerData.SetLastLoginDateTime();
		saveDataIO.Save(PlayerData);
	}

	public PlayerDataV3 LoadPlayerData()
	{
		PlayerDataV3 obj = saveDataIO.Load<PlayerDataV3>() ?? new PlayerDataV3();
		PlayerDataV3 result = obj;
		PlayerData = obj;
		return result;
	}

	public void SavePomodoroData()
	{
		PomodoroData.SaveReady();
		saveDataIO.Save(PomodoroData);
	}

	public void SaveTodo()
	{
		foreach (TodoListData value in TodoAllData.TodoListDic.Values)
		{
			SaveTodoList(value);
		}
	}

	private void LoadTodoAllData()
	{
		if (TodoAllData == null)
		{
			TodoAllData todoAllData = (TodoAllData = new TodoAllData());
		}
		if (!saveDataIO.TryLoadAll(out IEnumerable<TodoListData> result))
		{
			return;
		}
		foreach (TodoListData item in result)
		{
			TodoAllData.TodoListDic.TryAdd(item.UniqueID, item);
		}
	}

	public void SaveTodoList(TodoListData todoList)
	{
		saveDataIO.Save(todoList, todoList.UniqueID.ToString());
	}

	public void DeleteTodoList(ulong id)
	{
		saveDataIO.Delete<TodoListData>(id.ToString());
	}

	public void SaveHabitHeaders()
	{
		GetThrottledExecutor(AllHabitHeaderData.GetType()).Cancel();
		AllHabitHeaderData.SaveReady();
		saveDataIO.Save(AllHabitHeaderData);
	}

	public void SaveHabitHeadersThrottled()
	{
		GetThrottledExecutor(AllHabitHeaderData.GetType()).Reserve();
	}

	public void SaveHabitDeadPeriods()
	{
		GetThrottledExecutor(AllHabitDeadPeriodData.GetType()).Cancel();
		AllHabitDeadPeriodData.SaveReady();
		saveDataIO.Save(AllHabitDeadPeriodData);
	}

	public void SaveHabitDeadPeriodsThrottled()
	{
		GetThrottledExecutor(AllHabitDeadPeriodData.GetType()).Reserve();
	}

	public bool TryGetOrLoadHabitsMonthlyData(int year, int month, out HabitAllMonthlyData data)
	{
		if (_habitMonthlyDataDic.TryGetValue((year, month), out data))
		{
			return data != null;
		}
		string prefix = $"{year:D4}{month:D2}";
		data = saveDataIO.Load<HabitAllMonthlyData>(prefix);
		data?.LoadSetup();
		_habitMonthlyDataDic[(year, month)] = data;
		return data != null;
	}

	public void AddHabitsMonthlyData(int year, int month, HabitAllMonthlyData spanData)
	{
		if (_habitMonthlyDataDic.TryGetValue((year, month), out var value) && value != null)
		{
			Debug.LogWarning($"HabitSpanData already exists: {year:D4}{month:D2}. Overwriting");
		}
		_habitMonthlyDataDic[(year, month)] = spanData;
	}

	public void SaveHabitsMonthlyData(int year, int month)
	{
		if (_habitMonthlyDataDic.TryGetValue((year, month), out var value))
		{
			string prefix = $"{year:D4}{month:D2}";
			value.SaveReady();
			saveDataIO.Save(value, prefix);
		}
	}

	[Obsolete]
	public void SaveNote()
	{
		saveDataIO.Save(NoteData);
	}

	public void SaveNoteList()
	{
		saveDataIO.Save(NoteData.NoteList);
	}

	public void SavePageData(PageDataV2 pageData)
	{
		saveDataIO.Save(pageData, pageData.UniqueID.ToString());
	}

	public void SavePageCache(IReadOnlyList<PageDataV2> pageData)
	{
		if (pageData == null)
		{
			return;
		}
		foreach (PageDataV2 pageDatum in pageData)
		{
			saveDataIO.Save(pageDatum, pageDatum.UniqueID.ToString());
		}
	}

	public PageDataV2 LoadPageData(ulong pageID)
	{
		return saveDataIO.Load<PageDataV2>(pageID.ToString());
	}

	public void DeletePageData(ulong pageID)
	{
		saveDataIO.Delete<PageDataV2>(pageID.ToString());
	}

	public void SaveSetting()
	{
		SettingData.SaveReady();
		saveDataIO.Save(SettingData);
	}

	public void LoadSetting()
	{
		SettingData = saveDataIO.Load<SettingData>() ?? new SettingData();
		SettingData.LoadSetup();
	}

	public void SaveStory()
	{
		StoryData.SaveReady();
		saveDataIO.Save(StoryData);
	}

	public void SaveHeroineData()
	{
		saveDataIO.Save(HeroineData);
	}

	public void SaveMusicSetting()
	{
		MusicSetting.SaveReady();
		saveDataIO.Save(MusicSetting);
	}

	public void SaveLocalMusicSetting()
	{
		saveDataIO.Save(LocalMusicSetting);
	}

	public void SaveEnviroment()
	{
		GetThrottledExecutor(EnviromentData.GetType()).Cancel();
		saveDataIO.Save(EnviromentData);
	}

	public void SaveEnviromentThrottled()
	{
		GetThrottledExecutor(EnviromentData.GetType()).Reserve();
	}

	public void SaveEnviromentPreset()
	{
		GetThrottledExecutor(EnviromentPresetsData.GetType()).Cancel();
		saveDataIO.Save(EnviromentPresetsData);
	}

	public void SaveEnviromentPresetThrottled()
	{
		GetThrottledExecutor(EnviromentPresetsData.GetType()).Reserve();
	}

	public void SaveDecoration()
	{
		GetThrottledExecutor(DecorationSaveData.GetType()).Cancel();
		DecorationSaveData.SaveReady();
		saveDataIO.Save(DecorationSaveData);
	}

	public void SaveDecorationThrottled()
	{
		GetThrottledExecutor(DecorationSaveData.GetType()).Reserve();
	}

	public void SaveDecorationPreset()
	{
		GetThrottledExecutor(DecorationPresetsData.GetType()).Cancel();
		DecorationPresetsData.SaveReady();
		saveDataIO.Save(DecorationPresetsData);
	}

	public void SaveDecorationPresetThrottled()
	{
		GetThrottledExecutor(DecorationPresetsData.GetType()).Reserve();
	}

	public void SaveScenarioProgressData()
	{
		saveDataIO.Save(ScenarioProgressData);
	}

	public void SaveEnvironmentProgressData()
	{
		saveDataIO.Save(EnvironmentProgressData);
	}

	public void SaveDecorationProgressData()
	{
		saveDataIO.Save(DecorationProgressData);
	}

	public void SavePointPurchaseData()
	{
		saveDataIO.Save(PointPurchaseData);
	}

	public void SaveCollaborationData()
	{
		CollaborationSaveData.SaveReady();
		saveDataIO.Save(CollaborationSaveData);
	}

	public void SaveLimitedTimeEventData()
	{
		LimitedTimeEventSaveData.SaveReady();
		saveDataIO.Save(LimitedTimeEventSaveData);
	}

	public void SaveAchievementSaveData()
	{
		saveDataIO.Save(AchievementSaveData);
	}

	public void SaveFilesSnapshotData(FilesSnapshot snapshot)
	{
		FilesSnapshot = snapshot;
		saveDataIO.SaveJson(FilesSnapshot);
	}

	public bool TryLoadFilesSnapshotData(out FilesSnapshot result)
	{
		if (saveDataIO.TryLoadJson<FilesSnapshot>(out result))
		{
			FilesSnapshot = result;
			return true;
		}
		return false;
	}

	public void SetSafeSaveMark()
	{
		string text = AccountData?.DeviceID;
		if (!string.IsNullOrEmpty(text))
		{
			saveDataIO.SetPrefsKeyOnly<string>(ZString.Concat("SafeSaveKey", text));
		}
	}

	public bool HasSafeSaveKey()
	{
		string text = AccountData?.DeviceID;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		return saveDataIO.HasPrefsKey<string>(ZString.Concat("SafeSaveKey", text));
	}

	public void DeleteSafeSaveKey()
	{
		string text = AccountData?.DeviceID;
		if (!string.IsNullOrEmpty(text))
		{
			saveDataIO.DeletePrefsKey<string>(ZString.Concat("SafeSaveKey", text));
		}
	}

	public void SetSaveWarningNeverShowMark()
	{
		string text = AccountData?.DeviceID;
		if (!string.IsNullOrEmpty(text))
		{
			saveDataIO.SetPrefsKeyOnly<string>(ZString.Concat("SaveWarningNeverShowKey", text));
		}
	}

	public bool HasSaveWarningNeverShowKey()
	{
		string text = AccountData?.DeviceID;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		return saveDataIO.HasPrefsKey<string>(ZString.Concat("SaveWarningNeverShowKey", text));
	}

	public void SaveAutoTimeWindowChangeData()
	{
		saveDataIO.Save(AutoTimeWindowChangeData);
	}

	public CalenderMonthlyData LoadCalenderData(int year, int month)
	{
		string prefix = $"{year:D4}{month:D2}";
		return saveDataIO.Load<CalenderMonthlyData>(prefix);
	}

	public bool TryLoadCalenderData(int year, int month, out CalenderMonthlyData data)
	{
		string prefix = $"{year:D4}{month:D2}";
		return saveDataIO.TryLoad<CalenderMonthlyData>(prefix, out data);
	}

	public void SaveCalenderData(CalenderMonthlyData data)
	{
		string prefix = $"{data.Year:D4}{data.Month:D2}";
		saveDataIO.Save(data, prefix);
	}

	public void SaveCalenderData(CalenderMonthlyData data1, CalenderMonthlyData data2)
	{
		if (data1 != null && data2 != null && (data1.Year != data2.Year || data1.Month != data2.Month))
		{
			SaveCalenderData(data1);
			SaveCalenderData(data2);
		}
		else if (data1 != null)
		{
			SaveCalenderData(data1);
		}
		else if (data2 != null)
		{
			SaveCalenderData(data2);
		}
	}

	public void SaveCalenderMonth(CalenderMonthlyData data)
	{
		string prefix = $"{data.Year:D4}{data.Month:D2}";
		saveDataIO.Save(data, prefix);
	}

	private void OnApplicationQuit()
	{
		foreach (ThrottledExecutor value in _saveThrottledExecutors.Values)
		{
			value?.Flush();
			value?.Dispose();
		}
	}

	private ThrottledExecutor GetThrottledExecutor(Type type)
	{
		return _saveThrottledExecutors[type];
	}
}
