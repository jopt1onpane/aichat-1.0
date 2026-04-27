using System;
using System.Collections.Generic;
using System.Threading;
using Bulbul.MasterData;
using DG.Tweening;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class StorySystemUI : MonoBehaviour, IStorySystemUI
{
	public enum MessageType
	{
		MainStory,
		Normal
	}

	private MessageType _currentMessageType;

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
	private Button _autoButton;

	[SerializeField]
	private TMP_Text _auteText;

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
	[Header("画面下側の背景")]
	private Image _bottomBackImage;

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

	public MessageType CurrentMessageType => _currentMessageType;

	public SentenceSelectionButtonsUI SelectionButtonsUI => _selectionButtonsUI;

	public Observable<Unit> OnClickButtonSkip => _onClickButtonSkip;

	public Observable<Unit> OnClickButtonAuto => _onClickButtonAuto;

	public CancellationToken CancellationTokenOnDestroy => base.destroyCancellationToken;

	FadeController IStorySystemUI.FadeController => _storyFadeController;

	public void Setup()
	{
		_storyFadeController.ImmediateFadeIn();
		_transparentButton.onClick.AddListener(NextClick);
		ObservableSubscribeExtensions.Subscribe(_autoButton.OnClickAsObservable(), delegate
		{
			_onClickButtonAuto.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_autoButton.OnPointerEnterAsObservable(), delegate
		{
			_auteText.color = _buttonActiveColor;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_autoButton.OnPointerExitAsObservable(), delegate
		{
			if (!SaveDataManager.Instance.StoryData.IsUseAuto.Value)
			{
				_auteText.color = _buttonDeactiveColor;
			}
		}).AddTo(this);
		if (SaveDataManager.Instance.StoryData.IsUseAuto.Value)
		{
			_auteText.color = (SaveDataManager.Instance.StoryData.IsUseAuto.Value ? _buttonActiveColor : _buttonDeactiveColor);
		}
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.StoryData.IsUseAuto, delegate
		{
			_auteText.color = (SaveDataManager.Instance.StoryData.IsUseAuto.Value ? _buttonActiveColor : _buttonDeactiveColor);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_skipButton.OnClickAsObservable(), delegate
		{
			_onClickButtonSkip.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_skipButton.OnPointerEnterAsObservable(), delegate
		{
			_skipText.color = _buttonActiveColor;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_skipButton.OnPointerExitAsObservable(), delegate
		{
			_skipText.color = _buttonDeactiveColor;
		}).AddTo(this);
		DeactivateSkipButton();
		DeactivateAutoButton();
		_transparentButton.gameObject.SetActive(value: false);
		_rectTransform = _normalTextParent.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		_normalTextCanvasGroup.alpha = 0f;
		_heroineNameObject.SetActive(value: false);
		_currentMessageType = MessageType.Normal;
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
			_currentMessageType = MessageType.MainStory;
			if (novelID == "main_01")
			{
				_heroineNameTextLocalizationBehavior.GetComponent<TextMeshProUGUI>().text = "？？？？";
			}
			else
			{
				_heroineNameTextLocalizationBehavior.Set("heroine_name");
			}
			_heroineNameObject.SetActive(value: true);
			_bottomBackImage.gameObject.SetActive(value: true);
			RectTransform component = _bottomBackImage.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			sizeDelta.y = 308f;
			component.sizeDelta = sizeDelta;
		}
		else
		{
			_currentMessageType = MessageType.Normal;
		}
	}

	public void ChangeMessageType(MessageType type)
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
		_mainStoryTextParent.SetActive(_currentMessageType == MessageType.MainStory);
		_normalTextParent.SetActive(_currentMessageType == MessageType.Normal);
		switch (_currentMessageType)
		{
		case MessageType.MainStory:
			_mainStoryTextMessage.StartText(string.Empty);
			break;
		case MessageType.Normal:
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
		case MessageType.MainStory:
		{
			_mainStoryTextMessage.ClearOnTextShowedCallback();
			_mainStoryTextMessage.StartText(string.Empty);
			_transparentButton.gameObject.SetActive(value: false);
			_heroineNameObject.SetActive(value: false);
			DeactivateSkipButton();
			RectTransform component = _bottomBackImage.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			sizeDelta.y = 180f;
			component.sizeDelta = sizeDelta;
			break;
		}
		case MessageType.Normal:
		{
			_normalTextMessage.ClearOnTextShowedCallback();
			Action onEndAction = delegate
			{
				_normalTextParent.SetActive(value: false);
				_transparentButton.gameObject.SetActive(value: false);
			};
			DeactivateNormalText(onEndAction);
			break;
		}
		}
		_currentMessageType = MessageType.Normal;
	}

	public void ActivateBottomBackImage()
	{
		_bottomBackImage.gameObject.SetActive(value: true);
	}

	public void DeactivateBottomBackImage()
	{
		_bottomBackImage.gameObject.SetActive(value: false);
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
		_autoButton.gameObject.SetActive(value: true);
	}

	public void DeactivateAutoButton()
	{
		_autoButton.gameObject.SetActive(value: false);
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
			MessageType.MainStory => _mainStoryTextMessage, 
			MessageType.Normal => _normalTextMessage, 
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
