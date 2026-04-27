using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayListItemView : MonoBehaviour, IAnimationView, IRemovableItemView
{
	private static readonly float _fadeSec = 0.5f;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform cellContentRoot;

	[SerializeField]
	private GameObject raycastBlocker;

	[SerializeField]
	[Header("再生ボタン")]
	private Button _playMusicbutton;

	[SerializeField]
	[Header("楽曲名テキスト")]
	private TextMeshProUGUI _musicTitleText;

	[SerializeField]
	private ViewportOverTextAutoScroller _playingMusicTitleText;

	[SerializeField]
	[Header("作家名テキスト")]
	private TextMeshProUGUI _artistNameText;

	[SerializeField]
	private ViewportOverTextAutoScroller _playingArtistNameText;

	[SerializeField]
	[Header("背景画像")]
	private Image _backImage;

	[SerializeField]
	[Header("背景画像")]
	private Sprite _backSprite;

	[SerializeField]
	[Header("選択中背景画像")]
	private Sprite _activeBackSprite;

	[SerializeField]
	[Header("再生中背景画像")]
	private Sprite _playingActiveBackSprite;

	[SerializeField]
	[Header("再生中かつ選択中の背景画像")]
	private Sprite _playingMouseOverBackSprite;

	[SerializeField]
	[Header("ベースUIアニメーション")]
	private HoldButtonAnimation _baseHoldButtonAnimUI;

	[SerializeField]
	[Header("ドラッグUI")]
	private InteractableUI _dragInteractableUI;

	[SerializeField]
	[Header("ドラッグUI")]
	private HoldButtonAnimation _dragHoldButtonAnimUI;

	[SerializeField]
	[Header("ドラッグUI")]
	private ItemDragReorderHandle _dragHandle;

	[SerializeField]
	[Header("ドラッグ中適用スケール")]
	private float _draggingScale = 0.8f;

	[SerializeField]
	[Header("再生候補")]
	private ButtonEventObservable _changePlayCandidateMusicButton;

	[SerializeField]
	[Header("再生候補")]
	private InteractableUI _playCandidateInteractableUI;

	[SerializeField]
	[Header("再生ボタンの画像")]
	private Image playStateIcon;

	[SerializeField]
	[Header("再生ボタンのプレイ中画像")]
	private GameObject playingIconObj;

	[SerializeField]
	[Header("再生ボタンの再生してない画像")]
	private Sprite nonPlayIcon;

	[SerializeField]
	[Header("再生ボタンのプレイ中画像")]
	private Sprite playingIcon;

	[SerializeField]
	[Header("再生ボタンのポーズ画像")]
	private Sprite pauseIcon;

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
	[Header("削除ボタン")]
	private Button removeButton;

	[SerializeField]
	[Header("削除ボタンの画像")]
	private Image removeButtonImage;

	[SerializeField]
	[Header("削除モードで消すCanvasGroup")]
	private CanvasGroup _removingModeDeactivationCanvasGroup;

	[SerializeField]
	[Header("削除ボタンCanvasGroup")]
	private CanvasGroup _removeButtonCanvasGroup;

	[SerializeField]
	[Header("曲情報 Root")]
	private RectTransform _musicInfoRoot;

	[SerializeField]
	[Header("再生中 曲情報 ViewPort")]
	private GameObject _playingMusicTitleViewPort;

	[SerializeField]
	[Header("再生中 作曲者情報 ViewPort")]
	private GameObject _playingArtistNameViewPort;

	[SerializeField]
	[Header("通常モード\u3000曲情報位置")]
	private Vector2 _normalModeMusicInfoAnchoredPos;

	[SerializeField]
	[Header("削除モード 曲情報位置")]
	private Vector2 _removingModeMusicInfoAnchoredPos;

	[SerializeField]
	[Header("モバイル版未購入ロックオブジェクト")]
	private GameObject _mobileDemoEditionLockedObj;

	[Header("ドラッグ中に有効になるオブジェクト")]
	[SerializeField]
	private GameObject _draggingUseBackImage;

	[SerializeField]
	private GameObject _draggingUseHandle;

	private Subject<Unit> onClickPlayButton = new Subject<Unit>();

	private Subject<Unit> onClickFavoriteButton = new Subject<Unit>();

	private Subject<Unit> onClickRemoveButton = new Subject<Unit>();

	private Subject<Unit> onClickCheckBox = new Subject<Unit>();

	private Subject<Unit> _onClickMobileDemoLocked = new Subject<Unit>();

	private RectTransform _rectTransform;

	private ITwoStateUITransition __removingModeUI;

	private MusicPlayListItemModel _playListItemModel;

	private CancellationTokenSource _disableCancelToken;

	public Observable<Unit> OnClickPlayButton => onClickPlayButton;

	public Observable<Unit> OnClickFavoriteButton => onClickFavoriteButton;

	public Observable<Unit> OnClickRemoveButton => onClickRemoveButton;

	public Observable<Unit> OnClickCheckBox => onClickCheckBox;

	public Observable<Unit> OnClickMobileDemoLocked => _onClickMobileDemoLocked;

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public ITwoStateUITransition RemovingModeUI
	{
		get
		{
			if (__removingModeUI == null)
			{
				__removingModeUI = GetComponent<ITwoStateUITransition>();
			}
			return __removingModeUI;
		}
	}

	public ItemDragReorderHandle DragHandle => _dragHandle;

	public MusicPlayListItemModel PlayListItemModel => _playListItemModel;

	RectTransform IAnimationView.AnimationRectTransform => cellContentRoot;

	CanvasGroup IAnimationView.AnimationCanvasGroup => canvasGroup;

	CancellationToken IRemovableItemView.CancellationToken => _disableCancelToken.Token;

	public void ResetItemModel()
	{
		_playListItemModel = null;
	}

	void IAnimationView.SetActiveRaycastBlocker(bool isActive)
	{
		raycastBlocker.SetActive(isActive);
	}

	async UniTask IRemovableItemView.Play(ListItemViewAnimations.RemoveAnimationDirection direction, CancellationToken token)
	{
		CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disableCancelToken.Token);
		try
		{
			await ListItemViewAnimations.PlayRemovingAnimation(this, linkedSource.Token, TweenCancelBehaviour.Kill, direction);
			linkedSource.Token.ThrowIfCancellationRequested();
		}
		catch (OperationCanceledException ex)
		{
			throw ex;
		}
		finally
		{
			linkedSource?.Dispose();
		}
	}

	private void OnDestroy()
	{
		Cancel();
	}

	private void OnDisable()
	{
		if (_disableCancelToken != null)
		{
			Cancel();
			_disableCancelToken = new CancellationTokenSource();
		}
	}

	public void SetActive(bool active)
	{
		if (cellContentRoot.gameObject.activeSelf != active)
		{
			cellContentRoot.gameObject.SetActive(active);
		}
	}

	public void Initialize()
	{
		_disableCancelToken = new CancellationTokenSource();
		_dragInteractableUI.Setup();
		favoriteInteractableUI.Setup();
		_playCandidateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_playMusicbutton.OnClickAsObservable(), delegate
		{
			if (IsMobileDemoEditionLimited(_playListItemModel.audioInfo))
			{
				_onClickMobileDemoLocked.OnNext(Unit.Default);
			}
			else
			{
				onClickPlayButton?.OnNext(Unit.Default);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(favoriteButton.OnClick, delegate
		{
			if (IsMobileDemoEditionLimited(_playListItemModel.audioInfo))
			{
				_onClickMobileDemoLocked.OnNext(Unit.Default);
			}
			else
			{
				onClickFavoriteButton?.OnNext(Unit.Default);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(removeButton.OnClickAsObservable(), delegate
		{
			if (IsMobileDemoEditionLimited(_playListItemModel.audioInfo))
			{
				_onClickMobileDemoLocked.OnNext(Unit.Default);
			}
			else
			{
				onClickRemoveButton?.OnNext(Unit.Default);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_changePlayCandidateMusicButton.OnClick, delegate
		{
			if (IsMobileDemoEditionLimited(_playListItemModel.audioInfo))
			{
				_onClickMobileDemoLocked.OnNext(Unit.Default);
			}
			else
			{
				onClickCheckBox?.OnNext(Unit.Default);
			}
		}).AddTo(this);
		raycastBlocker.SetActive(value: false);
	}

	public void SetModel(MusicPlayListItemModel model, bool isPlaceHolder, bool isRemovingMode)
	{
		_playListItemModel = model;
		Cancel();
		_disableCancelToken = new CancellationTokenSource();
		canvasGroup.alpha = 1f;
		cellContentRoot.anchoredPosition = Vector3.zero;
		SetFavoriteImage();
		SetPlayCandidateImage();
		UpdateMusicInfo();
		UpdateBackImage();
		UpdatePlayStateIcon();
		SetRemoveButtonActive();
	}

	public void UpdateModel(MusicPlayListItemModel model, bool isPlaceHolder, bool isRemovingMode)
	{
		_playListItemModel = model;
		SetFavoriteImage();
		SetPlayCandidateImage();
		UpdateMusicInfo();
		UpdateBackImage();
		UpdatePlayStateIcon();
		SetRemoveButtonActive();
	}

	public void SetFavoriteImage()
	{
		if (_playListItemModel.audioInfo.Tag.HasFlagFast(AudioTag.Favorite))
		{
			favoriteInteractableUI.ActivateUseUI();
		}
		else
		{
			favoriteInteractableUI.DeactivateUseUI();
		}
	}

	public void SetPlayCandidateImage()
	{
		if (PlayListItemModel.isExclude)
		{
			_playCandidateInteractableUI.DeactivateUseUI();
			_musicTitleText.DOColor(new Color32(125, 125, 125, 125), 0.1f);
			_artistNameText.DOColor(new Color32(125, 125, 125, 125), 0.1f);
		}
		else
		{
			_playCandidateInteractableUI.ActivateUseUI();
			_musicTitleText.DOColor(new Color32(247, 247, 247, byte.MaxValue), 0.1f);
			_artistNameText.DOColor(new Color32(247, 247, 247, byte.MaxValue), 0.1f);
		}
	}

	public void SetRemoveButtonActive()
	{
		if (_playListItemModel != null)
		{
			if (string.IsNullOrEmpty(_playListItemModel.audioInfo.LocalPath))
			{
				removeButtonImage.gameObject.SetActive(value: false);
			}
			else
			{
				removeButtonImage.gameObject.SetActive(value: true);
			}
		}
	}

	private void UpdateMusicInfo()
	{
		if (_playListItemModel != null)
		{
			SetActiveMobileDemoEditionLockedObj(IsMobileDemoEditionLimited(_playListItemModel.audioInfo));
			if (_playListItemModel.isPlaying)
			{
				_playingMusicTitleViewPort.SetActive(value: true);
				_playingArtistNameViewPort.SetActive(value: true);
				_playingMusicTitleText.SetText(_playListItemModel.audioInfo.GetPlatformAudioTitle());
				_playingArtistNameText.SetText(_playListItemModel.audioInfo.GetPlatformCredit());
				_musicTitleText.gameObject.SetActive(value: false);
				_artistNameText.gameObject.SetActive(value: false);
			}
			else
			{
				_musicTitleText.SetText(_playListItemModel.audioInfo.GetPlatformAudioTitle());
				_artistNameText.SetText(_playListItemModel.audioInfo.GetPlatformCredit());
				_musicTitleText.gameObject.SetActive(value: true);
				_artistNameText.gameObject.SetActive(value: true);
				_playingMusicTitleViewPort.SetActive(value: false);
				_playingArtistNameViewPort.SetActive(value: false);
			}
		}
	}

	private void UpdateBackImage()
	{
		_backImage.sprite = (PlayListItemModel.isPlaying ? _playingActiveBackSprite : _backSprite);
	}

	private void UpdatePlayStateIcon()
	{
		if (PlayListItemModel.isPlaying)
		{
			playStateIcon.sprite = (PlayListItemModel.isPausing ? pauseIcon : playingIcon);
			bool flag = !PlayListItemModel.isPausing;
			playingIconObj.SetActive(flag);
			playStateIcon.gameObject.SetActive(!flag);
		}
		else
		{
			playingIconObj.SetActive(value: false);
			playStateIcon.gameObject.SetActive(value: true);
			playStateIcon.sprite = nonPlayIcon;
		}
	}

	public void ActivateDraggingImages()
	{
		if (PlayListItemModel != null)
		{
			_draggingUseHandle.SetActive(value: true);
			if (!PlayListItemModel.isPlaying)
			{
				_draggingUseBackImage.SetActive(value: true);
			}
		}
	}

	public void DeactivateDraggingImages()
	{
		if (PlayListItemModel != null)
		{
			_draggingUseHandle.SetActive(value: false);
			_draggingUseBackImage.SetActive(value: false);
		}
	}

	public void Cancel()
	{
		if (_disableCancelToken != null)
		{
			_disableCancelToken?.Cancel();
			_disableCancelToken?.Dispose();
			_disableCancelToken = null;
		}
	}

	public void SetActiveMobileDemoEditionLockedObj(bool active)
	{
		if (_mobileDemoEditionLockedObj.activeSelf != active)
		{
			_mobileDemoEditionLockedObj.SetActive(active);
		}
	}

	private bool IsMobileDemoEditionLimited(GameAudioInfo music)
	{
		return false;
	}
}
