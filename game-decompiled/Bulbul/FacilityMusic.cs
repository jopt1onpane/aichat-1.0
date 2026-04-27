using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using NestopiSystem;
using NestopiSystem.DIContainers;
using ObservableCollections;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityMusic : FacilityBase
{
	private enum MainState
	{
		Idle,
		Playing,
		Pause
	}

	[Inject]
	private SettingService _settingService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private IMusicPlayerUI[] _musicPlayerUIs;

	[Inject]
	private IMusicTagListShowController _tagListShowController;

	[Inject]
	private IMusicPlayListShowController _musicPlayListShowController;

	[Inject]
	private IMusicTabUI _musicTabUI;

	[Inject]
	private IMusicListUI _musicListUI;

	[Inject]
	private IMusicImportFailedView _musicImportFailedView;

	private MainState _mainState;

	private MusicService _musicService;

	private ReactiveProperty<bool> _isActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> _importGate = new ReactiveProperty<bool>(value: true);

	private Subject<Unit> _onCloseFromCloseButton = new Subject<Unit>();

	public bool IsPaused => _mainState == MainState.Pause;

	public MusicService MusicService => _musicService;

	public GameAudioInfo PlayingMusic => _musicService.PlayingMusic;

	public ReadOnlyReactiveProperty<bool> IsActive => _isActive;

	public Observable<Unit> OnCloseFromCloseButton => _onCloseFromCloseButton;

	public void Setup()
	{
		if (_musicService == null)
		{
			_musicService = ProjectLifetimeScope.Resolve<MusicService>();
		}
		_musicService.Setup();
		_musicService.OnChangeMusic.Subscribe(OnChangeMusic).AddTo(this);
		_musicImportFailedView.Setup(_musicService);
		ObservableSubscribeExtensions.Subscribe(_musicService.OnPlayMusic, delegate
		{
			_mainState = MainState.Playing;
			ChangePlayUI();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_musicService.OnPauseMusic, delegate
		{
			_mainState = MainState.Pause;
			ChangePauseUI();
		}).AddTo(this);
		bool isPlay = true;
		_musicListUI.Setup(_musicService.CurrentPlayList, this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickSwitchMuteButton != null
			select _.OnClickSwitchMuteButton).Merge(), delegate
		{
			_settingService.ChangeAudioMixerSwitchMute(AudioMixerType.Music);
		}).AddTo(this);
		(from _ in _musicPlayerUIs
			where _.OnChangeVolume != null
			select _.OnChangeVolume).Merge().Subscribe(delegate(float volume)
		{
			_settingService.ChangeAudioMixerVolume(AudioMixerType.Music, volume);
			if (volume <= 0f)
			{
				if (!SaveDataManager.Instance.SettingData.GetVolumeInfo(AudioMixerType.Music).IsMute.Value)
				{
					_settingService.ActivateAudioMixerMute(AudioMixerType.Music);
				}
			}
			else if (SaveDataManager.Instance.SettingData.GetVolumeInfo(AudioMixerType.Music).IsMute.Value)
			{
				_settingService.DeactivateAudioMixerMute(AudioMixerType.Music);
			}
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.MusicVolumeInfo.IsMute.Subscribe(delegate(bool isMute)
		{
			IMusicPlayerUI[] musicPlayerUIs2 = _musicPlayerUIs;
			for (int i = 0; i < musicPlayerUIs2.Length; i++)
			{
				musicPlayerUIs2[i].SetMute(isMute);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickPlayOrPauseButton != null
			select _.OnClickPlayOrPauseButton).Merge(), delegate
		{
			OnClickButtonPlayOrPauseMusic();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickNextButton != null
			select _.OnClickNextButton).Merge(), delegate
		{
			OnClickButtonSkip();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickBackButton != null
			select _.OnClickBackButton).Merge(), delegate
		{
			OnClickButtonBack();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickShuffleButton != null
			select _.OnClickShuffleButton).Merge(), delegate
		{
			OnClickButtonShuffleChange();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe((from _ in _musicPlayerUIs
			where _.OnClickLoopButton != null
			select _.OnClickLoopButton).Merge(), delegate
		{
			OnClickButtonChangeLoop();
		}).AddTo(this);
		(from _ in _musicPlayerUIs
			where _.OnChangeProgress != null
			select _.OnChangeProgress).Merge().Subscribe(delegate(float progress)
		{
			_musicService.SetMusicProgress(progress);
		}).AddTo(this);
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int num = 0; num < musicPlayerUIs.Length; num++)
		{
			musicPlayerUIs[num].Setup(isPlay, _musicService.IsRepeatOneMusic);
		}
		if (SaveDataManager.Instance.MusicSetting.IsShufflePlayMusic)
		{
			_musicService.SetShuffle(SaveDataManager.Instance.MusicSetting.IsShufflePlayMusic);
			ChangeShuffleUI(_musicService.IsShuffle);
		}
		_musicService.PlayNextMusic(0, MusicChangeKind.Auto).Forget();
		_musicService.Pause();
		ChangePauseUI();
		_mainState = MainState.Pause;
		ObservableSubscribeExtensions.Subscribe(_musicPlayListShowController.OnClickListCloseButton, delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
			_onCloseFromCloseButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_musicTabUI.OnClickTagButton, delegate
		{
			OnClickButtonOpenOrCloseTagListPullDown();
		}).AddTo(this);
		_musicTabUI.OnClickFileImportButton.SubscribeAwait(async delegate
		{
			await ImportLocalMusicAsync(isFolder: false);
		}, _importGate).AddTo(this);
		if (_musicTabUI.OnClickFolderImportButton != null)
		{
			_musicTabUI.OnClickFolderImportButton.SubscribeAwait(async delegate
			{
				await ImportLocalMusicAsync(isFolder: true);
			}, _importGate).AddTo(this);
		}
		_importGate.Subscribe(delegate(bool canImport)
		{
			_musicTabUI.SetInteractableFileImport(canImport);
			_musicTabUI.SetInteractableFolderImport(canImport);
		}).AddTo(this);
		Deactivate();
	}

	public void UpdateFacility()
	{
		switch (_mainState)
		{
		}
		if (PlayingMusic == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(PlayingMusic.AudioClip) != null)
		{
			GameAudioInfo audio = PlayingMusic;
			AudioClip audioClip = audio.AudioClip;
			AudioPlayer player = SingletonMonoBehaviour<MusicManager>.Instance.GetPlayer(audioClip);
			if (audio.PathType == AudioMode.LocalPc && (bool)audioClip && !player.AudioSource.isPlaying && player.CurrentState == AudioPlayer.State.Playing && Mathf.Approximately(audioClip.length, 0f) && Mathf.Approximately(player.AudioSource.time, 0f))
			{
				UniTask.Void(async delegate(CancellationToken ct)
				{
					await audio.ReloadLocalAudio(ct);
					player.AudioSource.clip = audio.AudioClip;
					if (PlayingMusic != null && PlayingMusic.UUID == audio.UUID && !player.AudioSource.clip)
					{
						await _musicService.SkipCurrentMusic(MusicChangeKind.Auto);
					}
				}, base.destroyCancellationToken);
			}
			UpdateProgressBarUI(_musicService.GetCurrentMusicProgress());
		}
		else if (_mainState == MainState.Playing)
		{
			PauseMusic();
		}
	}

	public void Release()
	{
		_musicService.Dispose();
	}

	private void ChangePauseUI()
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].OnPauseMusic();
		}
	}

	private void ChangePlayUI()
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].OnPlayMusic();
		}
	}

	private void ChangeShuffleUI(bool flag)
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].OnChangeShuffle(flag);
		}
	}

	private void ChangeLoopUI(bool flag)
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].OnChangeLoop(flag);
		}
	}

	private void ChangeMusicUI(string musicName, string artistName, MusicChangeKind kind)
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].OnChangeMusic(musicName, artistName, kind);
		}
	}

	private void UpdateProgressBarUI(float amount)
	{
		IMusicPlayerUI[] musicPlayerUIs = _musicPlayerUIs;
		for (int i = 0; i < musicPlayerUIs.Length; i++)
		{
			musicPlayerUIs[i].UpdateProgressBar(amount);
		}
	}

	public void OnClickButtonPlayOrPauseMusic()
	{
		switch (_mainState)
		{
		case MainState.Playing:
			PauseMusic();
			break;
		case MainState.Pause:
			UnPauseMusic();
			break;
		}
	}

	public void PauseMusic()
	{
		_musicService.Pause();
		ChangePauseUI();
		_mainState = MainState.Pause;
		SaveDataManager.Instance.MusicSetting.IsGameStartPlayMusic = false;
		SaveDataManager.Instance.SaveMusicSetting();
	}

	public void UnPauseMusic()
	{
		_musicService.UnPause();
		ChangePlayUI();
		_mainState = MainState.Playing;
		SaveDataManager.Instance.MusicSetting.IsGameStartPlayMusic = true;
		SaveDataManager.Instance.SaveMusicSetting();
	}

	public void PlayMusic(int index)
	{
		ObservableList<GameAudioInfo> currentPlayList = _musicService.CurrentPlayList;
		if (currentPlayList.InBounded(index))
		{
			if (_musicService.IsShuffle)
			{
				_musicService.SetShuffle(isShuffle: false);
				ChangeShuffleUI(flag: false);
			}
			_musicService.PlayArugumentMusic(currentPlayList[index], MusicChangeKind.Manual);
			ChangePlayUI();
			_mainState = MainState.Playing;
		}
	}

	public void PlayShuffle()
	{
		ObservableList<GameAudioInfo> currentPlayList = _musicService.CurrentPlayList;
		if (currentPlayList.Count >= 1)
		{
			_musicService.SetShuffle(isShuffle: true);
			_musicService.PlayArugumentMusic(currentPlayList[0], MusicChangeKind.Manual);
			ChangeShuffleUI(_musicService.IsShuffle);
			ChangePlayUI();
			_mainState = MainState.Playing;
		}
	}

	private void OnChangeMusic(MusicChangeKind changeKind)
	{
		string musicName = string.Empty;
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(_musicService.PlayingMusic.Title))
		{
			musicName = ((!DevicePlatform.Steam.IsMobile()) ? _musicService.PlayingMusic.Title : _musicService.PlayingMusic.GetPlatformAudioTitle());
		}
		else
		{
			Debug.LogError("楽曲名が設定されていません。");
		}
		empty = (string.IsNullOrEmpty(_musicService.PlayingMusic.Credit) ? "" : ((!DevicePlatform.Steam.IsMobile()) ? _musicService.PlayingMusic.Credit : _musicService.PlayingMusic.GetPlatformCredit()));
		ChangeMusicUI(musicName, empty, changeKind);
	}

	public void OnClickButtonSkip()
	{
		UniTask.Void(async delegate
		{
			if (_musicService.CurrentPlayList.Count > 0)
			{
				_mainState = MainState.Playing;
				await _musicService.SkipCurrentMusic(MusicChangeKind.Manual);
			}
		});
	}

	public void OnClickButtonBack()
	{
		UniTask.Void(async delegate
		{
			_mainState = MainState.Playing;
			ChangePlayUI();
			if (!(await _musicService.PlayBackMusic()))
			{
				_mainState = MainState.Pause;
				ChangePauseUI();
			}
		});
	}

	public void OnClickButtonChangeLoop()
	{
		bool flag = !_musicService.IsRepeatOneMusic;
		_musicService.SetRepeat(flag);
		ChangeLoopUI(flag);
	}

	public void OnClickButtonShuffleChange()
	{
		bool shuffle = !_musicService.IsShuffle;
		_musicService.SetShuffle(shuffle);
		ChangeShuffleUI(_musicService.IsShuffle);
	}

	public void OnClickButtonPlayListPlayMusicButton(GameAudioInfo audioInfo)
	{
		_mainState = MainState.Playing;
		ChangePlayUI();
		_musicService.PlayArugumentMusic(audioInfo, MusicChangeKind.PlaylistCellClick);
		SaveDataManager.Instance.MusicSetting.IsGameStartPlayMusic = true;
		SaveDataManager.Instance.SaveMusicSetting();
	}

	public void Activate()
	{
		_isActive.Value = true;
		_musicPlayListShowController.ActivatePlayList();
	}

	public void Deactivate()
	{
		_isActive.Value = false;
		_musicPlayListShowController.DeactivatePlayList();
	}

	private async UniTask ImportLocalMusicAsync(bool isFolder)
	{
		if (_musicService.AllMusicList.Count((GameAudioInfo m) => m.PathType == AudioMode.LocalPc) >= 100)
		{
			_musicImportFailedView.ShowImportImpossibleLimitAnnounce();
			return;
		}
		bool isAddedNewAudio = false;
		bool isLimitFailed = false;
		bool isImportedFailed = false;
		bool isInvalidFailed = false;
		foreach (GameAudioInfo item in await GameAudioInfo.LoadLocalFiles(isFolder, Application.exitCancellationToken))
		{
			if (item.AudioClip == null)
			{
				isInvalidFailed = true;
				continue;
			}
			if (_musicService.AllMusicList.Count((GameAudioInfo m) => m.PathType == AudioMode.LocalPc) >= 100)
			{
				isLimitFailed = true;
				break;
			}
			if (_musicService.AddLocalMusicItem(item))
			{
				SaveDataManager.Instance.LocalMusicSetting.LocalAudioDatas.Add(new LocalAudioData(item.LocalPath, item.UUID));
				isAddedNewAudio = true;
			}
			else
			{
				isImportedFailed = true;
			}
		}
		SaveDataManager.Instance.SaveLocalMusicSetting();
		if (isAddedNewAudio)
		{
			_systemSeService.PlayClick();
		}
		if (isLimitFailed || isImportedFailed || isInvalidFailed)
		{
			_musicImportFailedView.ShowImportFailedAnnounce(isLimitFailed, isImportedFailed, isInvalidFailed, isFolder);
		}
	}

	public void OnClickButtonOpenOrCloseTagListPullDown()
	{
		_tagListShowController.ToggleMusicTagList();
	}
}
