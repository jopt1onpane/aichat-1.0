using System;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class UIManagerForPC : MonoBehaviour, IOnClickButtonAllUIDeactivateProvider, IOnClickButtonOpenTutorialProvider
{
	[Inject]
	private DirectionService _directionService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private IUIShowManager _uiShowManager;

	[Inject]
	private IRaycastBlocker _raycastBlocker;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private ISpecialService _specialService;

	[Inject]
	private MyCozyWeatherService _cozyWeatherService;

	[SerializeField]
	private RoomGameManager _roomGameManager;

	[SerializeField]
	private FacilityStory _facilityStory;

	[SerializeField]
	private FacilityTodo _facilityTodo;

	[SerializeField]
	private FacilityNote _facilityNote;

	[SerializeField]
	private FacilityCalendar _facilityCalendar;

	[SerializeField]
	private FacilityEnvironment _facilityEnvironment;

	[SerializeField]
	private FacilitySetting _facilitySetting;

	[SerializeField]
	private FacilityDecoration _facilityDecoration;

	[SerializeField]
	private FacilityMusic _facilityMusic;

	[SerializeField]
	private FacilityPomodoro _facilityPomodoro;

	[SerializeField]
	private FacilityPlayerLevel _facilityPlayerLevel;

	[SerializeField]
	private FacilityPlayerPoint _facilityPlayerPoint;

	[SerializeField]
	private FacilityCurrentDateAndTime _facilityCurrentDateAndTime;

	[SerializeField]
	private FacilityHabitTracker _facilityHabitTracker;

	[SerializeField]
	private AutoTimeWindowViewChanger _autoTimeWindowViewChanger;

	public void Setup(Action tutorialSkip)
	{
		EnviromentSetup().Forget();
		_facilityPomodoro.Setup();
		_facilityPlayerLevel.Setup();
		_facilityPlayerPoint.Setup();
		_facilityTodo.Setup();
		_facilityNote.Setup();
		_facilityCalendar.Setup();
		_facilityMusic.Setup();
		_facilityDecoration.Setup();
		_facilityCurrentDateAndTime.Setup(_facilityStory.IsActive, _facilityDecoration.IsActive, _facilityEnvironment.IsActive, _facilityMusic.IsActive, _specialService.IsActive);
		_facilityStory.Setup(tutorialSkip);
		_facilitySetting.Setup();
		_facilityHabitTracker.Setup();
		async UniTask EnviromentSetup()
		{
			await _cozyWeatherService.Setup();
			_facilityEnvironment.Setup();
			_autoTimeWindowViewChanger.Setup();
		}
	}

	public void UpdatePlatform()
	{
		_facilityPlayerLevel.UpdateFacility();
		_facilityPomodoro.UpdateFacility();
		_facilityTodo.UpdateFacility();
		_facilityNote.UpdateFacility();
		_facilityCalendar.UpdateFacility();
		_facilityMusic.UpdateFacility();
		_facilityEnvironment.UpdateFacility();
		_facilityStory.UpdateFacility();
		_facilityHabitTracker.UpdateFacility();
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

	public void OnClickButtonFacilitySetting()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilitySetting.IsActive())
			{
				_systemSeService.PlayCancel();
				_facilitySetting.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilitySetting.Activate();
			_facilityStory.Deactivate();
			_facilityDecoration.Deactivate();
			_facilityEnvironment.Deactivate();
			_facilityMusic.Deactivate();
			_facilityCalendar.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilityStory()
	{
		if (_roomGameManager.IsCanUseFacility() && (!_scenarioReader.IsPlayingScenario() || _scenarioReader.PlayingScenarioType != ScenarioType.SmallTalk))
		{
			if (_facilityStory.IsActive.CurrentValue)
			{
				_systemSeService.PlayCancel();
				_facilityStory.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilityStory.Activate();
			_facilityMusic.Deactivate();
			_facilityDecoration.Deactivate();
			_facilityEnvironment.Deactivate();
			_facilityCalendar.Deactivate();
			_facilitySetting.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilitySpecial()
	{
		if (_roomGameManager.IsCanUseFacility() && (!_scenarioReader.IsPlayingScenario() || _scenarioReader.PlayingScenarioType != ScenarioType.SmallTalk))
		{
			if (_specialService.IsActive.CurrentValue)
			{
				_systemSeService.PlayCancel();
				_specialService.DeactivateList();
				return;
			}
			_systemSeService.PlayClick();
			_specialService.ActivateList();
			_facilityStory.Deactivate();
			_facilityMusic.Deactivate();
			_facilityDecoration.Deactivate();
			_facilityEnvironment.Deactivate();
			_facilityCalendar.Deactivate();
			_facilitySetting.Deactivate();
		}
	}

	public void OnClickButtonFacilityMusic()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityMusic.IsActive.CurrentValue)
			{
				_systemSeService.PlayCancel();
				_facilityMusic.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilityMusic.Activate();
			_facilityStory.Deactivate();
			_facilityDecoration.Deactivate();
			_facilityEnvironment.Deactivate();
			_facilityCalendar.Deactivate();
			_facilitySetting.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilityEnviroment()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityEnvironment.IsActive.CurrentValue)
			{
				_systemSeService.PlayCancel();
				_facilityEnvironment.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilityEnvironment.Activate();
			_facilityDecoration.Deactivate();
			_facilityMusic.Deactivate();
			_facilityStory.Deactivate();
			_facilityCalendar.Deactivate();
			_facilitySetting.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilityDecoration()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityDecoration.IsActive.CurrentValue)
			{
				_systemSeService.PlayCancel();
				_facilityDecoration.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilityDecoration.Activate();
			_facilityEnvironment.Deactivate();
			_facilityMusic.Deactivate();
			_facilityStory.Deactivate();
			_facilityCalendar.Deactivate();
			_facilitySetting.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilityTodo()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityTodo.IsActive())
			{
				_systemSeService.PlayCancel();
				_facilityTodo.Deactivate();
			}
			else
			{
				_systemSeService.PlayClick();
				_facilityTodo.Activate();
			}
		}
	}

	public void OnClickButtonFacilityNote()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityNote.IsActive())
			{
				_systemSeService.PlayCancel();
				_facilityNote.Deactivate();
			}
			else
			{
				_systemSeService.PlayClick();
				_facilityNote.Activate();
			}
		}
	}

	public void OnClickButtonFacilityCalendar()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityCalendar.IsActive())
			{
				_systemSeService.PlayCancel();
				_facilityCalendar.Deactivate();
				return;
			}
			_systemSeService.PlayClick();
			_facilityCalendar.Activate();
			_facilityStory.Deactivate();
			_facilityDecoration.Deactivate();
			_facilityEnvironment.Deactivate();
			_facilityMusic.Deactivate();
			_facilitySetting.Deactivate();
			_specialService.DeactivateList();
		}
	}

	public void OnClickButtonFacilityHabitTracker()
	{
		if (_roomGameManager.IsCanUseFacility())
		{
			if (_facilityHabitTracker.IsActive())
			{
				_systemSeService.PlayCancel();
				_facilityHabitTracker.Deactivate();
			}
			else
			{
				_systemSeService.PlayClick();
				_facilityHabitTracker.Activate();
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
}
