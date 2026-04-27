using System;
using Cysharp.Text;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class AutoTimeWindowView : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private LocalizationMasterWrapper _localizationMaster;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	[Header("時間帯ゲージ（幅は固定）")]
	private RectTransform _gaugeBar;

	[SerializeField]
	[Header("ピンの親（TimePins）")]
	private RectTransform _pinsParent;

	[SerializeField]
	[Header("ゲージセグメント（左から 夜(左)・日中・夕方・夜(右)）")]
	private RectTransform _nightBarLeft;

	[SerializeField]
	private RectTransform _dayBar;

	[SerializeField]
	private RectTransform _sunsetBar;

	[SerializeField]
	private RectTransform _nightBarRight;

	[SerializeField]
	[Header("ゲージの固定幅（0の場合はAwake時の幅を使用）")]
	private float _fixedGaugeWidth;

	[SerializeField]
	[Header("初期値（時）:日中, 夕方, 夜始まり")]
	private float _initialDayStartHour = 6f;

	[SerializeField]
	private float _initialSunsetStartHour = 17f;

	[SerializeField]
	private float _initialNightStartHour = 19f;

	[SerializeField]
	[Header("ピンの刻み（分）。0で刻みなし")]
	private int _pinStepMinutes = 10;

	[SerializeField]
	[Header("各ピンの設定時間表示（任意）: 日中, 夕方, 夜始まり")]
	private TMP_Text _timeLabelDayStart;

	[SerializeField]
	private TMP_Text _timeLabelSunsetStart;

	[SerializeField]
	private TMP_Text _timeLabelNightStart;

	[SerializeField]
	[Header("メーター目盛りラベル: 0時, 12時, 24時")]
	private TMP_Text _scaleLabel0;

	[SerializeField]
	private TMP_Text _scaleLabel12;

	[SerializeField]
	private TMP_Text _scaleLabel24;

	[SerializeField]
	[Header("簡体字用：メーターのフォントサイズ")]
	private int _ampmChineseSimplifiedMeterFontSize = 15;

	[SerializeField]
	[Header("各言語用：AMPMのフォントサイズ")]
	private int _ampmfontSize = 13;

	[SerializeField]
	[Header("簡体字用：上午下午フォントサイズ")]
	private int _ampmChineseSimplifiedFontSize = 13;

	[SerializeField]
	[Header("時間表示のフェード: この距離(px)未満で重なると判定してフェードアウト")]
	private float _timeLabelOverlapThresholdPixels = 50f;

	[SerializeField]
	[Header("時間表示のフェード: AMPM時のこの距離(px)未満で重なると判定してフェードアウト")]
	private float _timeLabelOverlapThresholdPixelsAmPm = 55f;

	[SerializeField]
	[Header("時間表示のフェード: 簡体字AMPM時のこの距離(px)未満で重なると判定してフェードアウト")]
	private float _timeLabelOverlapThresholdPixelsAmPmChinese = 57f;

	[SerializeField]
	[Header("時間表示のフェード速度（アルファ/秒）")]
	private float _timeLabelFadeSpeed = 5f;

	[SerializeField]
	[Header("設定画面のUICanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("初期化ボタン")]
	private Button _initButton;

	[SerializeField]
	[Header("枠外クリックで設定を閉じる用")]
	private ClickOutsideDetector _clickOutsideDetector;

	private AutoTimeWindowSettings _settings;

	private RectTransform[] _pinRects;

	private InteractableUI[] _pinInteractableUIs;

	private Camera _camera;

	private bool _isDragging;

	private int _draggingPinIndex = -1;

	private int _lastTouchedPinIndex = -1;

	private float _gaugeHeight;

	private float _gaugeWidth;

	private float _timeLabelAlpha0 = 1f;

	private float _timeLabelAlpha1 = 1f;

	private float _timeLabelAlpha2 = 1f;

	private bool _isActive;

	private Tween _fadeTween;

	private RectTransform _rectTransform;

	private float _fromPosY;

	private float _toPosY;

	private Tween _moveTween;

	private Subject<AutoTimeWindowSettings> _onSave = new Subject<AutoTimeWindowSettings>();

	private Subject<Unit> _onOutsideClick = new Subject<Unit>();

	public AutoTimeWindowSettings Settings => _settings;

	public Observable<AutoTimeWindowSettings> OnSave => _onSave;

	public Observable<Unit> OnOutsideClick => _onOutsideClick;

	private void Start()
	{
		if (_gaugeWidth <= 0f)
		{
			_gaugeWidth = _gaugeBar.rect.width;
		}
		ApplyTimesToPinsAndBars();
	}

	public void Setup()
	{
		_clickOutsideDetector.OnClickOutside.Subscribe(delegate(Unit _)
		{
			_onOutsideClick.OnNext(_);
		}).AddTo(this);
		AutoTimeWindowChangeData autoTimeWindowChangeData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		_settings = new AutoTimeWindowSettings(autoTimeWindowChangeData.TimeDayStart, autoTimeWindowChangeData.TimeSunsetStart, autoTimeWindowChangeData.TimeNightStart, _pinStepMinutes);
		int childCount = _pinsParent.childCount;
		_pinRects = new RectTransform[childCount];
		_pinInteractableUIs = new InteractableUI[childCount];
		for (int num = 0; num < childCount; num++)
		{
			_pinRects[num] = _pinsParent.GetChild(num) as RectTransform;
			_pinInteractableUIs[num] = _pinRects[num].GetComponentInChildren<InteractableUI>();
		}
		if (childCount >= 3)
		{
			_pinRects[0].SetSiblingIndex(0);
			_pinRects[1].SetSiblingIndex(1);
			_pinRects[2].SetAsLastSibling();
		}
		for (int num2 = 0; num2 < childCount; num2++)
		{
			int capturedIndex = num2;
			_pinInteractableUIs[num2].SetPointerEnterAction(delegate
			{
				OnPinHoverEnter(capturedIndex);
			});
			_pinInteractableUIs[num2].SetPointerExitAction(delegate
			{
				OnPinHoverExit(capturedIndex);
			});
		}
		Canvas componentInParent = _gaugeBar.GetComponentInParent<Canvas>();
		_camera = ((componentInParent != null && componentInParent.renderMode != RenderMode.ScreenSpaceOverlay) ? componentInParent.worldCamera : null);
		HorizontalOrVerticalLayoutGroup component = _gaugeBar.GetComponent<HorizontalOrVerticalLayoutGroup>();
		if (component != null)
		{
			component.enabled = false;
		}
		ContentSizeFitter component2 = _gaugeBar.GetComponent<ContentSizeFitter>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
		_gaugeWidth = ((_fixedGaugeWidth > 0f) ? _fixedGaugeWidth : _gaugeBar.rect.width);
		_gaugeHeight = _gaugeBar.rect.height;
		if (_gaugeHeight <= 0f && _dayBar != null)
		{
			_gaugeHeight = _dayBar.rect.height;
		}
		ObservableSubscribeExtensions.Subscribe(Observable.Merge<Unit>(SaveDataManager.Instance.SettingData.TimeFormat.AsUnitObservable(), _languageSupplier.Language.AsUnitObservable()), delegate
		{
			RefreshTimeLabels();
		}).AddTo(this);
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y - -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _fromPosY);
		_canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		_clickOutsideDetector.AddExcludeTransform(_rectTransform);
		_clickOutsideDetector.enabled = false;
		ObservableSubscribeExtensions.Subscribe(_clickOutsideDetector.OnClickOutside, delegate
		{
			if (_isActive)
			{
				DeactivateUI();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_initButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			SaveDataManager.Instance.AutoTimeWindowChangeData.InitTimeData();
			SaveDataManager.Instance.SaveAutoTimeWindowChangeData();
			RefreshFromSaveData();
		}).AddTo(this);
	}

	public void ActivateUI()
	{
		_isActive = true;
		base.gameObject.SetActive(value: true);
		_clickOutsideDetector.enabled = true;
		_timeLabelAlpha0 = 0f;
		_timeLabelAlpha1 = 0f;
		_timeLabelAlpha2 = 0f;
		SetLabelAlpha(_timeLabelDayStart, _timeLabelAlpha0);
		SetLabelAlpha(_timeLabelSunsetStart, _timeLabelAlpha1);
		SetLabelAlpha(_timeLabelNightStart, _timeLabelAlpha2);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void DeactivateUI()
	{
		_isActive = false;
		_clickOutsideDetector.enabled = false;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void Update()
	{
		if (!_isActive)
		{
			return;
		}
		if (_isDragging)
		{
			if (!Input.GetMouseButton(0))
			{
				_lastTouchedPinIndex = _draggingPinIndex;
				_pinInteractableUIs[_draggingPinIndex].DeactivateUseUI();
				_isDragging = false;
				_draggingPinIndex = -1;
				_onSave.OnNext(_settings);
				return;
			}
			UpdateDraggingPinFromMouse();
			ApplyTimesToPinsAndBars();
		}
		UpdateTimeLabelFade();
	}

	public void RefreshFromSaveData()
	{
		AutoTimeWindowChangeData autoTimeWindowChangeData = SaveDataManager.Instance.AutoTimeWindowChangeData;
		_settings = new AutoTimeWindowSettings(autoTimeWindowChangeData.TimeDayStart, autoTimeWindowChangeData.TimeSunsetStart, autoTimeWindowChangeData.TimeNightStart, _pinStepMinutes);
		ApplyTimesToPinsAndBars();
	}

	public void BeginDragPin(RectTransform pinRect)
	{
		int pinIndexByRect = GetPinIndexByRect(pinRect);
		if (pinIndexByRect >= 0)
		{
			_systemSeService.PlayClick();
			_draggingPinIndex = pinIndexByRect;
			_lastTouchedPinIndex = pinIndexByRect;
			_isDragging = true;
			_pinRects[pinIndexByRect].SetAsLastSibling();
			_pinInteractableUIs[pinIndexByRect].ActivateUseUI();
		}
	}

	private void OnPinHoverEnter(int index)
	{
		if (!_isDragging)
		{
			_lastTouchedPinIndex = index;
			_pinRects[index].SetAsLastSibling();
		}
	}

	private void OnPinHoverExit(int index)
	{
		if (_lastTouchedPinIndex == index)
		{
			_lastTouchedPinIndex = -1;
		}
	}

	private int GetPinIndexByRect(RectTransform pinRect)
	{
		for (int i = 0; i < _pinRects.Length; i++)
		{
			if (_pinRects[i] == pinRect)
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateDraggingPinFromMouse()
	{
		float hours = GetTime01FromScreenPosition(Input.mousePosition) * 24f;
		_settings.SetTime(_draggingPinIndex, hours);
	}

	private float GetTime01FromScreenPosition(Vector2 screenPos)
	{
		Rect rect = _gaugeBar.rect;
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(_camera, _gaugeBar.TransformPoint(new Vector3(rect.xMin, 0f, 0f)));
		Vector2 vector2 = RectTransformUtility.WorldToScreenPoint(_camera, _gaugeBar.TransformPoint(new Vector3(rect.xMax, 0f, 0f)));
		return Mathf.Clamp01(Mathf.InverseLerp(vector.x, vector2.x, screenPos.x));
	}

	private void ApplyTimesToPinsAndBars()
	{
		_gaugeBar.sizeDelta = new Vector2(_gaugeWidth, _gaugeBar.sizeDelta.y);
		float gaugeHeight = _gaugeHeight;
		float num = _settings.TimeDayStart / 24f;
		float num2 = _settings.TimeSunsetStart / 24f;
		float num3 = Mathf.Clamp01(_settings.TimeNightStart / 24f);
		float x = TimeToLocalX(num);
		float x2 = TimeToLocalX(num2);
		float x3 = TimeToLocalX(num3);
		_pinRects[0].anchoredPosition = new Vector2(x, _pinRects[0].anchoredPosition.y);
		_pinRects[1].anchoredPosition = new Vector2(x2, _pinRects[1].anchoredPosition.y);
		_pinRects[2].anchoredPosition = new Vector2(x3, _pinRects[2].anchoredPosition.y);
		float gaugeWidth = _gaugeWidth;
		float num4 = Mathf.Clamp01(num);
		float num5 = Mathf.Max(0f, num2 - num);
		float num6 = ((num3 >= num2) ? Mathf.Max(0f, num3 - num2) : Mathf.Max(0f, 1f - num2));
		float num7 = ((num3 >= num2) ? Mathf.Max(0f, 1f - num3) : 0f);
		float posX = 0f;
		float posX2 = num4 * gaugeWidth;
		float posX3 = (num4 + num5) * gaugeWidth;
		float posX4 = (num4 + num5 + num6) * gaugeWidth;
		if (_nightBarLeft != null)
		{
			SetBarRect(_nightBarLeft, posX, num4 * gaugeWidth, gaugeHeight);
		}
		if (_dayBar != null)
		{
			SetBarRect(_dayBar, posX2, num5 * gaugeWidth, gaugeHeight);
		}
		if (_sunsetBar != null)
		{
			SetBarRect(_sunsetBar, posX3, num6 * gaugeWidth, gaugeHeight);
		}
		if (_nightBarRight != null)
		{
			SetBarRect(_nightBarRight, posX4, num7 * gaugeWidth, gaugeHeight);
		}
		RefreshTimeLabels();
	}

	private float TimeToLocalX(float timeNormalized)
	{
		Rect rect = _gaugeBar.rect;
		Vector3 worldPoint = _gaugeBar.TransformPoint(new Vector3(rect.xMin, 0f, 0f));
		Vector3 worldPoint2 = _gaugeBar.TransformPoint(new Vector3(rect.xMax, 0f, 0f));
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(_camera, worldPoint);
		Vector2 vector2 = RectTransformUtility.WorldToScreenPoint(_camera, worldPoint2);
		float x = Mathf.Lerp(vector.x, vector2.x, timeNormalized);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_pinsParent, new Vector2(x, vector.y), _camera, out var localPoint);
		return localPoint.x;
	}

	private void RefreshTimeLabels()
	{
		FormatTimeHours(_timeLabelDayStart, _settings.TimeDayStart);
		FormatTimeHours(_timeLabelSunsetStart, _settings.TimeSunsetStart);
		FormatTimeHours(_timeLabelNightStart, _settings.TimeNightStart);
		if (IsEffectiveAmPm())
		{
			GameLanguageType num = _languageSupplier.Get();
			_localizationMaster.TryGet("ui_time_am", out var result);
			_localizationMaster.TryGet("ui_time_pm", out var result2);
			if (num == GameLanguageType.ChineseSimplified)
			{
				_localizationMaster.TryGet("ui_time_midtime", out var result3);
				_scaleLabel0.SetTextFormat("<size={0}>{1}0</size>", _ampmChineseSimplifiedMeterFontSize, result);
				_scaleLabel12.SetTextFormat("<size={0}>{1}12</size>", _ampmChineseSimplifiedMeterFontSize, result3);
				_scaleLabel24.SetTextFormat("<size={0}>{1}0</size>", _ampmChineseSimplifiedMeterFontSize, result);
			}
			else
			{
				_scaleLabel0.SetTextFormat("12<size={0}>{1}</size>", _ampmfontSize, result);
				_scaleLabel12.SetTextFormat("12<size={0}>{1}</size>", _ampmfontSize, result2);
				_scaleLabel24.SetTextFormat("12<size={0}>{1}</size>", _ampmfontSize, result);
			}
		}
		else
		{
			_scaleLabel0.SetText("00");
			_scaleLabel12.SetText("12");
			_scaleLabel24.SetText("24");
		}
	}

	private void UpdateTimeLabelFade()
	{
		if (_pinRects == null || _pinRects.Length < 3)
		{
			return;
		}
		float x = _pinRects[0].anchoredPosition.x;
		float x2 = _pinRects[1].anchoredPosition.x;
		float x3 = _pinRects[2].anchoredPosition.x;
		float num = _timeLabelOverlapThresholdPixels;
		if (IsEffectiveAmPm())
		{
			num = ((_languageSupplier.Get() != GameLanguageType.ChineseSimplified) ? _timeLabelOverlapThresholdPixelsAmPm : _timeLabelOverlapThresholdPixelsAmPmChinese);
		}
		float maxDelta = _timeLabelFadeSpeed * Time.deltaTime;
		int siblingIndex = _pinRects[0].GetSiblingIndex();
		int siblingIndex2 = _pinRects[1].GetSiblingIndex();
		int siblingIndex3 = _pinRects[2].GetSiblingIndex();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (Mathf.Abs(x - x2) < num)
		{
			if (siblingIndex < siblingIndex2)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
		}
		if (Mathf.Abs(x2 - x3) < num)
		{
			if (siblingIndex2 < siblingIndex3)
			{
				flag2 = true;
			}
			else
			{
				flag3 = true;
			}
		}
		if (Mathf.Abs(x - x3) < num)
		{
			if (siblingIndex < siblingIndex3)
			{
				flag = true;
			}
			else
			{
				flag3 = true;
			}
		}
		else
		{
			if (flag && flag2)
			{
				flag = false;
			}
			if (flag2 && flag3)
			{
				flag3 = false;
			}
		}
		bool flag4 = _draggingPinIndex == 0 || _lastTouchedPinIndex == 0;
		bool flag5 = _draggingPinIndex == 1 || _lastTouchedPinIndex == 1;
		bool flag6 = _draggingPinIndex == 2 || _lastTouchedPinIndex == 2;
		float target = ((flag && !flag4) ? 0f : 1f);
		float target2 = ((flag2 && !flag5) ? 0f : 1f);
		float target3 = ((flag3 && !flag6) ? 0f : 1f);
		_timeLabelAlpha0 = Mathf.MoveTowards(_timeLabelAlpha0, target, maxDelta);
		_timeLabelAlpha1 = Mathf.MoveTowards(_timeLabelAlpha1, target2, maxDelta);
		_timeLabelAlpha2 = Mathf.MoveTowards(_timeLabelAlpha2, target3, maxDelta);
		SetLabelAlpha(_timeLabelDayStart, _timeLabelAlpha0);
		SetLabelAlpha(_timeLabelSunsetStart, _timeLabelAlpha1);
		SetLabelAlpha(_timeLabelNightStart, _timeLabelAlpha2);
	}

	private static void SetLabelAlpha(TMP_Text label, float alpha)
	{
		Color color = label.color;
		color.a = alpha;
		label.color = color;
	}

	private bool IsEffectiveAmPm()
	{
		return SaveDataManager.Instance.SettingData.TimeFormat.Value == TimeFormatType.AMPM;
	}

	private void FormatTimeHours(TMP_Text text, float hours)
	{
		hours = Mathf.Clamp(hours, 0f, 24f);
		int num = Mathf.FloorToInt(hours);
		int num2 = Mathf.RoundToInt((hours - (float)num) * 60f);
		if (num2 >= 60)
		{
			num2 = 0;
			num++;
		}
		if (IsEffectiveAmPm())
		{
			if (_languageSupplier.Get() == GameLanguageType.ChineseSimplified)
			{
				string empty = string.Empty;
				if ((num >= 0 && num < 12) || num == 24)
				{
					_localizationMaster.TryGet("ui_time_am", out var result);
					empty = result;
					if (num == 24)
					{
						num = 0;
					}
				}
				else if (num >= 12 && num < 13)
				{
					_localizationMaster.TryGet("ui_time_midtime", out var result2);
					empty = result2;
				}
				else
				{
					_localizationMaster.TryGet("ui_time_pm", out var result3);
					empty = result3;
					num %= 12;
				}
				text.SetTextFormat("<size={0}>{1}</size>{2}:{3:D2}", _ampmChineseSimplifiedFontSize, empty, num, num2);
			}
			else
			{
				DateTime dateTime = new DateTime(2000, 1, 1, num % 24, num2, 0);
				bool flag = dateTime.Hour < 12;
				_localizationMaster.TryGet(flag ? "ui_time_am" : "ui_time_pm", out var result4);
				int num3 = dateTime.Hour % 12;
				if (num3 == 0)
				{
					num3 = 12;
				}
				text.SetTextFormat("{0}:{1:D2}<size={2}>{3}</size>", num3, num2, _ampmfontSize, result4);
			}
		}
		else if (hours >= 23.99f)
		{
			text.SetText("24:00");
		}
		else
		{
			text.SetTextFormat("{0:D2}:{1:D2}", num, num2);
		}
	}

	private static void SetBarRect(RectTransform bar, float posX, float width, float height)
	{
		bar.anchorMin = new Vector2(0f, 0f);
		bar.anchorMax = new Vector2(0f, 0f);
		bar.pivot = new Vector2(0f, 0f);
		bar.anchoredPosition = new Vector2(posX, 0f);
		bar.sizeDelta = new Vector2(Mathf.Max(0f, width), height);
	}
}
