using System;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class UIManagerForMobile : MonoBehaviour, IOnClickButtonAllUIDeactivateProvider, IOnClickButtonOpenTutorialProvider
{
	[Inject]
	private DirectionService _directionService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private IRaycastBlocker _raycastBlocker;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[SerializeField]
	private RoomGameManager _roomGameManager;

	[SerializeField]
	private UIShowManagerForMobile _uiShowManager;

	[SerializeField]
	private FacilityStory _facilityStory;

	[SerializeField]
	private FacilitySettingMobile _facilitySetting;

	[SerializeField]
	private FacilityEnvironmentMobile _facilityEnvironment;

	[SerializeField]
	private FacilityDecorationForMobile _facilityDecoration;

	[SerializeField]
	private FacilityMusic _facilityMusic;

	[SerializeField]
	private FacilityPlayerLevel _facilityPlayerLevel;

	[SerializeField]
	private FacilityPomodoro _facilityPomodoro;

	[SerializeField]
	private FacilityCurrentDateTimeMobile _facilityCurrentDateTime;

	[SerializeField]
	private FacilityPlayerPoint _facilityPlayerPoint;

	[SerializeField]
	private FacilityPomodoroTimerDisplayAndSetting _facilityPomodoroTimerDisplayAndSetting;

	[SerializeField]
	private FacilityNoteContentsUIView _facilityNoteContentsUIView;

	[SerializeField]
	private FacilityCalendarContentsUI _facilityCalendarContentsUI;

	[SerializeField]
	private FacilityCurrentBatteryLevel _facilityCurrentBatteryLevel;

	[SerializeField]
	private FacilityTodoListContentsUI _facilityTodoListContentsUI;

	[SerializeField]
	private FacilityHabitTrackerContentUI _facilityHabitTrackerContentUI;

	[SerializeField]
	private FacilityWallPaperUIForMobile _facilityWallPaperUI;

	[SerializeField]
	private FacilityDemoEditionNoticeForMobile _facilityDemoEditionNotice;

	[SerializeField]
	private FacilityShopWindow _facilityShopWindow;

	[SerializeField]
	private FacilityReview _facilityReview;

	[SerializeField]
	private SpecialService _specialService;

	[SerializeField]
	private FacilitiesUIGroupWindowView _facilitiesUIGroupWindowView;

	[SerializeField]
	private FacilityFailedReactionIconEffect _facilityFailedReactionIconEffect;

	[SerializeField]
	private FacilityInterstitialAd _facilityInterstitialAd;

	[SerializeField]
	private GameObject _raycastBlockerObject;

	[SerializeField]
	private ExitConfirmationUI _exitConfirmationUI;

	[SerializeField]
	private ObjectsActiveChecker _facilityWindowsActiveChecker;

	[SerializeField]
	private WallPaperManagerForMobile _wallPaperManagerForMobile;

	private bool _isPlayingTutorialStory;

	public void Setup(Action tutorialSkip)
	{
		_facilityWallPaperUI.Setup();
		_facilitiesUIGroupWindowView.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilitiesUIGroupWindowView.OnClickCloseButton, delegate
		{
			DeactivateFacilitiesUIGroup();
		}).AddTo(this);
		_facilityNoteContentsUIView.Setup();
		_facilityEnvironment.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilityEnvironment.OnClickClose, delegate
		{
			DeactivateEnvironment();
		}).AddTo(this);
		_facilityPomodoro.Setup();
		_facilityPomodoroTimerDisplayAndSetting.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilityPomodoroTimerDisplayAndSetting.OnClickOpenPomodoroTimerSettingButton, delegate
		{
			OnClickButtonPomodoro();
		}).AddTo(this);
		_facilityPomodoroTimerDisplayAndSetting.OnClose.Subscribe(delegate((bool isRequestedPortraitWallpaper, bool isNeedCloseSE) data)
		{
			var (flag, _) = data;
			if (data.isNeedCloseSE)
			{
				_systemSeService.PlayCancel();
			}
			if (flag && _wallPaperManagerForMobile.EnableAutoTransition)
			{
				DeactivatePomodoroNextWallpaper();
			}
			else
			{
				DeactivatePomodoro();
			}
		}).AddTo(this);
		_facilityPlayerLevel.Setup();
		_facilityPlayerPoint.Setup();
		_facilityMusic.Setup();
		_facilityDecoration.Setup();
		ObservableSubscribeExtensions.Subscribe(_facilityDecoration.OnClickClose, delegate
		{
			DeactivateDecoration();
		}).AddTo(this);
		_facilityCurrentDateTime.Setup();
		_facilityStory.Setup(tutorialSkip);
		_facilitySetting.Setup();
		_facilityCalendarContentsUI.Setup();
		_facilityFailedReactionIconEffect.Setup();
		_facilityCurrentBatteryLevel.Setup();
		_facilityTodoListContentsUI.Setup();
		_facilityHabitTrackerContentUI.Setup();
		_facilityInterstitialAd.Setup();
		_facilityDemoEditionNotice.Setup();
		_facilityShopWindow.Setup();
		_facilityReview.Setup();
		ObservableSubscribeExtensions.Subscribe(_specialService.OnCloseFromCloseButton, delegate
		{
			DeactivateSpecial();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityStory.OnCloseFromCloseButton, delegate
		{
			DeactivateStory();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityMusic.OnCloseFromCloseButton, delegate
		{
			DeactivateMusic();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilitySetting.OnCloseFromCloseButton, delegate
		{
			DeactivateSetting();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityShopWindow.OnClickCloseButton, delegate
		{
			DeactivateFacilityShop();
		}).AddTo(this);
		_scenarioReader.OnStartReady.Subscribe(delegate(ScenarioType type)
		{
			if (type == ScenarioType.Tutorial)
			{
				_isPlayingTutorialStory = true;
				_uiShowManager.SetCallIconActive(active: true);
			}
			if (type == ScenarioType.SmallTalk)
			{
				bool flag = false;
				if (_facilityStory.IsActive.CurrentValue)
				{
					_facilityStory.Deactivate();
					flag = true;
				}
				if (_specialService.IsActive.CurrentValue)
				{
					_specialService.DeactivateList();
					flag = true;
				}
				if (_facilityPomodoroTimerDisplayAndSetting.IsActive)
				{
					_facilityPomodoroTimerDisplayAndSetting.Deactivate().Forget();
					flag = true;
				}
				if (_exitConfirmationUI.IsActive)
				{
					_exitConfirmationUI.Deactivate();
				}
				if (flag)
				{
					DeactivateAsync().Forget();
				}
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_scenarioReader.OnEndStory, delegate
		{
			_isPlayingTutorialStory = false;
		}).AddTo(this);
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void UpdatePlatform()
	{
		_facilityPlayerLevel.UpdateFacility();
		_facilityPomodoro.UpdateFacility();
		_facilityMusic.UpdateFacility();
		_facilityStory.UpdateFacility();
		_facilityCurrentBatteryLevel.UpdateFacility();
		_facilityDecoration.UpdateFacility();
		if (_isPlayingTutorialStory)
		{
			_uiShowManager.SetCallIconActive(active: true);
		}
		else
		{
			_uiShowManager.SetCallIconActive(!_roomCameraManager.IsBlending && (!_facilityWindowsActiveChecker.CheckActive() || _wallPaperManagerForMobile.State == WallPaperManagerForMobile.WallPaperState.Horizontal));
		}
	}

	public void UpdateMusicOnly()
	{
		_facilityMusic.UpdateFacility();
	}

	public void OnClickButtonAllUIDeactivate()
	{
		if (_roomGameManager.IsCanUseFacility() && _uiShowManager.IsShowUI)
		{
			_uiShowManager.AllUIDeactivate().Forget();
		}
	}

	public void OnClickButtonPomodoro()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilityPomodoroTimerDisplayAndSetting.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivatePomodoro()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await _facilityPomodoroTimerDisplayAndSetting.Deactivate();
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivatePomodoroNextWallpaper()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await _facilityPomodoroTimerDisplayAndSetting.Deactivate();
			_wallPaperManagerForMobile.RequestVerticalWallpaper();
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilitySetting()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilitySetting.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateSetting()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await UniTask.WaitForSeconds(0.2f);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityStory()
	{
		if (_roomGameManager.IsCanUseFacility() && (!_scenarioReader.IsPlayingScenario() || _scenarioReader.PlayingScenarioType != ScenarioType.SmallTalk))
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilityStory.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateStory()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await UniTask.WaitForSeconds(0.2f);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilitySpecial()
	{
		if (_roomGameManager.IsCanUseFacility() && (!_scenarioReader.IsPlayingScenario() || _scenarioReader.PlayingScenarioType != ScenarioType.SmallTalk))
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_specialService.ActivateList();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateSpecial()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await UniTask.WaitForSeconds(0.2f);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityMusic()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilityMusic.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateMusic()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			SetUIRaycastBlock(block: true);
			await UniTask.WaitForSeconds(0.2f);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityEnvironment()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			await _facilityEnvironment.Activate();
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateEnvironment()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			_systemSeService.PlayCancel();
			SetUIRaycastBlock(block: true);
			await _facilityEnvironment.Deactivate();
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityDecoration()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			await _facilityDecoration.Activate();
			SetUIRaycastBlock(block: false);
		}
	}

	private void DeactivateDecoration()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			_systemSeService.PlayCancel();
			SetUIRaycastBlock(block: true);
			await _facilityDecoration.Deactivate();
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityTodo()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilitiesUIGroupWindowView.SetTab(FacilitiesUIGroupContentsView.ContentType.Todo);
			_facilitiesUIGroupWindowView.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityNote()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilitiesUIGroupWindowView.SetTab(FacilitiesUIGroupContentsView.ContentType.Note);
			_facilitiesUIGroupWindowView.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityCalendar()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilityCalendarContentsUI.OnClickCalendarButton();
			_facilitiesUIGroupWindowView.SetTab(FacilitiesUIGroupContentsView.ContentType.Calendar);
			_facilitiesUIGroupWindowView.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	public void DeactivateFacilitiesUIGroup()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			_systemSeService.PlayCancel();
			_facilitiesUIGroupWindowView.Deactivate();
			SetUIRaycastBlock(block: true);
			await UniTask.WaitForSeconds(0.2f);
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityHabitTracker()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			await _uiShowManager.DeactivateMainUI();
			_facilitiesUIGroupWindowView.SetTab(FacilitiesUIGroupContentsView.ContentType.HabitTracker);
			_facilitiesUIGroupWindowView.Activate();
			await UniTask.WaitForSeconds(0.2f);
			SetUIRaycastBlock(block: false);
		}
	}

	public void DeactivateFacilityShop()
	{
		DeactivateAsync().Forget();
		async UniTaskVoid DeactivateAsync()
		{
			_systemSeService.PlayCancel();
			SetUIRaycastBlock(block: true);
			await _facilityShopWindow.DeactivateAsync();
			await _uiShowManager.ActivateMainUI();
			SetUIRaycastBlock(block: false);
		}
	}

	public void OnClickButtonFacilityShop()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			ActivateAsync().Forget();
		}
		async UniTaskVoid ActivateAsync()
		{
			_systemSeService.PlayClick();
			SetUIRaycastBlock(block: true);
			if (!(await _facilityShopWindow.EntryAsync()))
			{
				SetUIRaycastBlock(block: false);
			}
			else
			{
				await _uiShowManager.DeactivateMainUI();
				await _facilityShopWindow.ActivateAsync();
				SetUIRaycastBlock(block: false);
			}
		}
	}

	public void OnClickExitButton()
	{
		if (_directionService.GamePlayingDefect.IsConnectionLost())
		{
			_roomGameManager.SetMainState(RoomGameManager.MainState.Release);
			return;
		}
		_raycastBlocker.Block();
		_uiShowManager.AllUIDeactivate().Forget();
		_scenarioReader.EndStory();
		_heroineService.EndGame();
		_roomGameManager.SetMainState(RoomGameManager.MainState.ExitGame0);
	}

	public void OnClickButtonOpenTutorial()
	{
		_systemSeService.PlayClick();
		TutorialService.TutorialPageType pageType = TutorialService.TutorialPageType.ScreenUI;
		TutorialService.TutorialPageOpenType pageOpenType = TutorialService.TutorialPageOpenType.ALL;
		_tutorialService.OpenTutorial(pageType, pageOpenType);
	}

	private void SetUIRaycastBlock(bool block)
	{
		_raycastBlockerObject.SetActive(block);
	}
}
