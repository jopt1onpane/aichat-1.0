using DG.Tweening;
using NestopiSystem;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

public class MusicVolumeUI : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private RectTransform sliderPadding;

	[SerializeField]
	private EventTrigger pointerEnterHandler;

	[SerializeField]
	[Header("ボリュームアイコン")]
	private Image volumeIconImage;

	[SerializeField]
	[Header("ミュートしてないアイコン画像")]
	private Sprite nonMuteIconSprite;

	[SerializeField]
	[Header("ミュートアイコン画像")]
	private Sprite muteIconSprite;

	private bool isMouseover;

	private bool isDraggingSlider;

	private RectTransform sliderRectTransform;

	private float sliderHeight;

	private Tween sliderShowTween;

	private float? savedVolume;

	private void Start()
	{
		pointerEnterHandler.OnPointerEnterAsObservable().Subscribe(OnPointerEnter).AddTo(this);
		slider.OnBeginDragAsObservable().Subscribe(OnBeginDragSlider).AddTo(this);
		slider.OnEndDragAsObservable().Subscribe(OnEndDragSlider).AddTo(this);
		slider.OnValueChangedAsObservable().Subscribe(SetVolumeInternal).AddTo(this);
		sliderRectTransform = slider.transform as RectTransform;
		sliderHeight = sliderRectTransform.sizeDelta.y;
	}

	private void Update()
	{
		Vector2 screenPoint = Input.mousePosition;
		isMouseover = RectTransformUtility.RectangleContainsScreenPoint(sliderPadding, screenPoint);
		if (slider.gameObject.activeInHierarchy && !isMouseover && !isDraggingSlider)
		{
			slider.gameObject.SetActive(value: false);
		}
	}

	private void OnPointerEnter()
	{
		if (!slider.gameObject.activeInHierarchy)
		{
			slider.gameObject.SetActive(value: true);
			sliderShowTween?.Kill(complete: true);
			sliderRectTransform.sizeDelta = new Vector2(sliderRectTransform.sizeDelta.x, 0f);
			sliderShowTween = sliderRectTransform.DOSizeDelta(new Vector2(sliderRectTransform.sizeDelta.x, sliderHeight), 0.05f);
		}
	}

	private void OnBeginDragSlider()
	{
		isDraggingSlider = true;
		if (slider.value > 0f)
		{
			savedVolume = slider.value;
		}
	}

	private void OnEndDragSlider()
	{
		isDraggingSlider = false;
		if (!isMouseover)
		{
			slider.gameObject.SetActive(value: false);
		}
		if (slider.value > 0f)
		{
			savedVolume = null;
		}
	}

	public void SetVolume(float volume)
	{
		slider.value = volume;
	}

	private void SetVolumeInternal(float volume)
	{
	}

	public void SetMute(bool isMute)
	{
		if (isMute)
		{
			if (slider.value > 0f)
			{
				savedVolume = slider.value;
			}
			slider.value = 0f;
		}
		else if (!isMute && savedVolume.HasValue && volumeIconImage.sprite == muteIconSprite)
		{
			if (!isDraggingSlider)
			{
				slider.value = savedVolume.Value;
			}
			savedVolume = null;
		}
		volumeIconImage.sprite = (isMute ? muteIconSprite : nonMuteIconSprite);
	}
}
