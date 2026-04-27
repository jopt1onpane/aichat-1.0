using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Bulbul.Mobile;

public class WallPaperManagerForMobile : MonoBehaviour
{
	[Serializable]
	public class DeactivationObj
	{
		public GameObject Obj;

		public CanvasGroup CanvasGroup;
	}

	public enum WallPaperState
	{
		UnUsed,
		Vertical,
		Horizontal
	}

	private struct RequestVerticalWallPaperData
	{
		public bool IsRequested;

		public bool IsImmediate;
	}

	private static readonly int _blackImageShowFrame = 3;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private IRoomGameSceneState _roomGameSceneState;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private RoomCameraManager _roomCameraManager;

	[Inject]
	private IUIShowManager _showManager;

	[Inject]
	private IRoomGameLongTalkNotice _roomGameLongTalkNotice;

	[Inject]
	private FacilityClickHeroine _facilityClickHeroine;

	[SerializeField]
	private WallPaperViewForMobile _wallPaperView;

	[SerializeField]
	private DeactivationObj[] _deactivationObjs;

	[SerializeField]
	[Header("機能群のアクティブチェック用")]
	private ObjectsActiveChecker _facilitiesWindowChecker;

	private ScreenOrientation _curScreenOrientation;

	private WallPaperState _state;

	private bool _isLockScenarioNow;

	private bool _isEndGame;

	private float _currentAutoTransitionSec = 100f;

	private bool _disableAutoTransition = true;

	private float _timer;

	private Subject<bool> _onChangedState = new Subject<bool>();

	private CancellationTokenSource _changedWallPaperModeCancellationToken;

	private EventSystem _eventSystem;

	private RequestVerticalWallPaperData _requestVerticalWallpaper;

	private CancellationTokenSource _cancellationTokenSource;

	public WallPaperState State => _state;

	public Observable<bool> OnChangedState => _onChangedState;

	public bool EnableAutoTransition => !_disableAutoTransition;

	public void RequestVerticalWallpaper(bool isImmediate = true)
	{
		_requestVerticalWallpaper.IsRequested = true;
		_requestVerticalWallpaper.IsImmediate = isImmediate;
	}

	private void LateUpdate()
	{
		if (_roomGameSceneState.CurrentMainState == RoomGameManager.MainState.ExitGame0 || _roomGameSceneState.CurrentMainState == RoomGameManager.MainState.Release)
		{
			_isEndGame = true;
		}
		UpdateWallPaper();
	}

	private void OnApplicationFocus(bool focus)
	{
		_timer = 0f;
	}

	private void OnDestroy()
	{
		CancelChangeWallPaper();
		Cancel();
	}

	private void UpdateWallPaper()
	{
		bool isRequested = _requestVerticalWallpaper.IsRequested;
		_requestVerticalWallpaper.IsRequested = false;
		if (_isLockScenarioNow || CheckGameStartDirection() || _isEndGame || ScreenOrientationManagerForMobile.Instance.IsRotateLock)
		{
			_timer = 0f;
			if (_state != WallPaperState.UnUsed)
			{
				ForceChangedUnUsed();
			}
			_curScreenOrientation = Screen.orientation;
			return;
		}
		ScreenOrientation orientation = Screen.orientation;
		switch (_state)
		{
		case WallPaperState.UnUsed:
		{
			bool isImmediate = false;
			if (isRequested)
			{
				_timer = _currentAutoTransitionSec;
				isImmediate = _requestVerticalWallpaper.IsImmediate;
			}
			if (UpdateTimer(!isRequested))
			{
				ChangeUsedWallPaper(orientation, isPrevUnUsed: true, isImmediate).Forget();
			}
			else if (CheckHorizontal(orientation))
			{
				if (!_showManager.IsShowUI)
				{
					_showManager.AllUIActivate(isUseDoComplete: true);
				}
				ChangeUsedWallPaper(orientation, isPrevUnUsed: true).Forget();
			}
			else
			{
				_curScreenOrientation = orientation;
			}
			break;
		}
		case WallPaperState.Horizontal:
			if (CheckVertical(orientation))
			{
				ChangeUsedWallPaper(orientation, isPrevUnUsed: false).Forget();
			}
			break;
		case WallPaperState.Vertical:
			if (CheckClickScreen(isExcludeHeroineClick: true) || CheckDisableTimer())
			{
				ChangeUnUsedWallPaper(orientation).Forget();
			}
			else if (CheckHorizontal(orientation))
			{
				ChangeUsedWallPaper(orientation, isPrevUnUsed: false).Forget();
			}
			break;
		}
	}

	private bool CheckHeroineMainStoryState()
	{
		if (_heroineService.GetCurrentAIState() == HeroineAI.ActionStateType.MainStory)
		{
			return true;
		}
		return false;
	}

	private bool CheckGameStartDirection()
	{
		if (_heroineService.GetCurrentAIState() == HeroineAI.ActionStateType.GameStartDirection)
		{
			return true;
		}
		return false;
	}

	private bool CheckVertical(ScreenOrientation orientation)
	{
		if ((uint)(orientation - 1) > 1u)
		{
			_ = orientation - 3;
			_ = 1;
			return false;
		}
		return true;
	}

	private bool CheckHorizontal(ScreenOrientation orientation)
	{
		switch (orientation)
		{
		case ScreenOrientation.LandscapeLeft:
		case ScreenOrientation.LandscapeRight:
			return true;
		case ScreenOrientation.Portrait:
		case ScreenOrientation.PortraitUpsideDown:
			return false;
		default:
			return false;
		}
	}

