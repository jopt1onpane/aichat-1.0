using DG.Tweening;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayListButtonForMobile : MonoBehaviour, IMusicPlayListButton
{
	private static readonly float _fadeSec = 0.5f;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	[Header("再生ボタン")]
	private Button _playMusicbutton;

	[SerializeField]
	[Header("楽曲名テキスト")]
	private TextMeshProUGUI _musicTitleText;

	[SerializeField]
	[Header("作家名テキスト")]
	private TextMeshProUGUI _artistNameText;

	[SerializeField]
	[Header("背景画像")]
	private Image _backImage;

	[SerializeField]
	[Header("背景画像")]
	private Sprite _activeBackSprite;

	[SerializeField]
	[Header("マウスオーバー中の背景画像")]
	private Sprite _mouseOverBackSprite;

	[SerializeField]
	[Header("再生中背景画像")]
	private Sprite _playingActiveBackSprite;

	[SerializeField]
	[Header("再生中マウスオーバー中の背景画像")]
	private Sprite _playingMouseOverBackSprite;

	[SerializeField]
	[Header("ベースUI")]
	private HoldButtonAnimation _baseHoldButtonAnimUI;

	[SerializeField]
	[Header("ドラッグUI")]
	private InteractableUI _dragInteractableUI;

	[SerializeField]
	[Header("ドラッグUI")]
	private HoldButtonAnimation _dragHoldButtonAnimUI;

	[SerializeField]
	[Header("再生候補")]
	private ButtonEventObservable _changePlayCandidateMusicButton;

	[SerializeField]
	[Header("再生候補")]
	private InteractableUI _playCandidateInteractableUI;

	[SerializeField]
	[Header("グレーアウト")]
	private Image greyOutImage;

	[SerializeField]
	[Header("再生ボタンの画像")]
	private Image playStateIcon;

	[SerializeField]
	[Header("再生ボタンのプレイ中画像")]
	private GameObject playingIcon;

	[SerializeField]
	[Header("再生ボタンのプレイ中画像")]
	private Sprite playingIconOnMouseover;

	[SerializeField]
	[Header("再生ボタンのポーズ画像")]
	private Sprite pauseIcon;

	[SerializeField]
	[Header("再生ボタンの再生してない画像")]
	private Sprite nonPlayIcon;

	[SerializeField]
	[Header("再生ボタンの再生してない画像")]
	private Sprite nonPlayIconMouseover;

	[SerializeField]
	[Header("お気に入りボタン")]
	private ButtonEventObservable favoriteButton;

	[SerializeField]
	[Header("お気に入りボタン")]
	private Image favoriteImage;

	[SerializeField]
	[Header("お気に入りボタン")]
	private InteractableUI favoriteInteractableUI;

	[SerializeField]
	private EventTrigger reorderTrigger;

	[SerializeField]
	[Header("削除ボタン")]
	private Button removeButton;

	[SerializeField]
	[Header("削除モードで消すCanvasGroup")]
	private CanvasGroup _removingModeDeactivationCanvasGroup;

	[SerializeField]
	[Header("削除ボタンCanvasGroup")]
	private CanvasGroup _removeButtonCanvasGroup;

	[SerializeField]
	[Header("通常モード\u3000曲情報位置")]
	private Vector2 _normalModeMusicInfoAnchoredPos;

	[SerializeField]
	[Header("削除モード 曲情報位置")]
	private Vector2 _removingModeMusicInfoAnchoredPos;

	[SerializeField]
	[Header("曲情報 Root")]
	private RectTransform _musicInfoRoot;

	private GameAudioInfo _audioInfo;

	private FacilityMusic _facilityMusic;

	private bool isMouseOver;

	private bool isPaused;

	private bool isDirty;

	private bool isDrag;

	private readonly Subject<(IMusicPlayListButton button, PointerEventData eventData)> onStartReorder = new Subject<(IMusicPlayListButton, PointerEventData)>();

	private readonly Subject<(IMusicPlayListButton button, PointerEventData eventData)> onReorderDrag = new Subject<(IMusicPlayListButton, PointerEventData)>();

	private readonly Subject<(IMusicPlayListButton button, PointerEventData eventData)> onEndReorder = new Subject<(IMusicPlayListButton, PointerEventData)>();

	private RectTransform _rectTransform;

	private bool _isLocal;

	private Sequence _sequence;

	public GameAudioInfo AudioInfo => _audioInfo;

	public Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnStartReorder => onStartReorder;

	public Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnReorderDrag => onReorderDrag;

	public Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnEndReorder => onEndReorder;

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public void Setup(GameAudioInfo audioInfo, FacilityMusic facilityMusic)
	{
		_audioInfo = audioInfo;
		_facilityMusic = facilityMusic;
		_musicTitleText.text = audioInfo.GetPlatformAudioTitle();
		_artistNameText.text = audioInfo.Credit;
		_playMusicbutton.onClick.AddListener(delegate
		{
			if (_facilityMusic.PlayingMusic == _audioInfo)
			{
				facilityMusic.OnClickButtonPlayOrPauseMusic();
			}
			else
			{
				facilityMusic.OnClickButtonPlayListPlayMusicButton(_audioInfo);
			}
		});
		favoriteInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(favoriteButton.OnClick, delegate
		{
			if (_audioInfo.Tag.HasFlagFast(AudioTag.Favorite))
			{
				_facilityMusic.MusicService.UnregisterFavoriteMusic(_audioInfo);
			}
			else
			{
				_facilityMusic.MusicService.RegisterFavoriteMusic(_audioInfo);
			}
			SetFavoriteImage();
		}).AddTo(this);
		SetFavoriteImage();
		_playCandidateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_changePlayCandidateMusicButton.OnClick, delegate
		{
			if (_facilityMusic.MusicService.IsContainsExcludedFromPlaylist(_audioInfo))
			{
				_facilityMusic.MusicService.IncludeInPlaylist(_audioInfo);
			}
			else
			{
				_facilityMusic.MusicService.ExcludeFromPlaylist(_audioInfo);
			}
			SetPlayCandidateImage();
		}).AddTo(this);
		SetPlayCandidateImage();
		_isLocal = _audioInfo.Tag.HasFlagFast(AudioTag.Local);
		if (_isLocal)
		{
			ObservableSubscribeExtensions.Subscribe(removeButton.OnClickAsObservable(), delegate
			{
				RoomLifetimeScope.Resolve<SystemSeService>().PlayCancel();
				_facilityMusic.MusicService.RemoveLocalMusicItem(_audioInfo);
			}).AddTo(this);
		}
		ObservableSubscribeExtensions.Subscribe(facilityMusic.MusicService.OnPauseMusic.Where((GameAudioInfo m) => m == _audioInfo), delegate
		{
			isPaused = true;
			isDirty = true;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(facilityMusic.MusicService.OnPlayMusic, delegate
		{
			isPaused = false;
			isDirty = true;
		}).AddTo(this);
		if (facilityMusic.PlayingMusic == audioInfo)
		{
			isPaused = facilityMusic.IsPaused;
		}
		UpdateBackImage();
		UpdatePlayStateIcon();
		isDrag = false;
		if (_baseHoldButtonAnimUI != null)
		{
			_baseHoldButtonAnimUI.Setup();
		}
	}

	private void Start()
	{
		reorderTrigger.OnBeginDragAsObservable().Select(this, (PointerEventData e, IMusicPlayListButton @this) => (@this: @this, e: e)).Subscribe<(IMusicPlayListButton, PointerEventData)>(onStartReorder)
			.AddTo(this);
		reorderTrigger.OnDragAsObservable().Select(this, (PointerEventData e, IMusicPlayListButton @this) => (@this: @this, e: e)).Subscribe<(IMusicPlayListButton, PointerEventData)>(onReorderDrag)
			.AddTo(this);
		reorderTrigger.OnEndDragAsObservable().Select(this, (PointerEventData e, IMusicPlayListButton @this) => (@this: @this, e: e)).Subscribe<(IMusicPlayListButton, PointerEventData)>(onEndReorder)
			.AddTo(this);
		_dragInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(reorderTrigger.OnBeginDragAsObservable(), delegate
		{
			ActivateDragAnimation();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(reorderTrigger.OnEndDragAsObservable(), delegate
		{
			DeactivateDragAnimation();
		}).AddTo(this);
	}

	public bool IsTitleEquals(string titleName)
	{
		if (_musicTitleText.text.Equals(titleName))
		{
			return true;
		}
		return false;
	}

	public void OnPointerEnter()
	{
		isMouseOver = true;
		isDirty = true;
	}

	public void OnPointerExit()
	{
		isMouseOver = false;
		isDirty = true;
	}

	private void LateUpdate()
	{
		if (isDirty && !isDrag)
		{
			isDirty = false;
			UpdateBackImage();
			UpdatePlayStateIcon();
		}
	}

	private void UpdatePlayStateIcon()
	{
		if (_facilityMusic.PlayingMusic == _audioInfo)
		{
			playStateIcon.sprite = (isPaused ? nonPlayIconMouseover : playingIconOnMouseover);
			bool flag = !isMouseOver && !isPaused;
			playingIcon.gameObject.SetActive(flag);
			playStateIcon.gameObject.SetActive(!flag);
		}
		else
		{
			playingIcon.SetActive(value: false);
			playStateIcon.gameObject.SetActive(value: true);
			playStateIcon.sprite = (isMouseOver ? nonPlayIconMouseover : nonPlayIcon);
		}
	}

	private void UpdateBackImage()
	{
		if (isMouseOver)
		{
			_backImage.sprite = ((_facilityMusic.PlayingMusic == _audioInfo) ? _playingMouseOverBackSprite : _mouseOverBackSprite);
		}
		else
		{
			_backImage.sprite = ((_facilityMusic.PlayingMusic == _audioInfo) ? _playingActiveBackSprite : _activeBackSprite);
		}
	}

	private void SetFavoriteImage()
	{
		if (_audioInfo.Tag.HasFlagFast(AudioTag.Favorite))
		{
			favoriteInteractableUI.ActivateUseUI();
		}
		else
		{
			favoriteInteractableUI.DeactivateUseUI();
		}
	}

	private void SetPlayCandidateImage()
	{
		if (_facilityMusic.MusicService.IsContainsExcludedFromPlaylist(_audioInfo))
		{
			_playCandidateInteractableUI.DeactivateUseUI();
			greyOutImage.DOFade(0.5f, 0.1f);
			_musicTitleText.DOColor(new Color32(125, 125, 125, 125), 0.1f);
			_artistNameText.DOColor(new Color32(125, 125, 125, 125), 0.1f);
		}
		else
		{
			_playCandidateInteractableUI.ActivateUseUI();
			greyOutImage.DOFade(0f, 0.1f);
			_musicTitleText.DOColor(new Color32(247, 247, 247, byte.MaxValue), 0.1f);
			_artistNameText.DOColor(new Color32(247, 247, 247, byte.MaxValue), 0.1f);
		}
	}

	public void Hide()
	{
		canvasGroup.alpha = 0f;
	}

	public void Show()
	{
		canvasGroup.alpha = 1f;
	}

	public void ActivateDragAnimation()
	{
		isDrag = true;
		_backImage.sprite = ((_facilityMusic.PlayingMusic == _audioInfo) ? _playingMouseOverBackSprite : _mouseOverBackSprite);
		if (_baseHoldButtonAnimUI != null)
		{
			_baseHoldButtonAnimUI.ActivateUseUI();
		}
		_dragInteractableUI.ActivateUseUI(isUseDoComplete: true);
		_dragHoldButtonAnimUI.ActivateUseUI();
	}

	public void DeactivateDragAnimation()
	{
		isDrag = false;
		_backImage.sprite = ((_facilityMusic.PlayingMusic == _audioInfo) ? _playingActiveBackSprite : _activeBackSprite);
		if (_baseHoldButtonAnimUI != null)
		{
			_baseHoldButtonAnimUI.DeactivateUseUI();
		}
		_dragInteractableUI.DeactivateUseUI(isUseDoComplete: true);
		_dragHoldButtonAnimUI.DeactivateUseUI();
	}

	public void SwitchMode(bool isRemovingMode = true, bool isImmediate = false)
	{
		if (isRemovingMode)
		{
			PlaySwitchRemovingMode();
		}
		else
		{
			PlaySwitchNormalMode();
		}
	}

	private void PlaySwitchNormalMode(bool isImmediate = false)
	{
		_sequence?.Kill();
		if (isImmediate)
		{
			_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: true);
			_removeButtonCanvasGroup.gameObject.SetActive(value: false);
			_removingModeDeactivationCanvasGroup.alpha = 1f;
			_removeButtonCanvasGroup.alpha = 0f;
			_removingModeDeactivationCanvasGroup.blocksRaycasts = true;
			_removeButtonCanvasGroup.blocksRaycasts = false;
			_musicInfoRoot.anchoredPosition = _normalModeMusicInfoAnchoredPos;
			return;
		}
		_sequence = DOTween.Sequence();
		_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: true);
		_removeButtonCanvasGroup.gameObject.SetActive(value: true);
		_removingModeDeactivationCanvasGroup.blocksRaycasts = false;
		_removeButtonCanvasGroup.blocksRaycasts = false;
		_sequence.Join(_removingModeDeactivationCanvasGroup.DOFade(1f, _fadeSec));
		_sequence.Join(_removeButtonCanvasGroup.DOFade(0f, _fadeSec));
		_sequence.Join(_musicInfoRoot.DOAnchorPos(_normalModeMusicInfoAnchoredPos, _fadeSec));
		_sequence.OnComplete(delegate
		{
			_removingModeDeactivationCanvasGroup.blocksRaycasts = true;
			_removeButtonCanvasGroup.blocksRaycasts = false;
			_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: true);
			_removeButtonCanvasGroup.gameObject.SetActive(value: false);
		});
	}

	private void PlaySwitchRemovingMode(bool isImmediate = false)
	{
		_sequence?.Kill();
		if (isImmediate)
		{
			_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: false);
			_removeButtonCanvasGroup.gameObject.SetActive(value: true);
			_removingModeDeactivationCanvasGroup.alpha = 0f;
			_removeButtonCanvasGroup.alpha = 1f;
			_removingModeDeactivationCanvasGroup.blocksRaycasts = false;
			_removeButtonCanvasGroup.blocksRaycasts = true;
			_musicInfoRoot.anchoredPosition = _removingModeMusicInfoAnchoredPos;
			return;
		}
		_sequence = DOTween.Sequence();
		_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: true);
		_removeButtonCanvasGroup.gameObject.SetActive(value: true);
		_removingModeDeactivationCanvasGroup.blocksRaycasts = false;
		_removeButtonCanvasGroup.blocksRaycasts = false;
		_sequence.Join(_removingModeDeactivationCanvasGroup.DOFade(0f, _fadeSec));
		_sequence.Join(_removeButtonCanvasGroup.DOFade(1f, _fadeSec));
		_sequence.Join(_musicInfoRoot.DOAnchorPos(_removingModeMusicInfoAnchoredPos, _fadeSec));
		_sequence.OnComplete(delegate
		{
			_removingModeDeactivationCanvasGroup.blocksRaycasts = false;
			_removeButtonCanvasGroup.blocksRaycasts = true;
			_removingModeDeactivationCanvasGroup.gameObject.SetActive(value: false);
			_removeButtonCanvasGroup.gameObject.SetActive(value: true);
		});
	}
}
