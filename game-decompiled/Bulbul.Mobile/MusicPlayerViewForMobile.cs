using System;
using NestopiSystem;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayerViewForMobile : MonoBehaviour, IMusicPlayerUI, IMusicUIBase
{
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
	private Button _showVolumeWindowButton;

	[SerializeField]
	private MusicPlayerVolumeIconView _volumeIconView;

	[SerializeField]
	private MusicPlayerVolumeWindow _volumeWindow;

	[SerializeField]
	private MusicPlayerVolumeUISwitch _volumeUISwich;

	private Subject<float> _onChangeProgress = new Subject<float>();

	private ViewportOverTextAutoScroller _musicTitleAutoScroller;

	private ViewportOverTextAutoScroller _artistNameAutoScroller;

	private bool isInitialized;

	public Observable<Unit> OnClickSwitchMuteButton => _volumeWindow.OnClickSwitchMuteButton;

	public Observable<float> OnChangeVolume => _volumeWindow.OnVolumeChanged;

	public Observable<float> OnChangeProgress => _onChangeProgress;

	public Observable<Unit> OnClickPlayOrPauseButton => _playOrPauseButton.OnClickAsObservable();

	public Observable<Unit> OnClickShuffleButton => _shuffleButton.OnClickAsObservable();

	public Observable<Unit> OnClickNextButton => _musicNextButton.OnClickAsObservable();

	public Observable<Unit> OnClickBackButton => _musicBackButton.OnClickAsObservable();

	public Observable<Unit> OnClickLoopButton => _loopButton.OnClickAsObservable();

	private void SetupVolumeUI()
	{
		_volumeUISwich.Setup();
		_volumeWindow.Setup();
		ObservableSubscribeExtensions.Subscribe(_showVolumeWindowButton.OnClickAsObservable(), delegate
		{
			VolumeInfo musicVolumeInfo2 = SaveDataManager.Instance.SettingData.MusicVolumeInfo;
			MusicPlayerVolumeWindow.SetupCurrentVolume(musicVolumeInfo2.Value);
			_volumeWindow.SetMute(musicVolumeInfo2.IsMute.Value);
			_volumeWindow.Activate();
			_volumeUISwich.ActivateVolumeUI();
		}).AddTo(this);
		MusicPlayerVolumeWindow.OnChangedMuteUI.Subscribe(delegate(bool mute)
		{
			_volumeIconView.ChangeMute(mute);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_volumeWindow.OnInputAnyNotice, delegate
		{
			_volumeUISwich.SetResetAutoDeactivationTimer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_volumeWindow.OnClickCloseButton, delegate
		{
			_volumeUISwich.DeactivateVolumeUI();
		}).AddTo(this);
		VolumeInfo musicVolumeInfo = SaveDataManager.Instance.SettingData.MusicVolumeInfo;
		_volumeIconView.ChangeMute(musicVolumeInfo.IsMute.Value);
		MusicPlayerVolumeWindow.SetupCurrentVolume(musicVolumeInfo.Value);
	}

	private void Init()
	{
		if (!isInitialized)
		{
			_musicTitleText.TryGetComponent<ViewportOverTextAutoScroller>(out _musicTitleAutoScroller);
			_artistNameText.TryGetComponent<ViewportOverTextAutoScroller>(out _artistNameAutoScroller);
			isInitialized = true;
		}
	}

	public void Setup(bool isPlay, bool isLoop)
	{
		Init();
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
		SetupVolumeUI();
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

	public void OnChangeMusic(string musicName, string artistName, MusicChangeKind kind)
	{
		SetMusicTitle(musicName);
		SetArtistName(artistName);
	}

	private void SetMusicTitle(string musicName)
	{
		Init();
		if (_musicTitleAutoScroller != null)
		{
			_musicTitleAutoScroller.SetText(musicName);
		}
		else
		{
			_musicTitleText.SetText(musicName);
		}
	}

	private void SetArtistName(string artistName)
	{
		Init();
		if (_artistNameAutoScroller != null)
		{
			_artistNameAutoScroller.SetText(artistName);
		}
		else
		{
			_artistNameText.SetText(artistName);
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

	public void OnPauseMusic()
	{
		_playOrPauseButtonImage.sprite = _playButtonSprite;
	}

	public void OnPlayMusic()
	{
		_playOrPauseButtonImage.sprite = _pauseButtonSprite;
	}

	public void SetMute(bool isMute)
	{
	}

	public void UpdateProgressBar(float progressAmount)
	{
		if (!isDraggingProgressSlider)
		{
			musicProgressSlider.value = progressAmount;
		}
	}
}
