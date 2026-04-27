using Bulbul.MasterData;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class ExitUI : MonoBehaviour
{
	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private FacilityLockEventService _facilityLockEventService;

	[SerializeField]
	private Button _exitIconButton;

	[SerializeField]
	private ExitConfirmationUI _exitConfirmationUI;

	[SerializeField]
	private LockUI _lockUI;

	private Subject<Unit> _onExitGame = new Subject<Unit>();

	private Tween _lockTween;

	public Observable<Unit> OnExitGame => _onExitGame;

	public void Setup()
	{
		_exitConfirmationUI.Setup();
		_exitConfirmationUI.AddDontCloseOnClick(_exitIconButton.transform as RectTransform);
		_lockUI.Setup();
		_exitConfirmationUI.OnDecide.Subscribe(delegate(bool isOk)
		{
			_exitConfirmationUI.Deactivate();
			if (isOk)
			{
				_onExitGame.OnNext(Unit.Default);
				_systemSeService.PlayClick();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_exitConfirmationUI.OnClickOutside, delegate
		{
			_exitConfirmationUI.Deactivate();
			_systemSeService.PlayCancel();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnLock, delegate
		{
			_exitConfirmationUI.Deactivate();
			_lockUI.Activate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_facilityLockEventService.OnUnlock, delegate
		{
			_lockUI.Deactivate();
		}).AddTo(this);
	}

	public void OnClickButtonExitGameIcon()
	{
		if (!_scenarioReader.IsPlayingScenario() || _scenarioReader.PlayingScenarioType != ScenarioType.SmallTalk)
		{
			if (_exitConfirmationUI.IsActive)
			{
				_systemSeService.PlayCancel();
				_exitConfirmationUI.Deactivate();
			}
			else
			{
				_systemSeService.PlayClick();
				_exitConfirmationUI.Activate();
			}
		}
	}
}
