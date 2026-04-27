using DG.Tweening;
using UnityEngine;

namespace Bulbul.Mobile;

public class MusicPlayerVolumeUISwitch : MonoBehaviour
{
	private static readonly float _duration = 0.25f;

	[SerializeField]
	[Header("音量調整モードで消えるUI")]
	private CanvasGroup _musicPlayerUICanvasGroup;

	[SerializeField]
	[Header("音量調整モードで出すUI")]
	private CanvasGroup _volumeUICanvasGroup;

	[SerializeField]
	private RectTransform _volumeUIRoot;

	[SerializeField]
	private float _volumeUIStartAnchoredX;

	[SerializeField]
	private float _volumeUITargetAnchoredX;

	[SerializeField]
	private CanvasGroup _progressBarCanvasGroup;

	[SerializeField]
	[Header("スライダー 右ピボットにすること")]
	private RectTransform _sliderRectTransform;

	[SerializeField]
	[Header("アニメーション用\u3000右ピボットにすること")]
	private RectTransform _unMaskRectTransform;

	[SerializeField]
	[Header("無操作で自動でDeactivateするか")]
	private bool _enableAutoDeactivationNoOperation;

	[SerializeField]
	private float _autoDeactivationSec = 2.5f;

	private bool _isActive;

	private bool _isTransition;

	private float _timer;

	private Sequence _sequence;

	public void Setup()
	{
		DeactivateVolumeUI();
		_sequence.Complete();
	}

	public void ActivateVolumeUI()
	{
		_timer = 0f;
		_isTransition = true;
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		PrepareAnimation();
		_sequence.Join(_musicPlayerUICanvasGroup.DOFade(0f, _duration).SetEase(Ease.OutCubic));
		_sequence.Join(_volumeUICanvasGroup.DOFade(1f, _duration).SetEase(Ease.OutCubic));
		_sequence.Join(_sliderRectTransform.DOScaleX(1f, _duration).SetEase(Ease.InOutCubic));
		_sequence.Join(_volumeUIRoot.DOAnchorPosX(_volumeUITargetAnchoredX, _duration).SetEase(Ease.InOutCubic));
		_sequence.Join(_unMaskRectTransform.DOScaleX(1f, _duration).SetEase(Ease.OutExpo));
		if (_progressBarCanvasGroup != null)
		{
			_sequence.Join(_progressBarCanvasGroup.DOFade(0f, _duration).SetEase(Ease.OutQuint));
		}
		_sequence.OnComplete(delegate
		{
			_isActive = true;
			_isTransition = false;
			_volumeUICanvasGroup.blocksRaycasts = true;
			_volumeUICanvasGroup.interactable = true;
		});
	}

	public void DeactivateVolumeUI()
	{
		_isTransition = true;
		_sequence?.Kill();
		_sequence = DOTween.Sequence();
		PrepareAnimation();
		_sequence.Join(_musicPlayerUICanvasGroup.DOFade(1f, _duration).SetEase(Ease.OutCubic));
		_sequence.Join(_volumeUICanvasGroup.DOFade(0f, _duration).SetEase(Ease.InCubic));
		_sequence.Join(_sliderRectTransform.DOScaleX(0f, _duration).SetEase(Ease.InOutCubic));
		_sequence.Join(_volumeUIRoot.DOAnchorPosX(_volumeUIStartAnchoredX, _duration).SetEase(Ease.InOutCubic));
		_sequence.Join(_unMaskRectTransform.DOScaleX(0f, _duration).SetEase(Ease.InExpo));
		if (_progressBarCanvasGroup != null)
		{
			_sequence.Join(_progressBarCanvasGroup.DOFade(1f, _duration).SetEase(Ease.InExpo));
		}
		_sequence.OnComplete(delegate
		{
			_isActive = false;
			_isTransition = false;
			_musicPlayerUICanvasGroup.blocksRaycasts = true;
			_musicPlayerUICanvasGroup.interactable = true;
		});
	}

	public void SetResetAutoDeactivationTimer()
	{
		_timer = 0f;
	}

	private void PrepareAnimation()
	{
		_musicPlayerUICanvasGroup.blocksRaycasts = false;
		_musicPlayerUICanvasGroup.interactable = false;
		_volumeUICanvasGroup.blocksRaycasts = false;
		_volumeUICanvasGroup.interactable = false;
	}

	private void Update()
	{
		if (_enableAutoDeactivationNoOperation && _isActive)
		{
			if (_timer >= _autoDeactivationSec && !_isTransition)
			{
				DeactivateVolumeUI();
			}
			_timer += Time.deltaTime;
		}
	}

	private void OnDisable()
	{
		DeactivateVolumeUI();
		_sequence.onComplete();
	}
}
