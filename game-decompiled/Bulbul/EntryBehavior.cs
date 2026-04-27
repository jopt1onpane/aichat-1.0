using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul.MasterData;
using Bulbul.Mobile;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NestopiSystem;
using NestopiSystem.Steam;
using R3;
using Steamworks;
using UnityEngine;
using UnityEngine.CrashReportHandler;
using VContainer.Unity;

namespace Bulbul;

public class EntryBehavior : IAsyncStartable
{
	private readonly MasterDataLoader masterDataLoader;

	private readonly SteamManager steamManager;

	private readonly MusicService musicService;

	private readonly UnlockMusic unlockMusic;

	private readonly LoadDirectionService loadDirectionService;

	private readonly LoginFlow loginFlow;

	private readonly SaveDataDirtyManager saveDataDirtyManager;

	private readonly WebApiGate webApiGate;

	private readonly WebApiErrorBehavior apiErrorBehavior;

	private readonly IUICanvasProvider uiCanvasProvider;

	private readonly AudioMixerGroupContainer audioMixerGroupContainer;

	public EntryBehavior(MasterDataLoader masterDataLoader, SteamManager steamManager, MusicService musicManager, UnlockMusic unlockMusic, LoadDirectionService loadDirectionService, LoginFlow loginFlow, SaveDataDirtyManager saveDataDirtyManager, WebApiGate webApiGate, WebApiErrorBehavior apiErrorBehavior, IUICanvasProvider uiCanvasProvider, AudioMixerGroupContainer audioMixerGroupContainer)
	{
		this.masterDataLoader = masterDataLoader;
		this.steamManager = steamManager;
		musicService = musicManager;
		this.unlockMusic = unlockMusic;
		this.loadDirectionService = loadDirectionService;
		this.loginFlow = loginFlow;
		this.saveDataDirtyManager = saveDataDirtyManager;
		this.webApiGate = webApiGate;
		this.apiErrorBehavior = apiErrorBehavior;
		this.uiCanvasProvider = uiCanvasProvider;
		this.audioMixerGroupContainer = audioMixerGroupContainer;
	}

