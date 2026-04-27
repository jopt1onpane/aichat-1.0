using System;
using System.Threading.Tasks;
using Bulbul;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using VContainer;

public class SlideFadeAnnounceDirection : MonoBehaviour
{
	public enum AnnounceType
	{
		None,
		UnlockEnvironment,
		UnlockDecoration,
		UnlockMusic,
		WantTalk,
		SaveDataMigration,
		MigrationInsufficientStorage,
		SaveDataBrokenBackup,
		PomodoroNormalMode,
		PomodoroAlterEgoMode,
		UnlockAlterEgoMode,
		FinishScenarioAlterEgo,
		PomodoroBearsRestaurantMode,
		UnlockBearsRestaurantMode,
		FinishScenarioBearsRestaurant,
		PomodoroValentine2026Mode,
		UnlockValentine2026Mode,
		FinishScenarioValentine2026,
		PomodoroLunaNewYear2026Mode,
		UnlockLunaNewYear2026Mode,
		FinishScenarioLunaNewYear2026,
		PomodoroNearSpring2026Mode,
		UnlockNearSpring2026Mode,
		FinishScenarioNearSpring2026,
		DemoStoryEnd,
		UnlockExtraScenario,
		ImportImpossibleLimit,
		ImportFailedLimit,
		ImportFailedImportedFile,
		ImportFailedImportedFolder,
		ImportFailedInvildFile,
		ImportFailedInvildFolder,
		WallPaperOperationPortrait
	}

	public struct AsyncPlayScope(SlideFadeAnnounceDirection parent) : IAsyncDisposable
	{
		public static readonly AsyncPlayScope Empty = new AsyncPlayScope(null);

		private SlideFadeAnnounceDirection parent = parent;

		public async ValueTask DisposeAsync()
		{
			if ((bool)parent)
			{
				SlideFadeAnnounceDirection p = parent;
				parent = null;
				await p._canvasGroup.DOFade(0f, p._fadeSeconds).ToUniTask();
				p.gameObject.SetActive(value: false);
				p._isPlaying = false;
			}
		}
	}