	private void ForceChangedUnUsed()
	{
		CancelChangeWallPaper();
		_state = WallPaperState.UnUsed;
		_onChangedState.OnNext(value: false);
		_wallPaperView.Deactivate();
		ActivateMainUI(isImmediate: true).Forget();
		_wallPaperView.SetActiveBlackImage(active: false);
		_timer = 0f;
		HideBannerAd();
	}

	private void CancelChangeWallPaper()
	{
		if (_changedWallPaperModeCancellationToken != null)
		{
			_changedWallPaperModeCancellationToken.Cancel();
			_changedWallPaperModeCancellationToken.Dispose();
			_changedWallPaperModeCancellationToken = null;
		}
		_wallPaperView.SetActiveBlackImage(active: false);
	}

	private async UniTask ChangeUnUsedWallPaper(ScreenOrientation orientation)
	{
		CancelChangeWallPaper();
		_changedWallPaperModeCancellationToken = new CancellationTokenSource();
		_state = WallPaperState.UnUsed;
		_onChangedState.OnNext(value: false);
		CheckVertical(orientation);
		bool num = _curScreenOrientation != orientation;
		_timer = 0f;
		HideBannerAd();
		_curScreenOrientation = orientation;
		if (num)
		{
			_wallPaperView.SetActiveBlackImage(active: true);
			await UniTask.DelayFrame(_blackImageShowFrame, PlayerLoopTiming.Update, _changedWallPaperModeCancellationToken.Token);
			_wallPaperView.SetActiveBlackImage(active: false);
		}
		ActivateMainUI().Forget();
		_wallPaperView.DeactivateAsync().Forget();
	}

	private async UniTask ChangeUsedWallPaper(ScreenOrientation orientation, bool isPrevUnUsed, bool isImmediate = false)
	{
	}

	private bool UpdateTimer(bool isCheckClicked = true)
	{
		if ((isCheckClicked && CheckClickScreen()) || CheckDisableTimer())
		{
			_timer = 0f;
			return false;
		}
		_timer += Time.deltaTime;
		return _timer >= _currentAutoTransitionSec;
	}

	private bool CheckClickScreen(bool isExcludeHeroineClick = false)
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (isExcludeHeroineClick && _facilityClickHeroine.IsClickedHeroineCurrentFrame)
			{
				return false;
			}
			if (InputController.Instance.CurrentFrameEventSystemRaycastResult.Count <= 0)
			{
				return true;
			}
			return !_wallPaperView.CheckBlock(InputController.Instance.CurrentFrameEventSystemRaycastHitObject);
		}
		return false;
	}

	private bool CheckDisableTimer()
	{
		if (!_disableAutoTransition && !_facilitiesWindowChecker.CheckActive() && _tutorialService.IsCloseTutorialPage() && _showManager.IsShowUI)
		{
			return CheckHeroineMainStoryState();
		}
		return true;
	}

	private void Cancel()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
	}

	private async UniTask ActivateMainUI(bool isImmediate = false)
	{
		Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		Sequence sequence = DOTween.Sequence();
		DeactivationObj[] deactivationObjs = _deactivationObjs;
		foreach (DeactivationObj deactivationObj in deactivationObjs)
		{
			deactivationObj.CanvasGroup.blocksRaycasts = false;
			sequence.Join(deactivationObj.CanvasGroup.DOFade(1f, 0.2f));
		}
		sequence.OnComplete(delegate
		{
			DeactivationObj[] deactivationObjs2 = _deactivationObjs;
			for (int j = 0; j < deactivationObjs2.Length; j++)
			{
				deactivationObjs2[j].CanvasGroup.blocksRaycasts = true;
			}
		});
		if (isImmediate)
		{
			sequence.Complete();
		}
		else
		{
			await sequence.AwaitForComplete(TweenCancelBehaviour.Kill, _cancellationTokenSource.Token);
		}
	}

	private async UniTask DeactivateMainUI(bool isImmediate = false)
	{
		Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		Sequence sequence = DOTween.Sequence();
		DeactivationObj[] deactivationObjs = _deactivationObjs;
		foreach (DeactivationObj deactivationObj in deactivationObjs)
		{
			deactivationObj.CanvasGroup.blocksRaycasts = false;
			sequence.Join(deactivationObj.CanvasGroup.DOFade(0f, 0.2f));
		}
		if (isImmediate)
		{
			sequence.Complete();
		}
		else
		{
			await sequence.AwaitForComplete(TweenCancelBehaviour.Kill, _cancellationTokenSource.Token);
		}
	}

	private void CreateOrShowBannerAd(bool isVertical)
	{
		if (AdmobCtrl.GetInstance().IsCanRequestAd())
		{
			if (isVertical)
			{
				AdmobCtrl.GetInstance().CreateOrShowPortraitBannerAd();
				AdmobCtrl.GetInstance().HideLandscapeBannerAd();
			}
			else
			{
				AdmobCtrl.GetInstance().CreateOrShowLandscapeBannerAd();
				AdmobCtrl.GetInstance().HidePortraitBannerAd();
			}
		}
	}

	private void HideBannerAd()
	{
		if (AdmobCtrl.GetInstance().IsCanRequestAd())
		{
			AdmobCtrl.GetInstance().HideLandscapeBannerAd();
			AdmobCtrl.GetInstance().HidePortraitBannerAd();
		}
	}

	private void DestroyBannerAd()
	{
		if (AdmobCtrl.GetInstance().IsCanRequestAd())
		{
			AdmobCtrl.GetInstance().DestroyLandscapeBannerAd();
			AdmobCtrl.GetInstance().DestroyPortraitBannerAd();
		}
	}
}
