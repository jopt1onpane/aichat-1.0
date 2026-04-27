using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class TutorialViewForMobile : MonoBehaviour, ITutorialView
{
	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private Button _closeTutorialButton;

	[SerializeField]
	private TutorialHelpView _tutorialHelpView;

	[SerializeField]
	private GameObject _rayCastBlockObject;

	[SerializeField]
	private RectTransform _rootRectTransform;

	[SerializeField]
	private TutorialFocusUIForMobile _pomodoroFocusUI;

	[SerializeField]
	private TutorialFocusUIForMobile _noteFocusUI;

	[SerializeField]
	private TutorialFocusUIForMobile _todoFocusUI;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private GameObject _currentPageObject;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _initPosY;

	private bool _isClose;

	public void Setup()
	{
		_tutorialHelpView.Setup();
		_rayCastBlockObject.SetActive(value: false);
		_initPosY = _rootRectTransform.anchoredPosition.y;
		_canvasGroup.alpha = 0f;
		ObservableSubscribeExtensions.Subscribe(_tutorialHelpView.PageView.OnClickBackButton, delegate
		{
			if (!_isClose && _tutorialService.CurrentPageType.CurrentValue != TutorialService.TutorialPageType.ScreenUI)
			{
				_tutorialService.ToPreviousPage();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialHelpView.PageView.OnClickLastButton, delegate
		{
			if (!_isClose)
			{
				_isClose = true;
				_tutorialService.CloseTutorial();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_closeTutorialButton.OnClickAsObservable(), delegate
		{
			if (!_isClose)
			{
				_isClose = true;
				_tutorialService.CloseTutorial();
				_systemSeService.PlayCancel();
			}
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
			if (_pomodoroFocusUI != null)
			{
				_pomodoroFocusUI.Activate(TutorialFocusUI.FocusType.Pomodoro);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusPomodoro, delegate
		{
			if (_pomodoroFocusUI != null)
			{
				_pomodoroFocusUI.Deactivate();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnStartFocusNote, delegate
		{
			if (_noteFocusUI != null)
			{
				_noteFocusUI.Activate(TutorialFocusUI.FocusType.Note);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusNote, delegate
		{
			if (_noteFocusUI != null)
			{
				_noteFocusUI.Deactivate();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnStartFocusTodo, delegate
		{
			if (_todoFocusUI != null)
			{
				_todoFocusUI.Activate(TutorialFocusUI.FocusType.Todo);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnEndFocusTodo, delegate
		{
			if (_todoFocusUI != null)
			{
				_todoFocusUI.Deactivate();
			}
		}).AddTo(this);
	}

	public void OpenTutorial(TutorialService.TutorialPageType pageType)
	{
		if ((uint)pageType > 1u && pageType != TutorialService.TutorialPageType.Count)
		{
			if (!base.gameObject.activeSelf || _canvasGroup.alpha != 1f)
			{
				_tutorialHelpView.PrepareTutorialHelp(pageType);
				Activate();
			}
			else
			{
				_tutorialHelpView.MovePage(pageType);
			}
		}
	}

	private void Activate()
	{
		_isClose = false;
		base.gameObject.SetActive(value: true);
		_rayCastBlockObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_rootRectTransform.anchoredPosition = new Vector3(_rootRectTransform.anchoredPosition.x, _initPosY + -30f, 0f);
		_moveTween = _rootRectTransform.DOAnchorPosY(_initPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_rayCastBlockObject.SetActive(value: false);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rootRectTransform.DOAnchorPosY(_initPosY + -30f, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}
}
