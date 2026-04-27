using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingGraphicsView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[Header("グラフィック")]
	[SerializeField]
	private GameObject graphicViewRoot;

	[SerializeField]
	private PulldownListUI graphicQualityPulldown;

	[SerializeField]
	private Canvas graphicQualityPulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector graphicQualityPulldownDetector;

	[SerializeField]
	private InteractableUI graphicHigh;

	[SerializeField]
	private InteractableUI graphicMedium;

	[SerializeField]
	private InteractableUI graphicLow;

	[SerializeField]
	private Slider renderScaleSlider;

	[SerializeField]
	private TextMeshProUGUI renderScalePercentText;

	[SerializeField]
	private PulldownListUI frameRatePulldown;

	[SerializeField]
	private Canvas frameRatePulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector frameRatePulldownDetector;

	[SerializeField]
	private InteractableUI frameRateSecondVSync;

	[SerializeField]
	private InteractableUI frameRateVSync;

	[SerializeField]
	private SettingInitButton graphicInitButton;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	private Subject<GraphicQualityLevel> onChangeQualityLevel = new Subject<GraphicQualityLevel>();

	private Subject<float> onChangeRenderScale = new Subject<float>();

	private Subject<int> onChangeFrameRate = new Subject<int>();

	private Subject<Unit> onClickGraphicInit = new Subject<Unit>();

	public Observable<Unit> OnClickClose => onClickClose;

	public Observable<GraphicQualityLevel> OnChangeQualityLevel => onChangeQualityLevel;

	public Observable<float> OnChangeRenderScale => onChangeRenderScale;

	public Observable<int> OnChangeFrameRate => onChangeFrameRate;

	public Observable<Unit> OnClickGraphicInit => onClickGraphicInit;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
		onChangeQualityLevel?.Dispose();
		onChangeRenderScale?.Dispose();
		onChangeFrameRate?.Dispose();
		onClickGraphicInit?.Dispose();
	}

	public void Activate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			viewAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			viewAnimation.Deactivate().Forget();
		}
	}

	public void Setup(SettingData saveData)
	{
		ObservableSubscribeExtensions.Subscribe(closeButton.OnClickAsObservable(), delegate
		{
			onClickClose.OnNext(Unit.Default);
		}).AddTo(this);
		graphicQualityPulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(graphicQualityPulldown.OnClickPullDownButton, delegate
		{
			graphicQualityPulldownCanvas.overrideSorting = true;
			graphicQualityPulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicQualityPulldown.OnClosePulldownComplete, delegate
		{
			graphicQualityPulldownCanvas.overrideSorting = false;
			graphicQualityPulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicQualityPulldownDetector.OnClickOutside, delegate
		{
			if (graphicQualityPulldown.IsOpen)
			{
				graphicQualityPulldown.ClosePullDown();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicHigh.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeQualityLevel.OnNext(GraphicQualityLevel.High);
			graphicQualityPulldown.ClosePullDown();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicMedium.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeQualityLevel.OnNext(GraphicQualityLevel.Medium);
			graphicQualityPulldown.ClosePullDown();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicLow.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeQualityLevel.OnNext(GraphicQualityLevel.Low);
			graphicQualityPulldown.ClosePullDown();
		}).AddTo(this);
		renderScaleSlider.minValue = 35f;
		renderScaleSlider.maxValue = 100f;
		renderScaleSlider.wholeNumbers = true;
		renderScaleSlider.value = saveData.RenderScale.Value;
		renderScaleSlider.OnValueChangedAsObservable().Subscribe(delegate(float scale)
		{
			onChangeRenderScale?.OnNext(scale);
		}).AddTo(this);
		frameRatePulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(frameRatePulldown.OnClickPullDownButton, delegate
		{
			frameRatePulldownCanvas.overrideSorting = true;
			frameRatePulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(frameRatePulldown.OnClosePulldownComplete, delegate
		{
			frameRatePulldownCanvas.overrideSorting = false;
			frameRatePulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(frameRatePulldownDetector.OnClickOutside, delegate
		{
			if (frameRatePulldown.IsOpen)
			{
				frameRatePulldown.ClosePullDown();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(frameRateSecondVSync.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeFrameRate.OnNext(30);
			frameRatePulldown.ClosePullDown();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(frameRateVSync.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeFrameRate.OnNext(60);
			frameRatePulldown.ClosePullDown();
		}).AddTo(this);
		graphicInitButton.Setup();
		ObservableSubscribeExtensions.Subscribe(graphicInitButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onClickGraphicInit.OnNext(Unit.Default);
		}).AddTo(this);
	}

	public void ChangeGraphicQuality(GraphicQualityLevel quality)
	{
		graphicHigh.DeactivateUseUI();
		graphicMedium.DeactivateUseUI();
		graphicLow.DeactivateUseUI();
		switch (quality)
		{
		case GraphicQualityLevel.Low:
			graphicLow.ActivateUseUI();
			graphicQualityPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_low");
			break;
		case GraphicQualityLevel.Medium:
			graphicMedium.ActivateUseUI();
			graphicQualityPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_middle");
			break;
		case GraphicQualityLevel.High:
			graphicHigh.ActivateUseUI();
			graphicQualityPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_high");
			break;
		}
	}

	public void ChangeRenderScale(int renderScale)
	{
		renderScaleSlider.SetValueWithoutNotify(renderScale);
		renderScalePercentText.SetText($"{renderScale}%");
	}

	public void ChangeFrameRate(int frameRate)
	{
		frameRateSecondVSync.DeactivateUseUI();
		frameRateVSync.DeactivateUseUI();
		switch (frameRate)
		{
		case 30:
			frameRateSecondVSync.ActivateUseUI();
			frameRatePulldown.ChangeSelectContentText("30 FPS");
			break;
		case 60:
			frameRateVSync.ActivateUseUI();
			frameRatePulldown.ChangeSelectContentText("60 FPS");
			break;
		}
	}

	public void ChangeGraphicSetting(bool isChange)
	{
		if (isChange)
		{
			graphicInitButton.Activate();
		}
		else
		{
			graphicInitButton.Deactivate();
		}
	}
}
