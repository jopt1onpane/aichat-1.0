using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicPlayListUIForMobile : MonoBehaviour, IMusicListUI, IMusicUIBase, IMusicPlayListShowController
{
	[SerializeField]
	private MusicPlayListView _musicPlayListView;

	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _switchRemovingModeButton;

	[SerializeField]
	private Button _switchNormalModeButton;

	[SerializeField]
	private MusicTagViewForMobile _tabView;

	private FacilityMusic _facilityMusic;

	private bool _isPlayListDirty;

	private bool _isRemovingMode;

	private MusicPlayListButtonForMobile[] buttons;

	Observable<Unit> IMusicPlayListShowController.OnClickListCloseButton => _closeButton.OnClickAsObservable();

	private void Start()
	{
		ObservableSubscribeExtensions.Subscribe(_switchNormalModeButton.OnClickAsObservable(), delegate
		{
			_switchNormalModeButton.gameObject.SetActive(value: false);
			_switchRemovingModeButton.gameObject.SetActive(value: true);
			_isRemovingMode = false;
			MusicPlayListButtonForMobile[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchMode(_isRemovingMode);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_switchRemovingModeButton.OnClickAsObservable(), delegate
		{
			_switchNormalModeButton.gameObject.SetActive(value: true);
			_switchRemovingModeButton.gameObject.SetActive(value: false);
			_isRemovingMode = true;
			MusicPlayListButtonForMobile[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchMode(_isRemovingMode);
			}
		}).AddTo(this);
	}

	void IMusicListUI.Setup(IReadOnlyObservableList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic)
	{
		_tabView.Setup();
		_facilityMusic = facilityMusic;
		_musicPlayListView.Setup(facilityMusic, audioInfoList);
		ObservableSubscribeExtensions.Subscribe(audioInfoList.ObserveCountChanged(), delegate
		{
			_isPlayListDirty = true;
		}).AddTo(this);
		_musicPlayListView.ViewPlayList();
		buttons = _musicPlayListView.CreateButtonArray<MusicPlayListButtonForMobile>();
	}

	private void LateUpdate()
	{
		if (_isPlayListDirty)
		{
			_musicPlayListView.ViewPlayList();
			buttons = _musicPlayListView.CreateButtonArray<MusicPlayListButtonForMobile>();
			MusicPlayListButtonForMobile[] array = buttons;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SwitchMode(_isRemovingMode, isImmediate: true);
			}
			_isPlayListDirty = false;
		}
	}

	void IMusicPlayListShowController.ActivatePlayList()
	{
		_root.SetActive(value: true);
		GameAudioInfo playingMusic = _facilityMusic.PlayingMusic;
		if (playingMusic != null)
		{
			_musicPlayListView.ScrollToPlayingMusic(playingMusic.Title);
		}
	}

	void IMusicPlayListShowController.DeactivatePlayList()
	{
		_root.SetActive(value: false);
	}
}