	async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
	{
		Input.multiTouchEnabled = false;
		InMemoryData.ClearAll();
		if (DevicePlatform.Steam.IsMobile())
		{
			Screen.orientation = ScreenOrientation.Portrait;
			await UniTask.NextFrame();
			ScreenOrientationManagerForMobile.Instance.LockRotatePortrait();
		}
		Debug.LogDeveloperCheck("[Entry]⇒ SteamManager Start Initialize");
		steamManager.Initialize();
		await (task1: masterDataLoader.LoadCompletionAsync(cancellation), task2: UniTask.WaitUntil(() => steamManager.IsInitialized, PlayerLoopTiming.Update, cancellation).ContinueWith(delegate
		{
			Debug.LogDeveloperCheck(string.Format("{0} Steamの初期化完了。AppID:{1}, ユーザ名:{2}", "[Entry]⇒", SteamUtils.GetAppID(), SteamFriends.GetPersonaName()));
			CrashReportHandler.SetUserMetadata("SteamName", SteamFriends.GetPersonaName().ToString());
		}));
		await RequestNotificationPermission(cancellation);
		SaveDataManager.Instance.LoadSetting();
		audioMixerGroupContainer.ManualUpdateAllVolume();
		webApiGate.AccountGate.Value = true;
		webApiGate.ResetGate.Value = true;
		if (!webApiGate.LoginGate.CurrentValue)
		{
			await apiErrorBehavior.LogoutDialog(cancellation);
			return;
		}
		WebApi.SetWebApiGate(webApiGate);
		if (await LoginAsync(cancellation))
		{
			await CheckSafeSaveFlowAsync(cancellation);
			loadDirectionService.StartDirection(DevicePlatform.Steam.IsPC());
			await UniTask.NextFrame(cancellation);
			float startTime = Time.time;
			Debug.LogDeveloperCheck("[Entry]⇒ Start SceneLoad");
			SceneLoadHandle sceneHandle = SceneLoader.LoadSceneAsync("RoomScene", autoActivate: false);
			await LoadAsync(cancellation);
			Debug.LogDeveloperCheck("[Entry]⇒ Wait SceneLoad");
			await UniTask.WaitUntil(() => sceneHandle.IsReady);
			Debug.LogDeveloperCheck("[Entry]⇒ Finished SceneLoad");
			float num = Time.time - startTime;
			float num2 = Mathf.Max(0f, 2f - num);
			if (num2 > 0f)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(num2));
			}
			LoadDirectionService.DirectionType loadDirectionType = LoadDirectionService.DirectionType.Normal;
			if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 32f)
			{
				loadDirectionType = LoadDirectionService.DirectionType.Error;
			}
			loadDirectionService.FinishDirection(loadDirectionType, DevicePlatform.Steam.IsPC());
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			await UniTask.WaitUntil(() => loadDirectionService.IsFinishDirection(loadDirectionType));
			sceneHandle.Activate();
		}
	}

	private async UniTask LoadAsync(CancellationToken ct)
	{
		Debug.LogDeveloperCheck("[Entry]⇒ Start SaveData Load");
		await SaveDataManager.Instance.Load();
		Debug.LogDeveloperCheck("[Entry]⇒ Finished SaveData Load");
		await UniTask.WaitUntil(() => masterDataLoader.IsLoaded);
		List<GameAudioInfo> builtin = masterDataLoader.MusicDataList.Select((MusicData x) => GameAudioInfo.CreateNormal(x.AudioClip, x.Tag, x.Title, x.Credit, x.UUID, x.IsManualUnlock, x.overwriteTitleForMobile, x.overwriteCreditForMobile)).ToList();
		foreach (GameAudioInfo item in builtin)
		{
			if (item.IsManualUnlocked)
			{
				item.IsUnlocked = unlockMusic.IsUnlocked(item.UUID);
			}
			else
			{
				item.IsUnlocked = true;
			}
		}
		GameAudioInfo[] source = await SaveDataManager.Instance.LocalMusicSetting.LocalAudioDatas.ToUniTaskAsyncEnumerable().SelectAwaitWithCancellation((LocalAudioData data, CancellationToken ct2) => GameAudioInfo.CreateLocalFileAsync(data.FilePath, data.UUID, ct2)).ToArrayAsync();
		string[] masterUUIDs = masterDataLoader.MusicDataList.Select((MusicData xx) => xx.UUID).ToArray();
		GameAudioInfo[] array = (from x in builtin.Concat(source.Where((GameAudioInfo x) => x.AudioClip != null))
			orderby SaveDataManager.Instance.MusicSetting.PlaylistOrder.IndexOf(x.UUID), Enumerable.Contains(masterUUIDs, x.UUID)
			select x).ToArray();
		GameAudioInfo[] array2 = array;
		foreach (GameAudioInfo gameAudioInfo in array2)
		{
			if (SaveDataManager.Instance.MusicSetting.FavoriteAudioUUIDs.Contains(gameAudioInfo.UUID))
			{
				gameAudioInfo.Tag.AddFavorite();
			}
		}
		musicService.Load(array);
		Debug.LogDeveloperCheck("[Entry]⇒ RecoveryPlayerData Start");
		RecoveryPlayerData();
		Debug.LogDeveloperCheck("[Entry]⇒ RecoveryPlayerData Finished");
		saveDataDirtyManager.StartDirtyObserve();
		void RecoveryPlayerData()
		{
			string latestMainScenarioId = null;
			int num2 = -1;
			foreach (string playedScenarioGroupID in SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs)
			{
				if (playedScenarioGroupID.StartsWith("main_") && int.TryParse(playedScenarioGroupID.Substring(5), out var result) && result > num2)
				{
					num2 = result;
					latestMainScenarioId = playedScenarioGroupID;
				}
			}
			if (latestMainScenarioId != null)
			{
				ScenarioGroupData scenarioGroupData = masterDataLoader.ScenarioGroupMasterList.FirstOrDefault((ScenarioGroupData x) => x.ID == latestMainScenarioId);
				if (scenarioGroupData != null)
				{
					int unlockLevel = scenarioGroupData.UnlockLevel;
					LevelData levelData = SaveDataManager.Instance.PlayerData.LevelData;
					if (levelData.CurrentLevel < unlockLevel)
					{
						levelData.SetLevel(unlockLevel);
						if (unlockLevel > 1)
						{
							SaveDataManager.Instance.PlayerData.IsNeedTutorial = false;
						}
					}
				}
			}
		}
	}

	private async UniTask<bool> LoginAsync(CancellationToken ct)
	{
		try
		{
			webApiGate.ResetGate.Value = true;
			await loginFlow.LoginAsync(ct);
			return true;
		}
		catch (MaintenanceException ex)
		{
			MaintenanceException e = ex;
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_maintenance_title";
				o.BodyID = "ui_maintenance_body";
				o.BodySelector = (string _) => e.MainText;
				o.Parent = uiCanvasProvider.CommonDialogParent;
				o.EnableCloseOnClickButton = true;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm_ok")
				};
			});
			object obj = null;
			try
			{
				await dialog.SubmitOrCloseWaitAsync(ct);
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return await LoginAsync(ct);
		}
		catch (LogoutException)
		{
			await apiErrorBehavior.LogoutDialog(ct);
			return false;
		}
		catch (AppResetException e2)
		{
			return await AppResetAsync(e2, ct);
		}
		catch (ErrorCodeException ex3)
		{
			ErrorCodeException e3 = ex3;
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_error";
				o.BodyID = "ui_common_error_errorCode";
				o.BodySelector = (string s) => string.Format(s, (int)e3.ErrorCode);
				o.Parent = uiCanvasProvider.CommonDialogParent;
				o.EnableCloseOnClickButton = true;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm_ok")
				};
			});
			object obj = null;
			try
			{
				await dialog.SubmitOrCloseWaitAsync(ct);
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return await LoginAsync(ct);
		}
		catch (UnityWebRequestException ex4)
		{
			int errorCode = ((ex4.ResponseCode != 0L) ? ((int)ex4.ResponseCode) : ((int)ex4.Result));
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_error";
				o.BodyID = "ui_common_error_errorCode2";
				o.BodySelector = (string s) => string.Format(s, errorCode);
				o.Parent = uiCanvasProvider.CommonDialogParent;
				o.EnableCloseOnClickButton = true;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm_ok")
				};
			});
			object obj = null;
			try
			{
				await dialog.SubmitOrCloseWaitAsync(ct);
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return await LoginAsync(ct);
		}
		catch (EntryBreakException ex5)
		{
			_ = ex5;
			await UniTask.Never(ct);
			return false;
		}
		catch (Exception exception)
		{
			ct.ThrowIfCancellationRequested();
			Debug.LogError(exception);
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_error";
				o.BodyID = "ui_common_error_body";
				o.Parent = uiCanvasProvider.CommonDialogParent;
				o.EnableCloseOnClickButton = true;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm_ok")
				};
			});
			object obj = null;
			try
			{
				await dialog.SubmitOrCloseWaitAsync(ct);
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			return await LoginAsync(ct);
		}
	}

	private async UniTask<bool> AppResetAsync(AppResetException e, CancellationToken ct)
	{
		if (e.ResetReason == ResetReason.None)
		{
			return true;
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.DeviceIDInvalid | ResetReason.UserIDInvalid))
		{
			CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_error";
				o.BodyID = "ui_common_error_body";
				o.Parent = uiCanvasProvider.CommonDialogParent;
			});
			await UniTask.Never(ct);
			return false;
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.Ban))
		{
			CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_error";
				o.BodyID = "ui_common_error_ban";
				o.Parent = uiCanvasProvider.CommonDialogParent;
			});
			await UniTask.Never(ct);
			return false;
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.Maintenance))
		{
			CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_maintenance_title";
				o.BodyID = "ui_maintenance_body";
				o.Parent = uiCanvasProvider.CommonDialogParent;
			});
			await UniTask.Never(ct);
			return false;
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.AppForceUpdate))
		{
			CommonDialog commonDialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_app_update_title";
				o.BodyID = "ui_app_update_body";
				o.Parent = uiCanvasProvider.CommonDialogParent;
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_app_open_store")
				};
			});
			using (ObservableSubscribeExtensions.Subscribe(commonDialog.OnSubmit, delegate
			{
				Application.OpenURL("https://store.steampowered.com/app/3548580");
			}))
			{
				await UniTask.Never(ct);
				return false;
			}
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.TermsUpdate))
		{
			return await LoginAsync(ct);
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.MasterDataUpdate))
		{
			return await LoginAsync(ct);
		}
		if (e.ResetReason.HasAnyFlag(ResetReason.OtherDeviceLogin))
		{
			CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_login_failed_title";
				o.BodyID = "ui_login_failed_body";
				o.Parent = uiCanvasProvider.CommonDialogParent;
			});
			return false;
		}
		return true;
	}

	private static async UniTask RequestNotificationPermission(CancellationToken ct)
	{
		await UniTask.CompletedTask;
	}

	private async UniTask CheckSafeSaveFlowAsync(CancellationToken ct)
	{
		if (string.IsNullOrEmpty(SaveDataManager.Instance.AccountData?.DeviceID))
		{
			return;
		}
		if (SaveDataManager.Instance.HasSafeSaveKey() && !SaveDataManager.Instance.HasSaveWarningNeverShowKey())
		{
			bool neverShowSelected = false;
			SaveWarningDialog dialog = SaveWarningDialog.Create(uiCanvasProvider.CommonDialogParent);
			object obj = null;
			try
			{
				neverShowSelected = await dialog.OnCloseClick.ToUniTask(useFirstValue: true, ct);
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (neverShowSelected)
			{
				SaveDataManager.Instance.SetSaveWarningNeverShowMark();
			}
		}
		SaveDataManager.Instance.SetSafeSaveMark();
	}
}
