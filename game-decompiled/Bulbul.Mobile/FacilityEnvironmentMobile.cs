using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityEnvironmentMobile : MonoBehaviour, IEnvironmentUIService, IApplyEnvironmentWindowController
{
	[SerializeField]
	private EnvironmentWindow _window;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private MyCozyWeatherService _cozyWeatherService;

	[Inject]
	private PlayerPointService _playerPointService;

	[Inject]
	private EnvironmentApplicationService _applicationService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private MobileDemoEditionLockedTargetCheckService _mobileDemoEditionLockedTargetCheckService;

	private EnvironmentTimeSelectorPresenter _timeSelectorPresenter;

	private EnvironmentListPresenter _listPresenter;

	private EnvironmentVolumeChangeDialog _volumeChangeDialog;

	private readonly ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>(value: false);

	private DateTime _lastChangeWindowDateTime;

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public DateTime LastChangeWindowDateTime => _lastChangeWindowDateTime;

	public Observable<Unit> OnClickClose => _window.CloseButton.OnClickAsObservable();

	public void Setup()
	{
		SetupAsync().Forget();
	}

	private async UniTask SetupAsync()
	{
	}

	private int GetCurrentPresetIdx()
	{
		return _environmentDataService.GetCurrentPresetIndex();
	}

	private bool HasDifferenceFromPreset(int idx)
	{
		return _environmentDataService.HasDifferenceFromPreset(idx);
	}

	public async UniTask Activate()
	{
		_isActive.Value = true;
		_window.gameObject.SetActive(value: true);
		_timeSelectorPresenter.ExpandShrinkImmediate();
		_window.gameObject.SetActive(value: false);
		await _window.ActivateAnimation.Activate();
	}

	public async UniTask Deactivate()
	{
		_isActive.Value = false;
		await _window.ActivateAnimation.Deactivate();
	}

	public void ApplyWindowBySavedata()
	{
	}
}
