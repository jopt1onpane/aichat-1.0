using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class MusicPlayListTabUI : MonoBehaviour, IMusicPlayerUI, IMusicUIBase, IMusicTabUI
{
	[Inject]
	private MusicService _musicService;

	[SerializeField]
	private Sprite favoriteSprite;

	[SerializeField]
	private Sprite allSprite;

	[Header("再生ボタン")]
	[SerializeField]
	private Image playOrPauseButtonImage;

	[SerializeField]
	private Sprite pauseButtonSprite;

	[SerializeField]
	private Sprite playButtonSprite;

	[Header("シャッフルボタン")]
	[SerializeField]
	private Image shuffleChangeButtonImage;

	[SerializeField]
	private Sprite enableShuffleSprite;

	[SerializeField]
	private Sprite disableShuffleSprite;

	[Header("ボタン")]
	[SerializeField]
	private Button _playOrPauseButton;

	[SerializeField]
	private Button _shuffleButton;

	[SerializeField]
	private Button _tagButton;

	[SerializeField]
	private MusicImportButton _fileImportButton;

	[SerializeField]
	private MusicImportButton _folderImportButton;

	public Observable<Unit> OnClickFileImportButton => _fileImportButton.OnClickAsObservable();

	public Observable<Unit> OnClickFolderImportButton => _folderImportButton.OnClickAsObservable();

	public Observable<Unit> OnClickTagButton => _tagButton.OnClickAsObservable();

	public Observable<Unit> OnClickSwitchMuteButton => null;

	public Observable<float> OnChangeVolume => null;

	public Observable<Unit> OnClickPlayOrPauseButton => _playOrPauseButton.OnClickAsObservable();

	public Observable<Unit> OnClickShuffleButton => _shuffleButton.OnClickAsObservable();

	public Observable<Unit> OnClickNextButton => null;

	public Observable<Unit> OnClickBackButton => null;

	public Observable<Unit> OnClickLoopButton => null;

	Observable<float> IMusicPlayerUI.OnChangeProgress => null;

	public void OnPauseMusic()
	{
		playOrPauseButtonImage.sprite = playButtonSprite;
	}

	public void OnPlayMusic()
	{
		playOrPauseButtonImage.sprite = pauseButtonSprite;
	}

	public void OnChangeShuffle(bool isShuffle)
	{
		shuffleChangeButtonImage.sprite = (isShuffle ? enableShuffleSprite : disableShuffleSprite);
	}

	public void SetInteractableFileImport(bool enable)
	{
		_fileImportButton.SetInteractable(enable);
	}

	public void SetInteractableFolderImport(bool enable)
	{
		_folderImportButton.SetInteractable(enable);
	}

	public void OnChangeLoop(bool isLoop)
	{
	}

	public void OnChangeMusic(string musicName, string artistName, MusicChangeKind kind)
	{
	}

	public void UpdateProgressBar(float amount)
	{
	}

	public void Setup(bool isPlay, bool isLoop)
	{
	}

	public void SetMute(bool isMute)
	{
	}
}
