using System;
using Bulbul;
using DG.Tweening;
using NestopiSystem;
using ObservableCollections;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class MusicUI : MonoBehaviour, IMusicListUI, IMusicUIBase, IMusicPlayerUI, IMusicTagListShowController, IMusicPlayListShowController
{
	[Inject]
	private ChangeOrderService _changeOrderService;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[SerializeField]
	[Header("楽曲名テキスト")]
	private TextMeshProUGUI _musicTitleText;

	[SerializeField]
	[Header("作家名テキスト")]
	private TextMeshProUGUI _artistNameText;

	[SerializeField]
	[Header("曲の進行度バー")]
	private Slider musicProgressSlider;

	private bool isDraggingProgressSlider;

	private bool isEnteringProgressArea;

	[SerializeField]
	private EventTrigger progressDraggableArea;

	[SerializeField]
	private CanvasGroup progressBarHandleGroup;

	[SerializeField]
	[Header("再生、一時停止ボタンImage")]
	private Image _playOrPauseButtonImage;

	[SerializeField]
	private Sprite _playButtonSprite;

	[SerializeField]
	private Sprite _pauseButtonSprite;

	[SerializeField]
	[Header("ボリュームバー")]
	private Slider _volumeSlider;

	[SerializeField]
	private MusicVolumeUI volumeUI;

	[SerializeField]
	[Header("ループ切り替えボタンImage")]
	private Image _loopChangeButtonImage;

	[SerializeField]
	private Sprite _loopButtonSprite;

	[SerializeField]
	private Sprite _notLoopButtonSprite;

	[SerializeField]
	[Header("シャッフル再生切り替えボタンImage")]
	private Image _shuffleChangeButtonImage;

	[SerializeField]
	private Sprite _shuffleButtonSprite;

	[SerializeField]
	private Sprite _notShuffleBUttonSprite;

	[SerializeField]
	[Header("リスト部分の動作を切り分けたクラス")]
	private MusicPlayListView _musicPlayListView;

	[SerializeField]
	[Header("プレイリストオブジェクト")]
	private GameObject _playlistParentObject;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private Button _switchMuteButton;

	[SerializeField]
	private Button _playOrPauseButton;

	[SerializeField]
	private Button _musicNextButton;

	[SerializeField]
	private Button _musicBackButton;

	[SerializeField]
	private Button _shuffleButton;

	[SerializeField]
	private Button _loopButton;

	[SerializeField]
	private Button _listCloseButton;

	private FacilityMusic _facilityMusic;

	private bool isPlaylistDirty;

	private Subject<float> _onChangeVolume = new Subject<float>();

	private Subject<float> _onChangeProgress = new Subject<float>();

	private MusicTagListUI tagListUI;

	private RectTransform _playListRectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private bool _isActive;

	public Observable<Unit> OnClickSwitchMuteButton => _switchMuteButton.OnClickAsObservable();

	public Observable<Unit> OnClickPlayOrPauseButton => _playOrPauseButton.OnClickAsObservable();

	public Observable<Unit> OnClickNextButton => _musicNextButton.OnClickAsObservable();

	public Observable<Unit> OnClickBackButton => _musicBackButton.OnClickAsObservable();

	public Observable<Unit> OnClickShuffleButton => _shuffleButton.OnClickAsObservable();

	public Observable<float> OnChangeVolume => _onChangeVolume;

	public Observable<Unit> OnClickLoopButton => _loopButton.OnClickAsObservable();

	public Observable<Unit> OnClickListCloseButton => _listCloseButton.OnClickAsObservable();

	public Observable<float> OnChangeProgress => _onChangeProgress;

	void IMusicListUI.Setup(IReadOnlyObservableList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic)
	{
		tagListUI = RoomLifetimeScope.Resolve<MusicTagListUI>();
		tagListUI.Setup();
		_musicPlayListView.Setup(facilityMusic, audioInfoList);
		_facilityMusic = facilityMusic;
		ObservableSubscribeExtensions.Subscribe(audioInfoList.ObserveCountChanged(), delegate
		{
			isPlaylistDirty = true;
		}).AddTo(this);
		_musicPlayListView.ViewPlayList();
		_playListRectTransform = _playlistParentObject.transform as RectTransform;
		_fromPosY = _playListRectTransform.anchoredPosition.y + -8f;
		_toPosY = _playListRectTransform.anchoredPosition.y;
	}

	void IMusicPlayerUI.Setup(bool isPlay, bool isLoop)
	{
		if (isPlay)
		{
			OnPlayMusic();
		}
		else
		{
			OnPauseMusic();
		}
		OnChangeLoop(isLoop);
		OnChangeShuffle(isShuffle: false);
		VolumeInfo musicVolumeInfo = SaveDataManager.Instance.SettingData.MusicVolumeInfo;
		volumeUI.SetVolume(musicVolumeInfo.Value);
		volumeUI.SetMute(musicVolumeInfo.IsMute.Value);
		Observable.Merge<bool?>(ObservableTriggerExtensions.OnPointerDownAsObservable(musicProgressSlider).Select((Func<PointerEventData, bool?>)((PointerEventData _) => true)), ObservableTriggerExtensions.OnPointerUpAsObservable(musicProgressSlider).Select((Func<PointerEventData, bool?>)((PointerEventData _) => false)), from _ in musicProgressSlider.OnDragAsObservable()
			select (bool?)null).Subscribe(delegate(bool? dragging)
		{
			if (dragging.HasValue)
			{
				isDraggingProgressSlider = dragging.Value;
				if (!dragging.Value && !isEnteringProgressArea)
				{
					progressBarHandleGroup.alpha = 0f;
				}
			}
			_onChangeProgress.OnNext(musicProgressSlider.value);
		}).AddTo(this);
		Observable.Merge<bool>(from _ in progressDraggableArea.OnPointerEnterAsObservable()
			select true, from _ in progressDraggableArea.OnPointerExitAsObservable()
			select false).Subscribe(delegate(bool isEntered)
		{
			isEnteringProgressArea = isEntered;
			if (isEntered)
			{
				progressBarHandleGroup.alpha = 1f;
			}
			else if (!isDraggingProgressSlider)
			{
				progressBarHandleGroup.alpha = 0f;
			}
		}).AddTo(this);
	}

	private void LateUpdate()
	{
		if (isPlaylistDirty)
		{
			_musicPlayListView.ViewPlayList();
			isPlaylistDirty = false;
		}
	}

	public void UpdateProgressBar(float progressAmount)
	{
		if (!isDraggingProgressSlider)
		{
			musicProgressSlider.value = progressAmount;
		}
	}

	public void OnPlayMusic()
	{
		_playOrPauseButtonImage.sprite = _pauseButtonSprite;
	}

	public void OnPauseMusic()
	{
		_playOrPauseButtonImage.sprite = _playButtonSprite;
	}

	public void OnChangeLoop(bool isLoop)
	{
		if (isLoop)
		{
			_loopChangeButtonImage.sprite = _loopButtonSprite;
		}
		else
		{
			_loopChangeButtonImage.sprite = _notLoopButtonSprite;
		}
	}

	public void OnChangeShuffle(bool isShuffle)
	{
		if (isShuffle)
		{
			_shuffleChangeButtonImage.sprite = _shuffleButtonSprite;
		}
		else
		{
			_shuffleChangeButtonImage.sprite = _notShuffleBUttonSprite;
		}
	}

	public void OnChangeMusic(string musicTitle, string artistName, MusicChangeKind changeKind)
	{
		_ = _musicTitleText.text;
		_musicTitleText.text = musicTitle;
		_artistNameText.text = artistName;
		if (changeKind != MusicChangeKind.PlaylistCellClick && (changeKind == MusicChangeKind.Manual || _musicPlayListView.CopyLastScrollDragTime(10f) < DateTime.Now))
		{
			_musicPlayListView.ScrollToPlayingMusic(musicTitle);
		}
		_musicPlayListView.ResetLastScrollDragTime();
	}

	public bool IsActivePlayList()
	{
		return _isActive;
	}

	public void ActivatePlayList()
	{
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Playlist);
		_isActive = true;
		CloseMusicTagList(immediate: true);
		_facilityOpenButton.ActivateUseUI();
		_playlistParentObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _playListRectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
		GameAudioInfo playingMusic = _facilityMusic.PlayingMusic;
		if (playingMusic != null)
		{
			_musicPlayListView.ScrollToPlayingMusic(playingMusic.Title);
		}
	}

	public void DeactivatePlayList()
	{
		_isActive = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _playListRectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_playlistParentObject.SetActive(value: false);
		});
	}

	public void OnChangeVolumeSlider()
	{
		_onChangeVolume.OnNext(_volumeSlider.value);
	}

	public void SetMute(bool isMute)
	{
		volumeUI.SetMute(isMute);
	}

	public void ToggleMusicTagList()
	{
		tagListUI.ToggleMusicTagList();
	}

	public void OpenMusicTagList()
	{
		tagListUI.OpenMusicTagList();
	}

	public void CloseMusicTagList(bool immediate = false)
	{
		tagListUI.CloseMusicTagList(immediate);
	}
}
