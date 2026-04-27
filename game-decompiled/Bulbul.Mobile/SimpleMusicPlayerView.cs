using System.Threading;
using Cysharp.Threading.Tasks;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SimpleMusicPlayerView : MonoBehaviour, IMusicPlayerUI, IMusicUIBase
{
	[Header("再生ボタン")]
	[SerializeField]
	private Image _playOrPauseButtonImage;

	[SerializeField]
	private Sprite _pauseButtonSprite;

	[SerializeField]
	private Sprite _playButtonSprite;

	[SerializeField]
	private Button _playOrPauseButton;

	[SerializeField]
	private Button _nextButton;

	[SerializeField]
	private TextMeshProUGUI _musicNameText;

	[SerializeField]
	private TextMeshProUGUI _artistText;

	[SerializeField]
	private Button _showVolumeWindowButton;

	[SerializeField]
	private MusicPlayerVolumeIconView _volumeIconView;

	[SerializeField]
	private MusicPlayerVolumeWindow _volumeWindow;

	[SerializeField]
	private MusicPlayerVolumeUISwitch _volumeUISwich;

	[SerializeField]
	private Image _progressFill;

	private ViewportOverTextAutoScroller _musicNameAutoScroller;

	private ViewportOverTextAutoScroller _artistNameAutoScroller;

	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	private bool isInitialized;

	public Observable<Unit> OnClickSwitchMuteButton => _volumeWindow.OnClickSwitchMuteButton;

	public Observable<float> OnChangeVolume => _volumeWindow.OnVolumeChanged;

	public Observable<Unit> OnClickPlayOrPauseButton => _playOrPauseButton.OnClickAsObservable();

	public Observable<Unit> OnClickShuffleButton => null;

	public Observable<Unit> OnClickNextButton => _nextButton.OnClickAsObservable();

	public Observable<Unit> OnClickBackButton => null;

	public Observable<Unit> OnClickLoopButton => null;

	Observable<float> IMusicPlayerUI.OnChangeProgress => null;

	private void Init()
	{
		if (!isInitialized)
		{
			_musicNameText.TryGetComponent<ViewportOverTextAutoScroller>(out _musicNameAutoScroller);
			_artistText.TryGetComponent<ViewportOverTextAutoScroller>(out _artistNameAutoScroller);
			isInitialized = true;
		}
	}

	private void Start()
	{
		Init();
		_volumeUISwich.Setup();
		_volumeWindow.Setup();
		MusicPlayerVolumeWindow.SetupCurrentVolume(SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value);
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
			_volumeWindow.DeactivateVolumePercent(_cancellationTokenSource.Token).Forget();
		}).AddTo(this);
		VolumeInfo musicVolumeInfo = SaveDataManager.Instance.SettingData.MusicVolumeInfo;
		_volumeIconView.ChangeMute(musicVolumeInfo.IsMute.Value);
		MusicPlayerVolumeWindow.SetupCurrentVolume(musicVolumeInfo.Value);
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
	}

	private void SetMusicName(string musicName)
	{
		Init();
		if (_musicNameAutoScroller != null)
		{
			_musicNameAutoScroller.SetText(musicName);
		}
		else
		{
			_musicNameText.SetText(musicName);
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
			_artistText.SetText(artistName);
		}
	}

	public void OnChangeLoop(bool isLoop)
	{
	}

	public void OnChangeMusic(string musicName, string artistName, MusicChangeKind kind)
	{
		SetMusicName(musicName);
		SetArtistName(artistName);
	}

	public void OnChangeShuffle(bool isShuffle)
	{
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

	public void Setup(bool isPlay, bool isLoop, IReadOnlyObservableList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic)
	{
	}

	public void UpdateProgressBar(float amount)
	{
		_progressFill.fillAmount = amount;
	}
}
