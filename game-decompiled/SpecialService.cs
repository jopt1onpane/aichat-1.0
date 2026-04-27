using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SpecialService : MonoBehaviour, ISpecialService
{
	public enum CollaborationType
	{
		None,
		AlterEgo,
		BearsRestaurant,
		Valentine2026,
		LunaNewYear2026,
		NearSpring2026
	}

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private DirectionService _directionService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private PlayerLevelService _playerLevelService;

	[Inject]
	private IPlayerLevelUIService playerLevelUIService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[SerializeField]
	[Header("新規アイコン")]
	private Image _newIconMark;

	[SerializeField]
	[Header("新規アイコン画像(通常)")]
	private Sprite _newIconMarkSpite;

	[SerializeField]
	[Header("新規アイコン画像(期間限定)")]
	private Sprite _newIconMarkLimitedSprite;

	[SerializeField]
	[Header("ロック用")]
	private LockUI _lockUI;

	[SerializeField]
	[Header("UI")]
	private SpecialView _view;

	[SerializeField]
	[Header("選択UIプレハブ:オルタエゴ")]
	private GameObject _specialSelectCellAlterEgoPrefab;

	[SerializeField]
	[Header("選択UIプレハブ:くまのレストラン")]
	private GameObject _specialSelectCellBearsRestaurantPrefab;

	[SerializeField]
	[Header("選択UIプレハブ:バレンタイン2026")]
	private GameObject _specialSelectCellValentine2026Prefab;

	[SerializeField]
	[Header("選択UIプレハブ:旧正月2026")]
	private GameObject _specialSelectCellLunaNewYear2026Prefab;

	[SerializeField]
	[Header("選択UIプレハブ:NearSpring2026")]
	private GameObject _specialSelectCellNearSpring2026Prefab;

	[SerializeField]
	[Header("選択UIの親： 進行中")]
	private Transform _inProgressParent;

	[SerializeField]
	[Header("選択UIの親： 読了")]
	private Transform _readParent;

	private readonly ReactiveProperty<bool> _specialTypeChangeGate = new ReactiveProperty<bool>(value: true);

	private ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(value: false);

	private readonly Dictionary<CollaborationType, SpecialSelectCell> specialSelectCells = new Dictionary<CollaborationType, SpecialSelectCell>();

	private DisposableBag _specialSelectListDisposable;

	private SpecialAlterEgoUtil _specialAlterEgoUtil = new SpecialAlterEgoUtil();

	private SpecialBearsRestaurantUtil _specialBearsRestaurantUtil = new SpecialBearsRestaurantUtil();

	private Subject<Unit> _onCloseFromCloseButton = new Subject<Unit>();

	private SpecialValentine2026Util _specialValentine2026Util = new SpecialValentine2026Util();

	private SpecialLunaNewYear2026Util _specialLunaNewYear2026Util = SpecialLunaNewYear2026Util.Instance;

	private SpecialNearSpring2026Util _specialNearSpring2026Util = new SpecialNearSpring2026Util();

	private Subject<ScenarioType> _onEndStory = new Subject<ScenarioType>();

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public Subject<Unit> OnCloseFromCloseButton => _onCloseFromCloseButton;

	public void Setup()
	{
		CreateList();
		SetEnableNewIconMark();
		_view.Setup();
		_isActive.Subscribe(delegate(bool isActive)
		{
			if (isActive)
			{
				_view.ActivateSpecialList();
			}
			else
			{
				_view.DeactivateSpecialList();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_view.OnClickCloseSpecialListButton, delegate
		{
			_isActive.Value = false;
			_systemSeService.PlayCancel();
			_onCloseFromCloseButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_view.OnClickOpenSpecialListButton, delegate
		{
			NewIconOnce();
		}).AddTo(this);
		if (SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData.NextLevelNecessaryExp == 0f)
		{
			SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData.SetNextLevelNecessaryExp(_masterDataLoader.AlterEgoData.NextLevelNecessaryExp);
		}
		if (SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData.NextLevelNecessaryExp == 0f)
		{
			SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData.SetNextLevelNecessaryExp(_masterDataLoader.BearsRestaurantData.NextLevelNecessaryExp);
		}
		if (SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData.NextLevelNecessaryExp == 0f)
		{
			SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData.SetNextLevelNecessaryExp(_masterDataLoader.Valentine2026Data.NextLevelNecessaryExp);
		}
		if (SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData.NextLevelNecessaryExp == 0f)
		{
			SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData.SetNextLevelNecessaryExp(_masterDataLoader.LunaNewYear2026Data.NextLevelNecessaryExp);
		}
		if (SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData.NextLevelNecessaryExp == 0f)
		{
			SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData.SetNextLevelNecessaryExp(_masterDataLoader.NearSpring2026Data.NextLevelNecessaryExp);
		}
		_lockUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			Lock();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			if (!_directionService.GamePlayingDefect.IsUseDefectForEpisodeDirection())
			{
				Unlock();
			}
		}).AddTo(this);
		SetEnableNewIconMark();
		if (_directionService.GamePlayingDefect.IsUseDefectForEpisodeDirection())
		{
			SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			Lock();
			SaveDataManager.Instance.SaveCollaborationData();
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == CollaborationType.Valentine2026 && !_specialValentine2026Util.IsValidPeriod())
		{
			ChangeToNormalMode();
		}
		else if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == CollaborationType.LunaNewYear2026 && (!_specialLunaNewYear2026Util.IsValidPeriod() || !SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.CurrentValue))
		{
			ChangeToNormalMode();
		}
		else if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == CollaborationType.NearSpring2026 && !_specialNearSpring2026Util.IsValidPeriod())
		{
			ChangeToNormalMode();
		}
		static void ChangeToNormalMode()
		{
			SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			SaveDataManager.Instance.SaveCollaborationData();
		}
	}

	public void CreateList()
	{
		while (_inProgressParent.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(_inProgressParent.GetChild(0).gameObject);
		}
		while (_readParent.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(_readParent.GetChild(0).gameObject);
		}
		_specialSelectListDisposable.DisposeAndRecreate();
		specialSelectCells.Clear();
		CreateLunaNewYear2026Cell();
		CreateCell(CollaborationType.NearSpring2026, _specialSelectCellNearSpring2026Prefab, _specialNearSpring2026Util);
		CreateCell(CollaborationType.Valentine2026, _specialSelectCellValentine2026Prefab, _specialValentine2026Util);
		CreateCell(CollaborationType.AlterEgo, _specialSelectCellAlterEgoPrefab, _specialAlterEgoUtil);
		CreateCell(CollaborationType.BearsRestaurant, _specialSelectCellBearsRestaurantPrefab, _specialBearsRestaurantUtil);
		SpecialSelectCell CreateCell(CollaborationType specialType, GameObject prefab, SpecialScenarioUtil specialUtil)
		{
			if (specialUtil.IsUnlocked())
			{
				if (specialUtil.IsInProgress())
				{
					SpecialSelectCell cell = UnityEngine.Object.Instantiate(prefab, _inProgressParent.transform).GetComponent<SpecialSelectCell>();
					cell.Setup(specialType, _onEndStory);
					if (!specialSelectCells.TryAdd(specialType, cell))
					{
						specialSelectCells[specialType] = cell;
					}
					SaveDataManager.Instance.CollaborationSaveData.CurrentType.Subscribe(delegate(CollaborationType collaborationType)
					{
						if (collaborationType == cell.SpecialType)
						{
							cell.Activate();
						}
						else
						{
							cell.Deactivate();
						}
					}).AddTo(cell);
					cell.OnSubmit.SubscribeAwait(async delegate(Unit _, CancellationToken ct)
					{
						await ChangeSpecial(cell, ct);
					}, _specialTypeChangeGate).AddTo(ref _specialSelectListDisposable);
					ObservableSubscribeExtensions.Subscribe(_playerLevelService.OnAddedExp, delegate
					{
						cell.OnAddValue();
					}).AddTo(cell);
					return cell;
				}
				SpecialSelectCell component = UnityEngine.Object.Instantiate(prefab, _readParent.transform).GetComponent<SpecialSelectCell>();
				component.Setup(specialType, _onEndStory);
				component.GrayOut();
				if (!specialSelectCells.TryAdd(specialType, component))
				{
					specialSelectCells[specialType] = component;
				}
				return component;
			}
			return null;
		}
		void CreateLunaNewYear2026Cell()
		{
			SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Subscribe(delegate(bool isValid)
			{
				if (specialSelectCells.TryGetValue(CollaborationType.LunaNewYear2026, out var value) && (bool)value)
				{
					value.gameObject.SetActive(isValid);
				}
				else
				{
					value = CreateCell(CollaborationType.LunaNewYear2026, _specialSelectCellLunaNewYear2026Prefab, _specialLunaNewYear2026Util);
					if ((bool)value)
					{
						value.gameObject.SetActive(isValid);
					}
				}
			}).AddTo(ref _specialSelectListDisposable);
		}
	}

	public void ActivateList()
	{
		_isActive.Value = true;
	}

	public void DeactivateList()
	{
		_isActive.Value = false;
	}

	private bool IsInProgress(CollaborationType specialType)
	{
		return specialType switch
		{
			CollaborationType.AlterEgo => _specialAlterEgoUtil.IsReadAll(), 
			CollaborationType.BearsRestaurant => _specialBearsRestaurantUtil.IsReadAll(), 
			CollaborationType.Valentine2026 => _specialValentine2026Util.IsReadAll(), 
			CollaborationType.LunaNewYear2026 => _specialLunaNewYear2026Util.IsReadAll(), 
			CollaborationType.NearSpring2026 => _specialNearSpring2026Util.IsReadAll(), 
			_ => false, 
		};
	}

	private async UniTask ChangeSpecial(SpecialSelectCell cell, CancellationToken ct)
	{
		if (IsPossibleChangeSpecial())
		{
			bool flag = false;
			CollaborationType value;
			if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == cell.SpecialType)
			{
				value = CollaborationType.None;
				cell.Deactivate();
			}
			else
			{
				value = cell.SpecialType;
				cell.Activate();
				flag = true;
			}
			SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = value;
			SaveDataManager.Instance.SaveCollaborationData();
			if (flag)
			{
				await PlaySpecialModeStartAnnounce(cell.SpecialType, ct);
			}
		}
	}

	public bool IsPossibleChangeSpecial()
	{
		if (_lockUI.IsActive)
		{
			return false;
		}
		if (_playerLevelService.IsCurrentLevelUpDirection)
		{
			return false;
		}
		return true;
	}

	public bool IsPossibleReadNextSpecialEpisodeNumber()
	{
		switch (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value)
		{
		case CollaborationType.AlterEgo:
		{
			AlterEgoSaveData alterEgoData = SaveDataManager.Instance.CollaborationSaveData.AlterEgoData;
			if (alterEgoData.LastReadEpisodeNumber == 0)
			{
				return true;
			}
			if (alterEgoData.LevelData.CurrentLevel >= 2)
			{
				return true;
			}
			break;
		}
		case CollaborationType.BearsRestaurant:
		{
			BearsRestaurantSaveData bearsRestaurantData = SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData;
			if (bearsRestaurantData.LastReadEpisodeNumber == 0)
			{
				return true;
			}
			if (bearsRestaurantData.LevelData.CurrentLevel >= 2)
			{
				return true;
			}
			break;
		}
		case CollaborationType.Valentine2026:
		{
			Valentine2026SaveData valentine2026Data = SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data;
			if (valentine2026Data.LastReadEpisodeNumber == 0)
			{
				return true;
			}
			if (valentine2026Data.LevelData.CurrentLevel >= 2)
			{
				return true;
			}
			break;
		}
		case CollaborationType.LunaNewYear2026:
			if (SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData.CurrentLevel >= 2)
			{
				return true;
			}
			break;
		case CollaborationType.NearSpring2026:
			if (SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData.CurrentLevel >= 2)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public int GetNextEpisodeNumber(ScenarioType scenarioType)
	{
		return scenarioType switch
		{
			ScenarioType.Special_AlterEgo => SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.NextEpisodeNumber, 
			ScenarioType.Special_BearsRestaurant => SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.NextEpisodeNumber, 
			ScenarioType.Special_Valentine2026 => SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.NextEpisodeNumber, 
			ScenarioType.Special_LunaNewYear2026 => SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.NextEpisodeNumber, 
			ScenarioType.Special_NearSpring2026 => SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.NextEpisodeNumber, 
			_ => -1, 
		};
	}

	public LevelData GetCurrentLevelData()
	{
		return SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value switch
		{
			CollaborationType.AlterEgo => SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData, 
			CollaborationType.BearsRestaurant => SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData, 
			CollaborationType.Valentine2026 => SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData, 
			CollaborationType.LunaNewYear2026 => SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData, 
			CollaborationType.NearSpring2026 => SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData, 
			_ => SaveDataManager.Instance.LevelData, 
		};
	}

	public float AdjustUpperExp(float exp)
	{
		float num = 0f;
		LevelData currentLevelData = GetCurrentLevelData();
		switch (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value)
		{
		case CollaborationType.AlterEgo:
			num = ((currentLevelData.CurrentLevel < 2) ? (currentLevelData.NextLevelNecessaryExp - currentLevelData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
			break;
		case CollaborationType.BearsRestaurant:
			num = ((currentLevelData.CurrentLevel < 2) ? (currentLevelData.NextLevelNecessaryExp - currentLevelData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
			break;
		case CollaborationType.Valentine2026:
			num = ((currentLevelData.CurrentLevel < 2) ? (currentLevelData.NextLevelNecessaryExp - currentLevelData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
			break;
		case CollaborationType.LunaNewYear2026:
			num = ((currentLevelData.CurrentLevel < 2) ? (currentLevelData.NextLevelNecessaryExp - currentLevelData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
			break;
		case CollaborationType.NearSpring2026:
			num = ((currentLevelData.CurrentLevel < 2) ? (currentLevelData.NextLevelNecessaryExp - currentLevelData.CurrentExp) : 0f);
			if (exp >= num)
			{
				exp = num;
			}
			break;
		}
		return exp;
	}

	public ScenarioType GetNextLongTalkScenarioType()
	{
		return SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value switch
		{
			CollaborationType.None => ScenarioType.MainScenario, 
			CollaborationType.AlterEgo => ScenarioType.Special_AlterEgo, 
			CollaborationType.BearsRestaurant => ScenarioType.Special_BearsRestaurant, 
			CollaborationType.Valentine2026 => ScenarioType.Special_Valentine2026, 
			CollaborationType.LunaNewYear2026 => ScenarioType.Special_LunaNewYear2026, 
			CollaborationType.NearSpring2026 => ScenarioType.Special_NearSpring2026, 
			_ => ScenarioType.None, 
		};
	}

	public void OnEndStory(ScenarioType scenarioType, int episodeNumber)
	{
		bool flag = false;
		switch (scenarioType)
		{
		case ScenarioType.Special_AlterEgo:
			SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LastReadEpisodeNumber = episodeNumber;
			if (_specialAlterEgoUtil.IsReadAll())
			{
				flag = true;
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			}
			else if (episodeNumber == 1)
			{
				SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.NextEpisodeNumber = 2;
			}
			break;
		case ScenarioType.Special_BearsRestaurant:
			SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LastReadEpisodeNumber = episodeNumber;
			if (_specialBearsRestaurantUtil.IsReadAll())
			{
				flag = true;
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			}
			else if (episodeNumber == 1)
			{
				SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.NextEpisodeNumber = 2;
			}
			break;
		case ScenarioType.Special_Valentine2026:
			SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LastReadEpisodeNumber = episodeNumber;
			if (_specialValentine2026Util.IsReadAll())
			{
				flag = true;
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			}
			else if (episodeNumber == 1)
			{
				SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.NextEpisodeNumber = 2;
			}
			break;
		case ScenarioType.Special_LunaNewYear2026:
			SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber = episodeNumber;
			if (_specialLunaNewYear2026Util.IsReadAll())
			{
				flag = true;
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			}
			break;
		case ScenarioType.Special_NearSpring2026:
			SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LastReadEpisodeNumber = episodeNumber;
			if (_specialNearSpring2026Util.IsReadAll())
			{
				flag = true;
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
			}
			break;
		case ScenarioType.MainScenario:
			if (IsNeedUsePossibleAnnounce())
			{
				flag = true;
				SetEnableNewIconMark();
			}
			if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber == 30f)
			{
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = CollaborationType.None;
				Lock();
			}
			else if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber == 32f)
			{
				Unlock();
			}
			break;
		}
		SaveDataManager.Instance.SaveCollaborationData();
		if (flag)
		{
			CreateList();
		}
		else
		{
			_onEndStory.OnNext(scenarioType);
		}
	}

	private void Lock()
	{
		_lockUI.Activate();
		SetEnableNewIconMark(enable: false);
		DeactivateList();
	}

	private void Unlock()
	{
		_lockUI.Deactivate();
		SetEnableNewIconMark();
	}

	public bool IsNeedUsePossibleAnnounce()
	{
		bool result = false;
		if (_specialAlterEgoUtil.IsNeedUsePossibleAnnounce() || _specialBearsRestaurantUtil.IsNeedUsePossibleAnnounce() || _specialValentine2026Util.IsNeedUsePossibleAnnounce() || _specialLunaNewYear2026Util.IsNeedUsePossibleAnnounce() || _specialNearSpring2026Util.IsNeedUsePossibleAnnounce())
		{
			result = true;
		}
		return result;
	}

	public void PlayPossibleAnnounce()
	{
		if (!_directionService.SlideFadeAnnounce.IsPlaying)
		{
			if (_specialAlterEgoUtil.IsNeedUsePossibleAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockAlterEgoMode);
				SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsFinishedUsePossibleAnnounce = true;
			}
			else if (_specialBearsRestaurantUtil.IsNeedUsePossibleAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockBearsRestaurantMode);
				SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.IsFinishedUsePossibleAnnounce = true;
			}
			else if (_specialValentine2026Util.IsNeedUsePossibleAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockValentine2026Mode);
				SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.IsFinishedUsePossibleAnnounce = true;
			}
			else if (_specialLunaNewYear2026Util.IsNeedUsePossibleAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockLunaNewYear2026Mode);
				SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsFinishedUsePossibleAnnounce = true;
			}
			else if (_specialNearSpring2026Util.IsNeedUsePossibleAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.UnlockNearSpring2026Mode);
				SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsFinishedUsePossibleAnnounce = true;
			}
			SaveDataManager.Instance.SaveCollaborationData();
		}
	}

	public async UniTask PlaySpecialModeStartAnnounce(CollaborationType specialType, CancellationToken ct)
	{
		if (_directionService.SlideFadeAnnounce.IsPlaying)
		{
			return;
		}
		SlideFadeAnnounceDirection.AnnounceType type = SlideFadeAnnounceDirection.AnnounceType.None;
		switch (specialType)
		{
		case CollaborationType.AlterEgo:
			type = SlideFadeAnnounceDirection.AnnounceType.PomodoroAlterEgoMode;
			break;
		case CollaborationType.BearsRestaurant:
			type = SlideFadeAnnounceDirection.AnnounceType.PomodoroBearsRestaurantMode;
			break;
		case CollaborationType.Valentine2026:
			type = SlideFadeAnnounceDirection.AnnounceType.PomodoroValentine2026Mode;
			break;
		case CollaborationType.LunaNewYear2026:
			type = SlideFadeAnnounceDirection.AnnounceType.PomodoroLunaNewYear2026Mode;
			break;
		case CollaborationType.NearSpring2026:
			type = SlideFadeAnnounceDirection.AnnounceType.PomodoroNearSpring2026Mode;
			break;
		}
		playerLevelUIService.FocusUIActivate();
		_systemSeService.PlaySpecialOpening();
		SlideFadeAnnounceDirection.AsyncPlayScope scope = await _directionService.SlideFadeAnnounce.CreatePlayScopeAsync(type);
		object obj = null;
		int num = 0;
		try
		{
			using (Disposable.Create(delegate
			{
				if (playerLevelUIService != null)
				{
					playerLevelUIService.FocusUIDeactivate();
				}
			}))
			{
				await UniTask.WaitForSeconds(1.5f, ignoreTimeScale: false, PlayerLoopTiming.Update, ct);
			}
			num = 1;
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await ((IAsyncDisposable)scope/*cast due to .constrained prefix*/).DisposeAsync();
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
		if (num != 1)
		{
		}
	}

	public bool IsNeedUseFinishAnnounce()
	{
		bool result = false;
		if (_specialAlterEgoUtil.IsNeedUseFinishAnnounce() || _specialBearsRestaurantUtil.IsNeedUseFinishAnnounce() || _specialValentine2026Util.IsNeedUseFinishAnnounce() || _specialLunaNewYear2026Util.IsNeedUseFinishAnnounce() || _specialNearSpring2026Util.IsNeedUseFinishAnnounce())
		{
			result = true;
		}
		return result;
	}

	public void PlayFinishAnnounce()
	{
		if (!_directionService.SlideFadeAnnounce.IsPlaying)
		{
			if (_specialAlterEgoUtil.IsNeedUseFinishAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.FinishScenarioAlterEgo);
				SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsFinishedCompleteAnnounce = true;
			}
			else if (_specialBearsRestaurantUtil.IsNeedUseFinishAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.FinishScenarioBearsRestaurant);
				SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.IsFinishedCompleteAnnounce = true;
			}
			else if (_specialValentine2026Util.IsNeedUseFinishAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.FinishScenarioValentine2026);
				SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.IsFinishedCompleteAnnounce = true;
			}
			else if (_specialLunaNewYear2026Util.IsNeedUseFinishAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.FinishScenarioLunaNewYear2026);
				SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsFinishedCompleteAnnounce = true;
			}
			else if (_specialNearSpring2026Util.IsNeedUseFinishAnnounce())
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.FinishScenarioNearSpring2026);
				SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsFinishedCompleteAnnounce = true;
			}
			SaveDataManager.Instance.SaveCollaborationData();
		}
	}

	private void NewIconOnce()
	{
		if ((bool)_newIconMark)
		{
			SetEnableNewIconMark(enable: false);
			if (_specialAlterEgoUtil.IsNeedNewIcon())
			{
				SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.IsNeedUseNewIcon = false;
			}
			if (_specialBearsRestaurantUtil.IsNeedNewIcon())
			{
				SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.IsNeedUseNewIcon = false;
			}
			if (_specialValentine2026Util.IsNeedNewIcon())
			{
				SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.IsNeedUseNewIcon = false;
			}
			if (_specialLunaNewYear2026Util.IsNeedNewIcon())
			{
				SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsNeedUseNewIcon = false;
			}
			if (_specialNearSpring2026Util.IsNeedNewIcon())
			{
				SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.IsNeedUseNewIcon = false;
			}
			SaveDataManager.Instance.SaveCollaborationData();
		}
	}

	public void SetEnableNewIconMark()
	{
		bool flag = IsNeedNewIcon();
		_newIconMark.gameObject.SetActive(flag);
		if (flag)
		{
			_newIconMark.sprite = (IsLimitedNeedNewIcon() ? _newIconMarkLimitedSprite : _newIconMarkSpite);
		}
	}

	public void SetEnableNewIconMark(bool enable)
	{
		_newIconMark.gameObject.SetActive(enable);
		if (enable)
		{
			_newIconMark.sprite = (IsLimitedNeedNewIcon() ? _newIconMarkLimitedSprite : _newIconMarkSpite);
		}
	}

	private bool IsNeedNewIcon()
	{
		if (_specialAlterEgoUtil.IsNeedNewIcon() || _specialBearsRestaurantUtil.IsNeedNewIcon() || _specialValentine2026Util.IsNeedNewIcon() || _specialLunaNewYear2026Util.IsNeedNewIcon() || _specialNearSpring2026Util.IsNeedNewIcon())
		{
			return true;
		}
		return false;
	}

	private bool IsLimitedNeedNewIcon()
	{
		if (!_specialValentine2026Util.IsNeedNewIcon() && !_specialLunaNewYear2026Util.IsNeedNewIcon())
		{
			return _specialNearSpring2026Util.IsNeedNewIcon();
		}
		return true;
	}
}
