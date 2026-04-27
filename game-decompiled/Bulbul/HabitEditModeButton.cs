using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class HabitEditModeButton : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private Button _mainButton;

	[SerializeField]
	private InteractableUI _interactableUI;

	[SerializeField]
	private GameObject _editModeObjs;

	[SerializeField]
	private Button _saveButton;

	[SerializeField]
	private Button _discardButton;

	[SerializeField]
	private RectTransform _editModeSizeTrans;

	[SerializeField]
	private float _editModeSizeX = 222.8f;

	private readonly Subject<Unit> _onStartEditMode = new Subject<Unit>();

	private readonly Subject<bool> _onEndEditMode = new Subject<bool>();

	private float _initialSizeX;

	private bool _isEditMode;

	private Tween _tween;

	public Observable<Unit> OnStartEditMode => _onStartEditMode;

	public Observable<bool> OnEndEditMode => _onEndEditMode;

	public void Initialize()
	{
		_initialSizeX = _editModeSizeTrans.sizeDelta.x;
		_editModeObjs.SetActive(value: false);
		ObservableSubscribeExtensions.Subscribe(_mainButton.OnClickAsObservable(), delegate
		{
			if (!_isEditMode)
			{
				_isEditMode = true;
				_mainButton.interactable = false;
				_interactableUI.ActivateUseUI();
				_onStartEditMode.OnNext(Unit.Default);
				_tween.Kill();
				_tween = _editModeSizeTrans.DOSizeDelta(new Vector2(_editModeSizeX, _editModeSizeTrans.sizeDelta.y), 0.2f).OnComplete(delegate
				{
					_editModeObjs.SetActive(value: true);
				});
				_systemSeService.PlayPulldownOpen();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_saveButton.OnClickAsObservable(), delegate
		{
			if (_isEditMode)
			{
				EndEditMode(isSave: true);
				_systemSeService.PlayClick();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_discardButton.OnClickAsObservable(), delegate
		{
			if (_isEditMode)
			{
				EndEditMode(isSave: false);
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
	}

	private void EndEditMode(bool isSave)
	{
		_isEditMode = false;
		_mainButton.interactable = true;
		_editModeObjs.SetActive(value: false);
		_onEndEditMode.OnNext(isSave);
		_tween.Kill();
		_tween = _editModeSizeTrans.DOSizeDelta(new Vector2(_initialSizeX, _editModeSizeTrans.sizeDelta.y), 0.2f).OnComplete(delegate
		{
			_interactableUI.DeactivateUseUI();
		});
	}
}
