using Bulbul;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TutorialView : MonoBehaviour, ITutorialView
{
	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	private Button _previousPageButton;

	[SerializeField]
	private Button _nextPageButton;

	[SerializeField]
	private Button _finishTutorialButton;

	[SerializeField]
	private Button _closeTutorialButton;

	[SerializeField]
	[Header("体験版")]
	private GameObject _gameDemoObject;

	[SerializeField]
	[Header("スクリーン: 親オブジェクト")]
	private GameObject _screenUIObject;

	[SerializeField]
	[Header("スクリーン: Image")]
	private Image _screenImage;

	[SerializeField]
	[Header("スクリーン: JP")]
	private Sprite _screenSpriteJP;

	[SerializeField]
	[Header("スクリーン: EN")]
	private Sprite _screenSpriteEN;

	[SerializeField]
	[Header("スクリーン: SC")]
	private Sprite _screenSpriteSC;

	[SerializeField]
	[Header("スクリーン: TC")]
	private Sprite _screenSpriteTC;

	[SerializeField]
	[Header("スクリーン: PR")]
	private Sprite _screenSpritePR;

	[SerializeField]
	[Header("スクリーン: KR")]
	private Sprite _screenSpriteKR;

	[SerializeField]
	[Header("スクリーン: RU")]
	private Sprite _screenSpriteRU;

	[SerializeField]
	[Header("ポモドーロタイマー: 親オブジェクト")]
	private GameObject _pomodoroTimerObject;

	[SerializeField]
	[Header("ポモドーロタイマー: Image")]
	private Image _pomodoroTimerImage;

	[SerializeField]
	[Header("ポモドーロタイマー: JP")]
	private Sprite _pomodoroTimerSpriteJP;

	[SerializeField]
	[Header("ポモドーロタイマー: EN")]
	private Sprite _pomodoroTimerSpriteEN;

	[SerializeField]
	[Header("ポモドーロタイマー: SC")]
	private Sprite _pomodoroTimerSpriteSC;

	[SerializeField]
	[Header("ポモドーロタイマー: TC")]
	private Sprite _pomodoroTimerSpriteTC;

	[SerializeField]
	[Header("ポモドーロタイマー: PR")]
	private Sprite _pomodoroTimerSpritePR;

	[SerializeField]
	[Header("ポモドーロタイマー: KR")]
	private Sprite _pomodoroTimerSpriteKR;

	[SerializeField]
	[Header("ポモドーロタイマー: RU")]
	private Sprite _pomodoroTimerSpriteRU;

	[SerializeField]
	[Header("レベルとストーリー: 親オブジェクト")]
	private GameObject _levelAndStoryObject;

	[SerializeField]
	[Header("レベルとストーリー: Image")]
	private Image _levelAndStoryImage;

	[SerializeField]
	[Header("レベルとストーリー: JP")]
	private Sprite _levelAndStorySpriteJP;

	[SerializeField]
	[Header("レベルとストーリー: EN")]
	private Sprite _levelAndStorySpriteEN;

	[SerializeField]
	[Header("レベルとストーリー: SC")]
	private Sprite _levelAndStorySpriteSC;

	[SerializeField]
	[Header("レベルとストーリー: TC")]
	private Sprite _levelAndStorySpriteTC;

	[SerializeField]
	[Header("レベルとストーリー: PR")]
	private Sprite _levelAndStorySpritePR;

	[SerializeField]
	[Header("レベルとストーリー: KR")]
	private Sprite _levelAndStorySpriteKR;

	[SerializeField]
	[Header("レベルとストーリー: RU")]
	private Sprite _levelAndStorySpriteRU;

	[SerializeField]
	[Header("環境: 親オブジェクト")]
	private GameObject _newEnvironmentObject;

	[SerializeField]
	[Header("環境: Image")]
	private Image _newEnvironmentImage;

	[SerializeField]
	[Header("環境: JP")]
	private Sprite _newEnvironmentSpriteJP;

	[SerializeField]
	[Header("環境: EN")]
	private Sprite _newEnvironmentSpriteEN;

	[SerializeField]
	[Header("環境: SC")]
	private Sprite _newEnvironmentSpriteSC;

	[SerializeField]
	[Header("環境: TC")]
	private Sprite _newEnvironmentSpriteTC;

	[SerializeField]
	[Header("環境: PR")]
	private Sprite _newEnvironmentSpritePR;

	[SerializeField]
	[Header("環境: KR")]
	private Sprite _newEnvironmentSpriteKR;

	[SerializeField]
	[Header("環境: RU")]
	private Sprite _newEnvironmentSpriteRU;

	[SerializeField]
	private GameObject _rayCastBlockObject;

	[SerializeField]
	private TutorialFocusUI _pomodoroFocusUI;

	[SerializeField]
	private TutorialFocusUI _noteFocusUI;

	[SerializeField]
	private TutorialFocusUI _todoFocusUI;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private GameObject _currentPageObject;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _initPosY;

	public void Setup()
	{
		_rayCastBlockObject.SetActive(value: false);
		_rectTransform = base.transform as RectTransform;
		_initPosY = _rectTransform.anchoredPosition.y;
		_canvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(_previousPageButton.OnClickAsObservable(), delegate
		{
			_tutorialService.ToPreviousPage();
			_systemSeService.PlayCancel();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_nextPageButton.OnClickAsObservable(), delegate
		{
			_tutorialService.ToNextPage();
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_closeTutorialButton.OnClickAsObservable(), delegate
		{
			_tutorialService.CloseTutorial();
			_systemSeService.PlayCancel();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_finishTutorialButton.OnClickAsObservable(), delegate
		{
			_tutorialService.CloseTutorial();
			_systemSeService.PlayClick();
		}).AddTo(this);
		_tutorialService.CurrentPageType.Subscribe(delegate(TutorialService.TutorialPageType pageType)
		{
			OpenTutorial(pageType);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnClose, delegate
		{
			Deactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnStartFocusPomodoro, delegate
		{
			_pomodoroFocusUI.Activate(TutorialFocusUI.FocusType.Pomodoro);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusPomodoro, delegate
		{
			_pomodoroFocusUI.Deactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnStartFocusNote, delegate
		{
			_noteFocusUI.Activate(TutorialFocusUI.FocusType.Note);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusNote, delegate
		{
			_noteFocusUI.Deactivate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnStartFocusTodo, delegate
		{
			_todoFocusUI.Activate(TutorialFocusUI.FocusType.Todo);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusTodo, delegate
		{
			_todoFocusUI.Deactivate();
		}).AddTo(this);
	}

	public void OpenTutorial(TutorialService.TutorialPageType pageType)
	{
		if (pageType != TutorialService.TutorialPageType.None)
		{
			if (!base.gameObject.activeSelf || _canvasGroup.alpha != 1f)
			{
				Activate();
			}
			_currentPageObject?.SetActive(value: false);
			switch (pageType)
			{
			case TutorialService.TutorialPageType.GameDemo:
				_currentPageObject = _gameDemoObject;
				break;
			case TutorialService.TutorialPageType.ScreenUI:
			{
				_currentPageObject = _screenUIObject;
				Image newEnvironmentImage = _screenImage;
				newEnvironmentImage.sprite = _languageSupplier.Language.CurrentValue switch
				{
					GameLanguageType.Japanese => _screenSpriteJP, 
					GameLanguageType.English => _screenSpriteEN, 
					GameLanguageType.ChineseSimplified => _screenSpriteSC, 
					GameLanguageType.ChineseTraditional => _screenSpriteTC, 
					GameLanguageType.Portuguese => _screenSpritePR, 
					GameLanguageType.Korean => _screenSpriteKR, 
					GameLanguageType.Russian => _screenSpriteRU, 
					_ => null, 
				};
				break;
			}
			case TutorialService.TutorialPageType.PomodoroTimer:
			{
				_currentPageObject = _pomodoroTimerObject;
				Image newEnvironmentImage = _pomodoroTimerImage;
				newEnvironmentImage.sprite = _languageSupplier.Language.CurrentValue switch
				{
					GameLanguageType.Japanese => _pomodoroTimerSpriteJP, 
					GameLanguageType.English => _pomodoroTimerSpriteEN, 
					GameLanguageType.ChineseSimplified => _pomodoroTimerSpriteSC, 
					GameLanguageType.ChineseTraditional => _pomodoroTimerSpriteTC, 
					GameLanguageType.Portuguese => _pomodoroTimerSpritePR, 
					GameLanguageType.Korean => _pomodoroTimerSpriteKR, 
					GameLanguageType.Russian => _pomodoroTimerSpriteRU, 
					_ => null, 
				};
				break;
			}
			case TutorialService.TutorialPageType.LevelAndStory:
			{
				_currentPageObject = _levelAndStoryObject;
				Image newEnvironmentImage = _levelAndStoryImage;
				newEnvironmentImage.sprite = _languageSupplier.Language.CurrentValue switch
				{
					GameLanguageType.Japanese => _levelAndStorySpriteJP, 
					GameLanguageType.English => _levelAndStorySpriteEN, 
					GameLanguageType.ChineseSimplified => _levelAndStorySpriteSC, 
					GameLanguageType.ChineseTraditional => _levelAndStorySpriteTC, 
					GameLanguageType.Portuguese => _levelAndStorySpritePR, 
					GameLanguageType.Korean => _levelAndStorySpriteKR, 
					GameLanguageType.Russian => _levelAndStorySpriteRU, 
					_ => null, 
				};
				break;
			}
			case TutorialService.TutorialPageType.NewEnviroment:
			{
				_currentPageObject = _newEnvironmentObject;
				Image newEnvironmentImage = _newEnvironmentImage;
				newEnvironmentImage.sprite = _languageSupplier.Language.CurrentValue switch
				{
					GameLanguageType.Japanese => _newEnvironmentSpriteJP, 
					GameLanguageType.English => _newEnvironmentSpriteEN, 
					GameLanguageType.ChineseSimplified => _newEnvironmentSpriteSC, 
					GameLanguageType.ChineseTraditional => _newEnvironmentSpriteTC, 
					GameLanguageType.Portuguese => _newEnvironmentSpritePR, 
					GameLanguageType.Korean => _newEnvironmentSpriteKR, 
					GameLanguageType.Russian => _newEnvironmentSpriteRU, 
					_ => null, 
				};
				break;
			}
			}
			_currentPageObject.SetActive(value: true);
			TutorialService.TutorialPageType tutorialPageType = TutorialService.TutorialPageType.None;
			TutorialService.TutorialPageType tutorialPageType2 = TutorialService.TutorialPageType.Count;
			switch (_tutorialService.CurrentPageOpenType)
			{
			case TutorialService.TutorialPageOpenType.OnlyGameDemo:
				tutorialPageType = TutorialService.TutorialPageType.GameDemo;
				tutorialPageType2 = TutorialService.TutorialPageType.GameDemo;
				break;
			case TutorialService.TutorialPageOpenType.HelpAll:
				tutorialPageType = TutorialService.TutorialPageType.ScreenUI;
				tutorialPageType2 = TutorialService.TutorialPageType.Count;
				break;
			case TutorialService.TutorialPageOpenType.ALL:
				tutorialPageType = TutorialService.TutorialPageType.ScreenUI;
				tutorialPageType2 = TutorialService.TutorialPageType.Count;
				break;
			}
			if (pageType > tutorialPageType)
			{
				ShowPreviousPageButton();
			}
			else
			{
				HidePreviousPageButton();
			}
			if (pageType + 1 == tutorialPageType2 || pageType == tutorialPageType2)
			{
				ShowFinishTutorialButton();
				HideNextPageButton();
			}
			else
			{
				ShowNextPageButton();
				HideFinishTutorialButton();
			}
		}
	}

	private void ShowNextPageButton()
	{
		_nextPageButton.gameObject.SetActive(value: true);
	}

	private void HideNextPageButton()
	{
		_nextPageButton.gameObject.SetActive(value: false);
		_nextPageButton.GetComponent<InteractableUI>().DeactivateAllUI(isUseDoComplete: true);
	}

	private void ShowPreviousPageButton()
	{
		_previousPageButton.gameObject.SetActive(value: true);
	}

	private void HidePreviousPageButton()
	{
		_previousPageButton.gameObject.SetActive(value: false);
		_previousPageButton.GetComponent<InteractableUI>().DeactivateAllUI(isUseDoComplete: true);
	}

	private void ShowFinishTutorialButton()
	{
		_finishTutorialButton.gameObject.SetActive(value: true);
	}

	private void HideFinishTutorialButton()
	{
		_finishTutorialButton.gameObject.SetActive(value: false);
		_finishTutorialButton.GetComponent<InteractableUI>().DeactivateAllUI(isUseDoComplete: true);
	}

	private void Activate()
	{
		base.gameObject.SetActive(value: true);
		_rayCastBlockObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_rectTransform.anchoredPosition = new Vector3(_rectTransform.anchoredPosition.x, _initPosY + -8f, 0f);
		_moveTween = _rectTransform.DOAnchorPosY(_initPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_rayCastBlockObject.SetActive(value: false);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_initPosY + -8f, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