	private const float WantTalkAnnounceDelaySeconds = 20f;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("ローカライズ用テキスト")]
	private TextLocalizationBehaviour _textLocalizationBehavior;

	private float _fadeSeconds;

	private float _fadeOutDelaySeconds;

	private float _moveAmountY;

	private RectTransform _rectTransform;

	private float _initPosY;

	private float _fromPosY;

	private float _toPosY;

	private bool _isPlaying;

	private Tween _fadeInMoveTween;

	private Tween _fadeTween;

	private Tween _fadeOutTween;

	private Tween _fadeOutMoveTween;

	private DateTime _lastWantTalkAnnounceDateTime;

	private bool _isPossibleWantTalkAnnounce = true;

	private PlayerLevelService _playerLevelService;

	public bool IsPlaying => _isPlaying;

	public bool IsPossibleWantTalkAnnounce => _isPossibleWantTalkAnnounce;

	public void InitSetup(PlayerLevelService playerLevelService = null)
	{
		if ((object)_rectTransform == null)
		{
			_rectTransform = GetComponent<RectTransform>();
		}
		_initPosY = _rectTransform.anchoredPosition.y;
		_lastWantTalkAnnounceDateTime = DateTime.MinValue;
		if (playerLevelService != null)
		{
			_playerLevelService = playerLevelService;
			ObservableSubscribeExtensions.Subscribe(_playerLevelService.OnAddExp, delegate
			{
				_isPossibleWantTalkAnnounce = true;
			}).AddTo(this);
		}
	}

	private void PlaySetup(AnnounceType type)
	{
		switch (type)
		{
		case AnnounceType.UnlockEnvironment:
			PlaySetup(0.6f, 1.8f, 7f, "announce_new_enviroment");
			_systemSeService.PlayLevelUp();
			break;
		case AnnounceType.UnlockDecoration:
			PlaySetup(0.6f, 1.8f, 7f, "announce_new_decoration");
			_systemSeService.PlayLevelUp();
			break;
		case AnnounceType.UnlockMusic:
			PlaySetup(0.6f, 1.8f, 7f, "announce_new_music");
			break;
		case AnnounceType.WantTalk:
			if (!((DateTime.Now - _lastWantTalkAnnounceDateTime).TotalSeconds < 20.0))
			{
				PlaySetup(0.6f, 1.8f, 7f, "ui_popup_add_new_story");
				_lastWantTalkAnnounceDateTime = DateTime.Now;
				_systemSeService.PlayLevelUp();
				_isPossibleWantTalkAnnounce = false;
			}
			break;
		case AnnounceType.SaveDataMigration:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_wait_savedata_migration");
			break;
		case AnnounceType.MigrationInsufficientStorage:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_wait_savedata_migration_insufficient");
			break;
		case AnnounceType.SaveDataBrokenBackup:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_wait_savedata_broken_backup");
			break;
		case AnnounceType.PomodoroNormalMode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_normal_mode");
			break;
		case AnnounceType.PomodoroAlterEgoMode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_alterego_mode");
			break;
		case AnnounceType.UnlockAlterEgoMode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_unlock_alterego_mode");
			break;
		case AnnounceType.FinishScenarioAlterEgo:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_finish_alterego_scenario");
			break;
		case AnnounceType.PomodoroBearsRestaurantMode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_bearsrestaurant_mode");
			break;
		case AnnounceType.UnlockBearsRestaurantMode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_unlock_bearsrestaurant_mode");
			break;
		case AnnounceType.FinishScenarioBearsRestaurant:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_finish_bearsrestaurant_scenario");
			break;
		case AnnounceType.PomodoroValentine2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_valentine2026_mode");
			break;
		case AnnounceType.UnlockValentine2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_unlock_valentine2026_mode");
			break;
		case AnnounceType.FinishScenarioValentine2026:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_finish_valentine2026_scenario");
			break;
		case AnnounceType.PomodoroLunaNewYear2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_lunaNewYear2026_mode");
			break;
		case AnnounceType.UnlockLunaNewYear2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_unlock_lunaNewYear2026_mode");
			break;
		case AnnounceType.FinishScenarioLunaNewYear2026:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_finish_lunaNewYear2026_scenario");
			break;
		case AnnounceType.PomodoroNearSpring2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_pomodoro_nearSpring2026_mode");
			break;
		case AnnounceType.UnlockNearSpring2026Mode:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_unlock_nearSpring2026_mode");
			break;
		case AnnounceType.FinishScenarioNearSpring2026:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_finish_nearSpring2026_scenario");
			break;
		case AnnounceType.DemoStoryEnd:
			PlaySetup(0.6f, 5f, 7f, "announce_demo_end_story");
			break;
		case AnnounceType.UnlockExtraScenario:
			PlaySetup(0.6f, 1.8f, 7f, "ui_popup_add_new_story");
			break;
		case AnnounceType.ImportImpossibleLimit:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_import_impossible_limit");
			break;
		case AnnounceType.ImportFailedLimit:
			PlaySetup(0.6f, 3f, 7f, "ui_announce_import_failed_limit");
			break;
		case AnnounceType.ImportFailedImportedFile:
			PlaySetup(0.6f, 3f, 7f, "ui_announce_import_failed_imported_for_file");
			break;
		case AnnounceType.ImportFailedImportedFolder:
			PlaySetup(0.6f, 3f, 7f, "ui_announce_import_failed_imported_for_folder");
			break;
		case AnnounceType.ImportFailedInvildFile:
			PlaySetup(0.6f, 3f, 7f, "ui_announce_import_failed_not_supported_single");
			break;
		case AnnounceType.ImportFailedInvildFolder:
			PlaySetup(0.6f, 3f, 7f, "ui_announce_import_failed__supported_multiple");
			break;
		case AnnounceType.WallPaperOperationPortrait:
			PlaySetup(0.6f, 1.8f, 7f, "ui_announce_wallpaper_operation_portrait");
			break;
		}
	}

	private void PlaySetup(float fadeSeconds, float fadeOutDelaySeconds, float moveAmountY, string localizeID)
	{
		_canvasGroup.alpha = 0f;
		_fadeSeconds = fadeSeconds;
		_fadeOutDelaySeconds = fadeOutDelaySeconds;
		_moveAmountY = moveAmountY;
		_textLocalizationBehavior.Set(localizeID);
		_fromPosY = _initPosY - _moveAmountY;
		_toPosY = _initPosY;
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
		_rectTransform.anchoredPosition = new Vector3(anchoredPosition.x, _fromPosY);
	}

	public void Play(AnnounceType type)
	{
		_fadeInMoveTween?.Kill();
		_fadeTween?.Kill();
		_fadeOutTween?.Kill();
		_fadeOutMoveTween?.Kill();
		PlaySetup(type);
		_isPlaying = true;
		base.gameObject.SetActive(value: true);
		_fadeInMoveTween = _rectTransform.DOAnchorPosY(_toPosY, _fadeSeconds);
		_fadeTween = _canvasGroup.DOFade(1f, _fadeSeconds).OnComplete(delegate
		{
			_fadeOutTween = _canvasGroup.DOFade(0f, _fadeSeconds).SetDelay(_fadeOutDelaySeconds).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
				_isPlaying = false;
			});
			_fadeOutMoveTween = _rectTransform.DOAnchorPosY(_fromPosY, _fadeSeconds).SetDelay(_fadeOutDelaySeconds);
		});
	}

	public async UniTask<AsyncPlayScope> CreatePlayScopeAsync(AnnounceType type)
	{
		PlaySetup(type);
		_isPlaying = true;
		_rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, _fromPosY);
		_canvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: true);
		_fadeInMoveTween?.Kill();
		_fadeInMoveTween = _rectTransform.DOAnchorPosY(_toPosY, _fadeSeconds);
		_fadeTween?.Kill();
		_fadeTween = _canvasGroup.DOFade(1f, _fadeSeconds).OnComplete(delegate
		{
			_rectTransform.DOAnchorPosY(_fromPosY, _fadeSeconds).SetDelay(_fadeOutDelaySeconds);
		});
		await _fadeTween.ToUniTask();
		return new AsyncPlayScope(this);
	}
}
