using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class UIShowManagerForMobile : MonoBehaviour, IUIShowManager
{
	[Inject]
	private IUICanvasProvider _uiCanvasProvider;

	[Inject]
	private IAllUIDeactivateInteractUIProvider _allUIDeactivateInteractUIProvider;

	[Inject]
	private ITutorialView _tutorialView;

	[SerializeField]
	private GameObject[] _deactivationUIObjectsForStoryPlay;

	[SerializeField]
	private CanvasGroup _mainSceneUICanvasGroup;

	[SerializeField]
	private GameObject _storySelectUIObj;

	[SerializeField]
	private CanvasGroup _callIconCanvasGroup;

	private bool _isShowUI;

	private Tween _uiParentFadeTween;

	private Tween _mainSceneUICanvasGroupFadeTween;

	private DebugService _debugService;

	private bool _tempStorySelectUIObjActiveState;

	private float _tempMainSceneUICanvasGroupAlpha = 1f;

	private bool _isNeedEndTutorial;

	public bool IsShowUI => _isShowUI;

	public void Setup()
	{
		if (_uiCanvasProvider == null)
		{
			_uiCanvasProvider = RoomLifetimeScope.Resolve<IUICanvasProvider>();
		}
		if (_allUIDeactivateInteractUIProvider == null)
		{
			_allUIDeactivateInteractUIProvider = RoomLifetimeScope.Resolve<IAllUIDeactivateInteractUIProvider>();
		}
		if (_tutorialView == null)
		{
			_tutorialView = RoomLifetimeScope.Resolve<ITutorialView>();
		}
		_tutorialView.Setup();
		if ((object)_debugService == null)
		{
			_debugService = RoomLifetimeScope.Resolve<DebugService>();
		}
	}

	public async UniTask AllUIActivate(bool isUseDoComplete = false)
	{
		if (!(_debugService != null) || !_debugService.IsUseManageShowUI)
		{
			_isShowUI = true;
			_allUIDeactivateInteractUIProvider.AllDeactivateInteractUI.DeactivateAllUI(isUseDoComplete: true);
			await ActivateUIParent(isUseDoComplete);
		}
	}

	public async UniTask AllUIDeactivate(bool isUseDoComplete = false)
	{
		if (!(_debugService != null) || !_debugService.IsUseManageShowUI)
		{
			_isShowUI = false;
			_tutorialView.Deactivate();
			UIShowManagerForMobile uIShowManagerForMobile = this;
			bool isUseDoComplete2 = isUseDoComplete;
			await uIShowManagerForMobile.DeactivateUIParent(default(CancellationToken), isUseDoComplete2);
		}
	}

	public async UniTask TutorialOtherUIActivate()
	{
		_isShowUI = true;
		_allUIDeactivateInteractUIProvider.AllDeactivateInteractUI.DeactivateAllUI(isUseDoComplete: true);
		await ActivateUIParent();
	}

	public async UniTask TutorialOtherUIDeactivate(CancellationToken token)
	{
		_isShowUI = false;
		await DeactivateUIParent(token);
	}

	private async UniTask ActivateUIParent(bool isUseDoComplete = false)
	{
		_uiCanvasProvider.UIParent.SetActive(value: true);
		_uiParentFadeTween?.Kill();
		if (isUseDoComplete)
		{
			_uiCanvasProvider.UIParentCanvasGroup.alpha = 1f;
			return;
		}
		_uiParentFadeTween = _uiCanvasProvider.UIParentCanvasGroup.DOFade(1f, 0.5f);
		await _uiParentFadeTween;
	}

	private async UniTask DeactivateUIParent(CancellationToken token = default(CancellationToken), bool isUseDoComplete = false)
	{
		_uiParentFadeTween?.Kill();
		if (isUseDoComplete)
		{
			_uiCanvasProvider.UIParent.SetActive(value: false);
			_uiCanvasProvider.UIParentCanvasGroup.alpha = 0f;
			return;
		}
		_uiParentFadeTween = _uiCanvasProvider.UIParentCanvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_uiCanvasProvider.UIParent.SetActive(value: false);
		});
		await _uiParentFadeTween.ToUniTask(TweenCancelBehaviour.Kill, token);
	}

	public async UniTask ActivateMainUI(bool isUseDoComplete = false)
	{
		_mainSceneUICanvasGroupFadeTween?.Kill();
		if (isUseDoComplete)
		{
			_mainSceneUICanvasGroup.alpha = 1f;
			_mainSceneUICanvasGroup.blocksRaycasts = true;
			return;
		}
		_mainSceneUICanvasGroupFadeTween = _mainSceneUICanvasGroup.DOFade(1f, 0.3f).OnComplete(delegate
		{
			_mainSceneUICanvasGroup.blocksRaycasts = true;
		});
		await _mainSceneUICanvasGroupFadeTween;
	}

	public async UniTask DeactivateMainUI(CancellationToken token = default(CancellationToken), bool isUseDoComplete = false)
	{
		_mainSceneUICanvasGroupFadeTween?.Kill();
		_mainSceneUICanvasGroup.blocksRaycasts = false;
		if (isUseDoComplete)
		{
			_mainSceneUICanvasGroup.alpha = 0f;
			return;
		}
		_mainSceneUICanvasGroupFadeTween = _mainSceneUICanvasGroup.DOFade(0f, 0.3f);
		await _mainSceneUICanvasGroupFadeTween.ToUniTask(TweenCancelBehaviour.Kill, token);
	}

	public void AdjustUIForPlayScenario()
	{
		GameObject[] deactivationUIObjectsForStoryPlay = _deactivationUIObjectsForStoryPlay;
		for (int i = 0; i < deactivationUIObjectsForStoryPlay.Length; i++)
		{
			deactivationUIObjectsForStoryPlay[i].SetActive(value: false);
		}
	}

	public void AdjustUIForEndScenario()
	{
		if (_isNeedEndTutorial)
		{
			AdjustUIForEndTutorialScenario();
		}
		GameObject[] deactivationUIObjectsForStoryPlay = _deactivationUIObjectsForStoryPlay;
		for (int i = 0; i < deactivationUIObjectsForStoryPlay.Length; i++)
		{
			deactivationUIObjectsForStoryPlay[i].SetActive(value: true);
		}
	}

	public void AdjustUIForStartTutorialScenario()
	{
		_tempStorySelectUIObjActiveState = _storySelectUIObj.activeSelf;
		_tempMainSceneUICanvasGroupAlpha = _mainSceneUICanvasGroup.alpha;
		_storySelectUIObj.SetActive(value: false);
		_mainSceneUICanvasGroup.alpha = 1f;
		AllUIDeactivate().Forget();
		_isNeedEndTutorial = true;
	}

	public void AdjustUIForEndTutorialScenario()
	{
		_storySelectUIObj.SetActive(_tempStorySelectUIObjActiveState);
		_mainSceneUICanvasGroup.alpha = _tempMainSceneUICanvasGroupAlpha;
		_isNeedEndTutorial = false;
	}

	public void SetCallIconActive(bool active)
	{
		_callIconCanvasGroup.alpha = (active ? 1f : 0f);
		_callIconCanvasGroup.blocksRaycasts = active;
	}
}
