using System;
using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class HabitItemDateUI : MonoBehaviour
{
	public record ViewModel(bool IsEditMode, ReadOnlyReactiveProperty<HabitDateEnableState> EnableDisplay, ReactiveProperty<bool> IsChecked, Action<bool> OnClickChangeEnable);

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _checkMark;

	[SerializeField]
	private Image _checkBgImage;

	[SerializeField]
	private Image _baseCircleImage;

	[SerializeField]
	private Sprite _deadCircleSprite;

	[SerializeField]
	private float _disableAlpha = 0.5f;

	[SerializeField]
	private float _checkAnimDuration = 0.37f;

	[SerializeField]
	private Button _changeEnableButton;

	[SerializeField]
	private Image _changeEnableButtonImage;

	[SerializeField]
	private Image _changeEnableButtonImageActive;

	[SerializeField]
	private Sprite _toEnableSprite;

	[SerializeField]
	private Sprite _toEnableSpriteActive;

	private Sprite _aliveCircleSprite;

	private Sprite _toDisableSprite;

	private Sprite _toDisableSpriteActive;

	private ViewModel _viewModel;

	private DisposableBag _disposableBag;

	private Tween _checkTween;

	public void Initialize()
	{
		_aliveCircleSprite = _baseCircleImage.sprite;
		_toDisableSprite = _changeEnableButtonImage.sprite;
		_toDisableSpriteActive = _changeEnableButtonImageActive.sprite;
		_changeEnableButton.gameObject.SetActive(value: false);
	}

	public void Bind(ViewModel viewModel)
	{
		_disposableBag.Clear();
		_viewModel = viewModel;
		ObservableSubscribeExtensions.Subscribe(viewModel.EnableDisplay.Skip(1), delegate
		{
			UpdateDisplay();
		}).AddTo(ref _disposableBag);
		ObservableSubscribeExtensions.Subscribe(viewModel.IsChecked.Skip(1), delegate
		{
			UpdateDisplay(playCheckAnim: true);
		}).AddTo(ref _disposableBag);
		ObservableSubscribeExtensions.Subscribe(_button.OnClickAsObservable(), delegate
		{
			_viewModel.IsChecked.Value = !_viewModel.IsChecked.Value;
		}).AddTo(ref _disposableBag);
		ObservableSubscribeExtensions.Subscribe(_changeEnableButton.OnClickAsObservable(), delegate
		{
			if (_viewModel.OnClickChangeEnable != null)
			{
				bool obj = viewModel.EnableDisplay.CurrentValue switch
				{
					HabitDateEnableState.Enabled => false, 
					HabitDateEnableState.Disabled => true, 
					HabitDateEnableState.DeadPeriod => true, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				_viewModel.OnClickChangeEnable(obj);
			}
		}).AddTo(ref _disposableBag);
		UpdateDisplay();
	}

	public void Unbind()
	{
		_disposableBag.Clear();
		_viewModel = null;
	}

	private void UpdateDisplay(bool playCheckAnim = false)
	{
		_checkTween?.Kill();
		_checkTween = null;
		bool isChecked = _viewModel.IsChecked.Value;
		HabitDateEnableState enableState = _viewModel.EnableDisplay.CurrentValue;
		bool isEditMode = _viewModel.IsEditMode;
		var (sprite, color) = GetCircleImage(isEditMode);
		_baseCircleImage.sprite = sprite;
		_baseCircleImage.color = color;
		_changeEnableButton.gameObject.SetActive(isEditMode);
		if (isEditMode)
		{
			_button.interactable = false;
			_checkMark.gameObject.SetActive(value: false);
			_checkBgImage.gameObject.SetActive(value: false);
			_changeEnableButtonImage.sprite = ((enableState == HabitDateEnableState.Enabled) ? _toDisableSprite : _toEnableSprite);
			_changeEnableButtonImageActive.sprite = ((enableState == HabitDateEnableState.Enabled) ? _toDisableSpriteActive : _toEnableSpriteActive);
			return;
		}
		_button.interactable = enableState == HabitDateEnableState.Enabled || isChecked;
		_checkMark.gameObject.SetActive(value: true);
		_checkBgImage.gameObject.SetActive(value: true);
		if (playCheckAnim)
		{
			_checkTween = DOTween.Sequence().Append(_checkMark.transform.DOScale(isChecked ? 1 : 0, _checkAnimDuration)).Join(_checkBgImage.DOFade(isChecked ? 1 : 0, _checkAnimDuration));
			return;
		}
		_checkMark.transform.localScale = (isChecked ? Vector3.one : Vector3.zero);
		_checkBgImage.color = Color.white.WithA(isChecked ? 1 : 0);
		(Sprite sprite, Color color) GetCircleImage(bool editMode)
		{
			Color white = Color.white;
			Color color2 = Color.white.WithA(_disableAlpha);
			if (editMode)
			{
				Color item = ((enableState == HabitDateEnableState.Enabled) ? white : color2);
				return (sprite: _aliveCircleSprite, color: item);
			}
			if (isChecked)
			{
				return (sprite: _aliveCircleSprite, color: white);
			}
			return enableState switch
			{
				HabitDateEnableState.Disabled => (sprite: _aliveCircleSprite, color: color2), 
				HabitDateEnableState.DeadPeriod => (sprite: _deadCircleSprite, color: color2), 
				_ => (sprite: _aliveCircleSprite, color: white), 
			};
		}
	}

	private void OnDestroy()
	{
		Unbind();
		_checkTween?.Kill();
		_checkTween = null;
	}
}
