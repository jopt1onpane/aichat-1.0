using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class DecorationWindowView : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private FacilityBurgerActivateAnimationMobile _activateAnimation;

	[SerializeField]
	private DecorationListViewForMobile _listView;

	[SerializeField]
	private PlayerPointView _playerPointView;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private PresetSaveDialogView _presetSaveDialogView;

	[SerializeField]
	private Button _presetButton;

	[SerializeField]
	private InteractableUI _presetInteractableUI;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public DecorationListViewForMobile ListView => _listView;

	public PresetSaveDialogView PresetSaveDialogView => _presetSaveDialogView;

	public void Setup()
	{
		_presetSaveDialogView.Setup();
		ObservableSubscribeExtensions.Subscribe(_presetButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlaySelect();
			_presetSaveDialogView.Activate();
		}).AddTo(this);
		_presetSaveDialogView.IsActive.Subscribe(delegate(bool isActive)
		{
			_presetInteractableUI.SetUseUI(isActive);
		}).AddTo(this);
		_activateAnimation.Setup();
		_listView.Setup();
	}

	public void SetPlayerPoint(int value, bool withAninmation)
	{
		_playerPointView.SetPoint(value, withAninmation);
	}

	public async UniTask Activate()
	{
		await _activateAnimation.Activate();
	}

	public async UniTask Deactivate()
	{
		await _activateAnimation.Deactivate();
	}
}
