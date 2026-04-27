using System;
using System.Collections.Generic;
using System.Threading;
using Bulbul.MasterData;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class StorySystemUIForMobile : MonoBehaviour, IStorySystemUI
{
	[Serializable]
	public class StorySystemUIAddMobile
	{
		public SubTitleAutoFitter _normalTextAutoFitter;

		public CanvasScalerReferenceResolusionSwitcher _resoSwitcher;

		public RectTransform _normalTextOffsetRectTransform;

		public RectTransform _mainTextParentRectTransform;

		public RectTransform _heroineNameRectTransform;
	}

	[Serializable]
	public class StorySystemMobileObjPositions
	{
		public float NormalTextPortraitY;

		public float NormalTextLandscapeY;

		public float MainTextPortraitHeight;

		public float MainTextLandscapeHeight;

		public float HeroineNamePortraitY;

		public float HeroineNameLandscapeY;
	}

	private StorySystemUI.MessageType _currentMessageType;

	private ScreenOrientation _currentScreenOrientation;

	[SerializeField]
	private Button _transparentButton;

	[SerializeField]
	private ScenarioTextMessage _mainStoryTextMessage;

	[SerializeField]
	private GameObject _mainStoryTextParent;

	[SerializeField]
	private ScenarioTextMessage _normalTextMessage;

	[SerializeField]
	private GameObject _normalTextParent;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _normalTextCanvasGroup;

	[SerializeField]
	private SentenceSelectionButtonsUI _selectionButtonsUI;

	[SerializeField]
	private Button _skipButton;

	[SerializeField]
	private TMP_Text _skipText;

	[SerializeField]
	private Color _buttonDeactiveColor;

	[SerializeField]
	private Color _buttonActiveColor;

	[SerializeField]
	private GameObject _heroineNameObject;

	[SerializeField]
	private TextLocalizationBehaviour _heroineNameTextLocalizationBehavior;

	[SerializeField]
	private StorySystemUIAddMobile _mobileAddUIs;

	[SerializeField]
	[Header("壁紙モードで縦横が変わるのでその場合の位置変更対応用")]
	private StorySystemMobileObjPositions _mobileObjPositions;

	[SerializeField]
	private FadeController _storyFadeController;

	private readonly List<Action> _onClickList = new List<Action>();

	private Subject<Unit> _onClickButtonSkip = new Subject<Unit>();

	private Subject<Unit> _onClickButtonAuto = new Subject<Unit>();

	private bool _isActiveNormalText;

	private RectTransform _rectTransform;

	private Tween _normalTextMoveTween;

	private Tween _normalTextFadeTween;

	private float _fromPosY;

	private float _toPosY;

	private readonly List<Action> _callbacksForLoop = new List<Action>();

	public StorySystemUI.MessageType CurrentMessageType => _currentMessageType;

	public SentenceSelectionButtonsUI SelectionButtonsUI => _selectionButtonsUI;

	public Observable<Unit> OnClickButtonSkip => _onClickButtonSkip;

	public Observable<Unit> OnClickButtonAuto => _onClickButtonAuto;

	public CancellationToken CancellationTokenOnDestroy => base.destroyCancellationToken;

	FadeController IStorySystemUI.FadeController => _storyFadeController;

	public void Setup()
	{
		_storyFadeController.ImmediateFadeIn();
		if (_mobileAddUIs._normalTextAutoFitter != null && _mobileAddUIs._resoSwitcher != null)
		{
			_mobileAddUIs._resoSwitcher.OnSwitchedRefResolution.Subscribe(delegate(ScreenOrientation orientation)
			{
				Vector2 anchoredPosition = _mobileAddUIs._normalTextOffsetRectTransform.anchoredPosition;
				Vector2 anchoredPosition2 = _mobileAddUIs._heroineNameRectTransform.anchoredPosition;
				switch (orientation)
				{
				case ScreenOrientation.LandscapeLeft:
				case ScreenOrientation.LandscapeRight:
					anchoredPosition.y = _mobileObjPositions.NormalTextLandscapeY;
					anchoredPosition2.y = _mobileObjPositions.HeroineNameLandscapeY;
					_mobileAddUIs._mainTextParentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mobileObjPositions.MainTextLandscapeHeight);
					break;
				case ScreenOrientation.Portrait:
				case ScreenOrientation.PortraitUpsideDown:
					anchoredPosition.y = _mobileObjPositions.NormalTextPortraitY;
					anchoredPosition2.y = _mobileObjPositions.HeroineNamePortraitY;
					_mobileAddUIs._mainTextParentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _mobileObjPositions.MainTextPortraitHeight);
					break;
				}
				_currentScreenOrientation = orientation;
				SelectionButtonsUI.IsEnabled.OnNext(SelectionButtonsUI.IsEnabled.Value);
				_mobileAddUIs._normalTextOffsetRectTransform.anchoredPosition = anchoredPosition;
				_mobileAddUIs._heroineNameRectTransform.anchoredPosition = anchoredPosition2;
				_mobileAddUIs._normalTextAutoFitter.RequestLayout();
			}).AddTo(this);
		}
		SelectionButtonsUI.IsEnabled.Subscribe(delegate(bool isEnable)
		{
			if (isEnable && (_currentScreenOrientation == ScreenOrientation.LandscapeLeft || _currentScreenOrientation == ScreenOrientation.LandscapeRight))
			{
				_mainStoryTextParent.SetActive(value: false);
				_normalTextParent.SetActive(value: false);
				_heroineNameObject.SetActive(value: false);
			}
			else
			{
				_mainStoryTextParent.SetActive(_currentMessageType != StorySystemUI.MessageType.Normal);
				_normalTextParent.SetActive(_currentMessageType == StorySystemUI.MessageType.Normal);
				_heroineNameObject.SetActive(_currentMessageType != StorySystemUI.MessageType.Normal);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(SelectionButtonsUI.OnSelectionButtonClicked, delegate
		{
			_mainStoryTextMessage.ClearText();
			_normalTextMessage.ClearText();
		}).AddTo(this);
		_transparentButton.onClick.AddListener(NextClick);
		ObservableSubscribeExtensions.Subscribe(_skipButton.OnClickAsObservable(), delegate
		{
			_onClickButtonSkip.OnNext(Unit.Default);
		}).AddTo(this);
		DeactivateSkipButton();
		_transparentButton.gameObject.SetActive(value: false);
		_rectTransform = _normalTextParent.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_normalTextCanvasGroup.alpha = 0f;
		_heroineNameObject.SetActive(value: false);
		_currentMessageType = StorySystemUI.MessageType.Normal;
		_mainStoryTextParent.SetActive(value: false);
		_normalTextParent.SetActive(value: false);
	}

	private void OnDestroy()
	{
		_onClickList.Clear();
		_callbacksForLoop.Clear();
	}

	private void NextClick()
	{
		_callbacksForLoop.Clear();
		_callbacksForLoop.AddRange(_onClickList);
		foreach (Action item in _callbacksForLoop)
		{
			try
			{
				item();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void AddOnClickCallback(Action callback)
	{
		if (_onClickList.Contains(callback))
		{
			Debug.LogError(callback?.Method.Name + " is already added");
		}
		else
		{
			_onClickList.Add(callback);
		}
	}

	public void ClearOnClickCallback()
	{
		_onClickList.Clear();
	}

	public void MainStoryReady(string novelID, ScenarioType scenarioType)
	{
		_selectionButtonsUI.DisableButtons(isDoComplete: true);
		_normalTextCanvasGroup.alpha = 0f;
		_normalTextParent.SetActive(value: false);
		_normalTextMessage.ClearOnTextShowedCallback();
		_normalTextMessage.StartText(string.Empty);
		if ((uint)(scenarioType - 1) <= 2u || scenarioType == ScenarioType.ExtraScenario || (uint)(scenarioType - 50) <= 4u)
		{
			_currentMessageType = StorySystemUI.MessageType.MainStory;
			if (novelID == "main_01")
			{
				_heroineNameTextLocalizationBehavior.GetComponent<TextMeshProUGUI>().text = "？？？？";
			}
			else
			{
				_heroineNameTextLocalizationBehavior.Set("heroine_name");
			}
			_heroineNameObject.SetActive(value: true);
		}
		else
		{
			_currentMessageType = StorySystemUI.MessageType.Normal;
		}
	}

	public void ChangeMessageType(StorySystemUI.MessageType type)
	{
		Finish();
		_currentMessageType = type;
	}

	public void Begin(bool isUseMask = true)
	{
		if (isUseMask)
		{
			ActivateTransparentButton();
		}
		else
		{
			DeactivateTransparentButton();
		}
		_mainStoryTextParent.SetActive(_currentMessageType == StorySystemUI.MessageType.MainStory);
		_normalTextParent.SetActive(_currentMessageType == StorySystemUI.MessageType.Normal);
		switch (_currentMessageType)
		{
		case StorySystemUI.MessageType.MainStory:
			_mainStoryTextMessage.StartText(string.Empty);
			break;
		case StorySystemUI.MessageType.Normal:
			_normalTextMessage.StartText(string.Empty);
			break;
		}
	}

	public void Finish()
	{
		ActivateBottomBackImage();
		_selectionButtonsUI.DisableButtons(isDoComplete: true);
		ClearOnClickCallback();
		switch (_currentMessageType)
		{
		case StorySystemUI.MessageType.MainStory:
			_mainStoryTextMessage.ClearOnTextShowedCallback();
			_mainStoryTextMessage.StartText(string.Empty);
			DeactivateTransparentButton();
			_heroineNameObject.SetActive(value: false);
			_mainStoryTextParent.SetActive(value: false);
			DeactivateSkipButton();
			break;
		case StorySystemUI.MessageType.Normal:
		{
			_normalTextMessage.ClearOnTextShowedCallback();
			Action onEndAction = delegate
			{
				_normalTextParent.SetActive(value: false);
				DeactivateTransparentButton();
			};
			DeactivateNormalText(onEndAction);
			break;
		}
		}
		_currentMessageType = StorySystemUI.MessageType.Normal;
	}

	public void ActivateBottomBackImage()
	{
	}

	public void DeactivateBottomBackImage()
	{
	}

	public bool IsActiveNormalText()
	{
		return _isActiveNormalText;
	}

	public void ActivateNormalText()
	{
		_isActiveNormalText = true;
		_normalTextParent.SetActive(value: true);
		_normalTextMoveTween?.Kill();
		_normalTextFadeTween?.Kill();
		_normalTextMoveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_normalTextFadeTween = _normalTextCanvasGroup.DOFade(1f, 0.2f);
	}

	public void DeactivateNormalText(Action onEndAction = null)
	{
		_isActiveNormalText = false;
		_normalTextMoveTween?.Kill();
		_normalTextFadeTween?.Kill();
		_normalTextMoveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_normalTextFadeTween = _normalTextCanvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_normalTextMessage.StartText(string.Empty);
			onEndAction?.Invoke();
		});
	}

	public void ActivateSkipButton()
	{
		_skipButton.gameObject.SetActive(value: true);
	}

	public void DeactivateSkipButton()
	{
		_skipButton.gameObject.SetActive(value: false);
	}

	public void ActivateAutoButton()
	{
	}

	public void DeactivateAutoButton()
	{
	}

	public void ActivateTransparentButton()
	{
		_transparentButton.gameObject.SetActive(value: true);
	}

	public void DeactivateTransparentButton()
	{
		_transparentButton.gameObject.SetActive(value: false);
	}

	public ScenarioTextMessage CurrentTextMessage()
	{
		return _currentMessageType switch
		{
			StorySystemUI.MessageType.MainStory => _mainStoryTextMessage, 
			StorySystemUI.MessageType.Normal => _normalTextMessage, 
			_ => _mainStoryTextMessage, 
		};
	}

	public void DebugClearAll()
	{
		_mainStoryTextMessage.ClearOnTextShowedCallback();
		_normalTextMessage.ClearOnTextShowedCallback();
		ClearOnClickCallback();
		_callbacksForLoop.Clear();
	}

	public void DebugDeactivateParentUI()
	{
		base.gameObject.transform.localPosition = new Vector3(0f, 5000f, 0f);
		_skipButton.gameObject.SetActive(value: false);
		_heroineNameObject.gameObject.SetActive(value: false);
	}

	public void DebugActivateParentUI()
	{
		base.gameObject.transform.localPosition = Vector3.zero;
		_skipButton.gameObject.SetActive(value: true);
		_heroineNameObject.gameObject.SetActive(value: true);
	}
}
