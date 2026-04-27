using System.Threading;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NestopiSystem;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayerVolumeWindow : MonoBehaviour
{
	private static float _currentSliderValue = 0f;

	private static Subject<bool> _onChangedMuteUI = new Subject<bool>();

	[SerializeField]
	private Slider _volumeSlider;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private RectTransform _rootRect;

	[SerializeField]
	private ToggleStyleButton _muteButton;

	[SerializeField]
	private Button _closeButton;

	private Subject<float> _onVolumeChanged = new Subject<float>();

	private Subject<Unit> _onClickSwitchMuteButton = new Subject<Unit>();

	[SerializeField]
	private Image _volumePercentImage;

	[SerializeField]
	private TMP_Text _volumePercentText;

	[SerializeField]
	private float _waitTime = 1f;

	[SerializeField]
	private float _volumePercentFadeInTime = 0.2f;

	[SerializeField]
	private float _volumePercentFadeOutTime = 0.25f;

	[SerializeField]
	private float _volumePercentAlphaInFade = 0.8f;

	private Subject<Unit> _onInputAnyNotice = new Subject<Unit>();

	private Tween _fadeVolumePercentTween;

	private CancellationTokenSource _ctsFadeOutCancel = new CancellationTokenSource();

	private float? _savedValue;

	private bool _isHandlingSlider;

	public static Observable<bool> OnChangedMuteUI => _onChangedMuteUI;

	public bool IsUIMute => _muteButton.IsOn;

	public Observable<float> OnVolumeChanged => _onVolumeChanged;

	public Observable<Unit> OnClickSwitchMuteButton => _onClickSwitchMuteButton;

	public Observable<Unit> OnClickCloseButton => _closeButton.OnClickAsObservable();

	public Observable<Unit> OnInputAnyNotice => _onInputAnyNotice;

	public static void SetupCurrentVolume(float volume)
	{
		_currentSliderValue = volume;
	}

	private void OnDestroy()
	{
		_onVolumeChanged.Dispose();
		_onClickSwitchMuteButton.Dispose();
		_onInputAnyNotice.Dispose();
	}

	public void Setup()
	{
		_volumeSlider.OnValueChangedAsObservable().Skip(1).Subscribe(delegate(float value)
		{
			_onVolumeChanged.OnNext(value);
			_currentSliderValue = value;
			UpdateVolumeSliderPercent(_currentSliderValue);
		})
			.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(ObservableTriggerExtensions.OnPointerDownAsObservable(_volumeSlider), delegate
		{
			if (_ctsFadeOutCancel != null)
			{
				_ctsFadeOutCancel.Cancel();
				_ctsFadeOutCancel.Dispose();
			}
			_ctsFadeOutCancel = new CancellationTokenSource();
			Sequence sequence = DOTween.Sequence();
			_fadeVolumePercentTween.Kill();
			sequence.Join(_volumePercentImage.DOFade(_volumePercentAlphaInFade, _volumePercentFadeInTime));
			sequence.Join(_volumePercentText.DOFade(1f, _volumePercentFadeOutTime));
			_fadeVolumePercentTween = sequence;
			_volumePercentImage.gameObject.SetActive(value: true);
			UpdateVolumeSliderPercent(_volumeSlider.value);
		}).AddTo(this);
		ObservableTriggerExtensions.OnPointerUpAsObservable(_volumeSlider).SubscribeAwait(async delegate(PointerEventData value, CancellationToken ct)
		{
			CancellationToken token = ct.CreateLinkedTokenSource(_ctsFadeOutCancel.Token).Token;
			await UniTask.WaitForSeconds(_waitTime, ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			await DeactivateVolumePercent(token);
		}, AwaitOperation.Switch).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_volumeSlider.OnBeginDragAsObservable(), delegate
		{
			_isHandlingSlider = true;
			_savedValue = _volumeSlider.value;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_volumeSlider.OnDragAsObservable(), delegate
		{
			_muteButton.SetToggleWithoutTransition(_volumeSlider.value == 0f, isNotify: false);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_volumeSlider.OnEndDragAsObservable(), delegate
		{
			_isHandlingSlider = false;
			if (_volumeSlider.value == 0f)
			{
				_currentSliderValue = _savedValue.Value;
				SetMute(mute: true);
				_onVolumeChanged.OnNext(_currentSliderValue);
				_onClickSwitchMuteButton.OnNext(Unit.Default);
				_savedValue = null;
			}
			else
			{
				_onChangedMuteUI.OnNext(_muteButton.IsOn);
				_savedValue = null;
			}
		}).AddTo(this);
		_muteButton.OnValueChanged.Subscribe(delegate(bool mute)
		{
			_onInputAnyNotice.OnNext(Unit.Default);
			_onClickSwitchMuteButton.OnNext(Unit.Default);
			_volumeSlider.SetValueWithoutNotify(mute ? 0f : _currentSliderValue);
			_onChangedMuteUI.OnNext(mute);
		}).AddTo(this);
		_volumePercentAlphaInFade = _volumePercentImage.color.a;
		_volumePercentImage.SetAlpha(0f);
		_volumePercentText.SetAlpha(0f);
	}

	private void Update()
	{
		if (_isHandlingSlider)
		{
			_onInputAnyNotice.OnNext(Unit.Default);
		}
	}

	public void UpdateVolumeSliderPercent(float volumePercent)
	{
		_volumePercentText.SetTextFormat("{0}%", (int)(_volumeSlider.value * 100f));
	}

	public void Deactivate()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
		_volumePercentImage.gameObject.SetActive(value: false);
	}

	public void SetMute(bool mute)
	{
		_muteButton.SetToggleWithoutTransition(mute, isNotify: false);
		_volumeSlider.SetValueWithoutNotify(mute ? 0f : _currentSliderValue);
		_onChangedMuteUI.OnNext(mute);
	}

	public async UniTask DeactivateVolumePercent(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_fadeVolumePercentTween.Kill();
		Sequence sequence = DOTween.Sequence();
		TweenerCore<Color, Color, ColorOptions> t = _volumePercentImage.DOFade(0f, _volumePercentFadeInTime);
		TweenerCore<Color, Color, ColorOptions> t2 = _volumePercentText.DOFade(0f, _volumePercentFadeOutTime);
		_fadeVolumePercentTween = sequence;
		await sequence.Join(t).Join(t2).OnComplete(delegate
		{
			_volumePercentImage.gameObject.SetActive(value: false);
		})
			.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
	}
}
