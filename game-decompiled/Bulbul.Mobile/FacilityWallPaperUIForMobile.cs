using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityWallPaperUIForMobile : MonoBehaviour
{
	[Inject]
	private MusicService _musicService;

	[Inject]
	private PlayerLevelService _playerLevelService;

	[Inject]
	private IPlayerLevelUIService _playerLevelUIService;

	[Inject]
	private PomodoroService _pomodoroService;

	[Inject]
	private SlideFadeAnnounceDirection _slideFadeAnnounceDirection;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private WallPaperViewForMobile _view;

	[SerializeField]
	private WallPaperManagerForMobile _manager;

	[SerializeField]
	[Header("表示非表示チェック用\u3000Alphaで表示制御しているようなので")]
	private CanvasGroup _playerPointForAlphaCheck;

	[SerializeField]
	[Header("壁紙中にポモドーロ付近を押した場合に縦画面で操作してね的な文言を出す")]
	private Button _wallpaperPomodoroBlockButton;

	[Header("壁紙モードで出さなくてはいけないUIが上位で非表示になっていることがあるのでignoreParentGroupsで無視させる ")]
	[SerializeField]
	[Header("表示制御用\u3000壁紙では動作中以外表示しない為")]
	private CanvasGroup _playerLvUICanvasGroup;

	[SerializeField]
	[Header("表示制御用\u3000ポモドーロが動いていない時は壁紙中に出さない")]
	private CanvasGroup _pomodoroCanvasGroup;

	[SerializeField]
	[Header("表示制御用\u3000ポイント")]
	private CanvasGroup _playerPointUICanvasGroup;

	private bool _isCheckEndLvDirection;

	private bool _isWaitEndExpDirection;

	private CancellationTokenSource _lvUpCheckCancellationTokenSource;

	private CancellationTokenSource _deactivationPlayerLvUICancellationTokenSource;

	private CancellationTokenSource _activetationPlayerLvUICancellationTokenSource;

	private bool _curPlayerPointUIActiveState;

	private Tween _pomodoroTween;

	private bool _pomodoroTimerWorking;

	private void OnDestroy()
	{
		CancelActivationPlayerLvUI();
		CancelCheckedLvUp();
		CancelDeactivationPlayerLvUI();
	}

	public void Setup()
	{
	}

	public void Update()
	{
		UpdateWallpaperLeftTopViewShow();
		UpdatePlayerLvShow();
	}

	private void UpdatePlayerLvShow()
	{
		if (_isCheckEndLvDirection && _playerLevelUIService.NotAddedYetShowExp <= 0f)
		{
			_isCheckEndLvDirection = false;
			CheckPlayerLvUpAsync().Forget();
		}
	}

	private void UpdateWallpaperLeftTopViewShow()
	{
		if (_manager.State != WallPaperManagerForMobile.WallPaperState.Horizontal && _curPlayerPointUIActiveState != CheckPlayerPointUIActive())
		{
			_curPlayerPointUIActiveState = CheckPlayerPointUIActive();
			if (_curPlayerPointUIActiveState)
			{
				_view.DeactivateLeftTopUIsAsync().Forget();
			}
			else
			{
				_view.ActivateLeftTopUIsAsync().Forget();
			}
		}
	}

	private bool CheckPlayerPointUIActive()
	{
		return _playerPointForAlphaCheck.alpha > 0f;
	}

	private void CancelCheckedLvUp()
	{
		if (_lvUpCheckCancellationTokenSource != null)
		{
			_lvUpCheckCancellationTokenSource.Cancel();
			_lvUpCheckCancellationTokenSource.Dispose();
			_lvUpCheckCancellationTokenSource = null;
		}
		_isWaitEndExpDirection = false;
	}

	private async UniTask CheckPlayerLvUpAsync()
	{
		CancelCheckedLvUp();
		_lvUpCheckCancellationTokenSource = new CancellationTokenSource();
		if (_playerLevelUIService.IsEndLevelUpDirection())
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896), ignoreTimeScale: false, PlayerLoopTiming.Update, _lvUpCheckCancellationTokenSource.Token);
			await UniTask.WaitUntil(_playerLevelUIService.IsEndLevelUpDirection, PlayerLoopTiming.Update, _lvUpCheckCancellationTokenSource.Token);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, _lvUpCheckCancellationTokenSource.Token);
		}
		else
		{
			await UniTask.WaitUntil(_playerLevelUIService.IsEndLevelUpDirection, PlayerLoopTiming.Update, _lvUpCheckCancellationTokenSource.Token);
			await UniTask.Delay(TimeSpan.FromSeconds(1.0), ignoreTimeScale: false, PlayerLoopTiming.Update, _lvUpCheckCancellationTokenSource.Token);
		}
		_isWaitEndExpDirection = false;
		if (_manager.State != WallPaperManagerForMobile.WallPaperState.UnUsed)
		{
			DeactivatePlayerLvUIAsync().Forget();
		}
	}

	private void SetMusicTitle(string title)
	{
		if (!(_view == null))
		{
			_view.SetMusicTitle(title);
		}
	}

	private void SetNoMusic()
	{
		if (!(_view == null))
		{
			_view.SetNoMusic();
		}
	}

	public async UniTask ActivatePlayerLvUIAsync(bool isIfWorkingStopped)
	{
		if (isIfWorkingStopped)
		{
			CancelDeactivationPlayerLvUI();
			CancelActivationPlayerLvUI();
		}
		else if (_activetationPlayerLvUICancellationTokenSource != null)
		{
			return;
		}
		if (!(_playerLvUICanvasGroup.alpha >= 1f))
		{
			_activetationPlayerLvUICancellationTokenSource = new CancellationTokenSource();
			await _playerLvUICanvasGroup.DOFade(1f, 0.2f).AwaitForComplete(TweenCancelBehaviour.Kill, _activetationPlayerLvUICancellationTokenSource.Token);
			if (_activetationPlayerLvUICancellationTokenSource != null)
			{
				_activetationPlayerLvUICancellationTokenSource.Dispose();
				_activetationPlayerLvUICancellationTokenSource = null;
			}
		}
	}

	public async UniTask DeactivatePlayerLvUIAsync()
	{
		CancelActivationPlayerLvUI();
		CancelDeactivationPlayerLvUI();
		if (!(_playerLvUICanvasGroup.alpha <= 0f))
		{
			_deactivationPlayerLvUICancellationTokenSource = new CancellationTokenSource();
			await _playerLvUICanvasGroup.DOFade(0f, 0.2f).AwaitForComplete(TweenCancelBehaviour.Kill, _deactivationPlayerLvUICancellationTokenSource.Token);
			if (_deactivationPlayerLvUICancellationTokenSource != null)
			{
				_deactivationPlayerLvUICancellationTokenSource.Dispose();
				_deactivationPlayerLvUICancellationTokenSource = null;
			}
		}
	}

	private void CancelDeactivationPlayerLvUI()
	{
		if (_deactivationPlayerLvUICancellationTokenSource != null)
		{
			_deactivationPlayerLvUICancellationTokenSource.Cancel();
			_deactivationPlayerLvUICancellationTokenSource.Dispose();
			_deactivationPlayerLvUICancellationTokenSource = null;
		}
	}

	public void CancelActivationPlayerLvUI()
	{
		if (_activetationPlayerLvUICancellationTokenSource != null)
		{
			_activetationPlayerLvUICancellationTokenSource.Cancel();
			_activetationPlayerLvUICancellationTokenSource.Dispose();
			_activetationPlayerLvUICancellationTokenSource = null;
		}
	}

	private void ActivatePomodoro()
	{
		_pomodoroTween?.Kill();
		_pomodoroTween = null;
		if (_pomodoroCanvasGroup.alpha >= 1f)
		{
			_pomodoroCanvasGroup.alpha = 1f;
			_pomodoroCanvasGroup.blocksRaycasts = true;
			return;
		}
		_pomodoroCanvasGroup.blocksRaycasts = false;
		_pomodoroTween = _pomodoroCanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
		{
			_pomodoroCanvasGroup.blocksRaycasts = true;
		});
	}

	private void DeactivatePomodoro()
	{
		_pomodoroTween?.Kill();
		_pomodoroTween = null;
		if (_pomodoroCanvasGroup.alpha <= 0f)
		{
			_pomodoroCanvasGroup.alpha = 0f;
			_pomodoroCanvasGroup.blocksRaycasts = false;
		}
		else
		{
			_pomodoroCanvasGroup.blocksRaycasts = false;
			_pomodoroTween = _pomodoroCanvasGroup.DOFade(0f, 0.2f);
		}
	}

	private void SetIgnoreParentGroups(bool ignore)
	{
		_pomodoroCanvasGroup.ignoreParentGroups = ignore;
		_playerLvUICanvasGroup.ignoreParentGroups = ignore;
		_playerPointUICanvasGroup.ignoreParentGroups = ignore;
	}
}
