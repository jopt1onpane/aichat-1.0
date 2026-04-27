using System.Linq;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayListUIMobile : MonoBehaviour, IMusicListUI, IMusicUIBase, IMusicPlayListShowController
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private FacilityCommonActivateAnimationMobile _facilityAnimation;

	[SerializeField]
	private MusicPlayListViewMobile _playList;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private ToggleStyleButton _removeModeToggle;

	[SerializeField]
	private Button _switchRemovingModeButton;

	[SerializeField]
	private Button _switchNormalModeButton;

	[SerializeField]
	private MusicTagViewForMobile _tabView;

	[Header("タグプルダウンUIの操作補助")]
	[SerializeField]
	private PulldownListUI _tagViewPulldown;

	[SerializeField]
	private ClickOutsideDetector _tagViewOutsideDetector;

	[SerializeField]
	private Canvas _tagViewPulldownCanvas;

	[SerializeField]
	private GameObject _tagViewRaycastBlocker;

	[Header("削除モード中に使用出来ないUIを塞ぐオブジェクト")]
	[SerializeField]
	private GameObject _tagListInactiveImage;

	[SerializeField]
	private GameObject _importInactiveImage;

	[SerializeField]
	private TMP_Text importMusicCountText;

	private FacilityMusic _facilityMusic;

	private bool _lastCheckedUpgradePassPurchasedState;

	Observable<Unit> IMusicPlayListShowController.OnClickListCloseButton => _closeButton.OnClickAsObservable();

	void IMusicListUI.Setup(IReadOnlyObservableList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic)
	{
	}

	void IMusicPlayListShowController.ActivatePlayList()
	{
		_root.SetActive(value: true);
		_facilityAnimation.Activate();
		_tabView.ButtonsSetCheck();
		_tagListInactiveImage.SetActive(value: false);
		_importInactiveImage?.SetActive(value: false);
		_tabView.SetInteractableFileImport(enable: true);
		_playList.SetupMusicModel(_facilityMusic.MusicService.CurrentPlayList, isResetRemovingMode: true);
		_removeModeToggle.SetToggleWithoutTransition(isOn: false, isNotify: false);
		_playList.SetPlayListMode(isRemoving: false, isImmediate: true);
		GameAudioInfo playingMusic = _facilityMusic.PlayingMusic;
		if (playingMusic != null)
		{
			_playList.ScrollToPlayingMusic(playingMusic.UUID);
		}
	}

	void IMusicPlayListShowController.DeactivatePlayList()
	{
		_facilityAnimation.Deactivate();
	}

	private void UpdateRemoveToggleVisibility()
	{
		bool flag = _facilityMusic.MusicService.AllMusicList.Any((GameAudioInfo x) => x.PathType == AudioMode.LocalPc);
		_removeModeToggle.gameObject.SetActive(flag);
		if (!flag)
		{
			ForceExitRemoveMode();
		}
	}

	private void ForceExitRemoveMode()
	{
		_removeModeToggle.SetToggleWithoutTransition(isOn: false, isNotify: false);
		_playList.SetPlayListMode(isRemoving: false);
		_playList.SetupMusicModel(_facilityMusic.MusicService.CurrentPlayList);
		_tabView.ButtonsSetCheck();
		_tagListInactiveImage?.SetActive(value: false);
		_importInactiveImage?.SetActive(value: false);
		_tabView.SetInteractableFileImport(enable: true);
	}
}
