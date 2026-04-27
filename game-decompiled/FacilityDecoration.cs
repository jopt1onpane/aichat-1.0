using Bulbul;
using R3;
using UnityEngine;
using VContainer;

public class FacilityDecoration : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private DecorationDataService _decorationDataService;

	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private IDecorationListUI decorationListUI;

	[Inject]
	private ILayoutPresetChangeUI _presetChangeUI;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	private ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public void Setup()
	{
		_decorationDataService.Migrate();
		_decorationService.ApplyDecorationBySavedata();
		decorationListUI.Setup();
		ObservableSubscribeExtensions.Subscribe(decorationListUI.OnClickCloseButton, delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
		}).AddTo(this);
		_presetChangeUI.Setup(_decorationDataService);
		ObservableSubscribeExtensions.Subscribe(_presetChangeUI.OnChangeCurrentData, delegate
		{
			_decorationService.ApplyDecorationBySavedata();
		}).AddTo(this);
	}

	public void Activate()
	{
		decorationListUI.Activate();
		_isActive.Value = true;
		_roomCameraManager?.SetActiveDecorationCam(isActive: true);
	}

	public void Deactivate()
	{
		decorationListUI.Deactivate();
		_isActive.Value = false;
	}
}
