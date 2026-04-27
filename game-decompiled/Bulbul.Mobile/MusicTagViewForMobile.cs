using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class MusicTagViewForMobile : MonoBehaviour, IMusicTabUI, IMusicUIBase, IMusicTagListShowController
{
	[SerializeField]
	private MusicTagListUI _tagListUI;

	[SerializeField]
	private MusicImportButton _fileImportButton;

	[SerializeField]
	private Button _tagButton;

	public Observable<Unit> OnClickFileImportButton => _fileImportButton.OnClickAsObservable();

	public Observable<Unit> OnClickFolderImportButton => null;

	public Observable<Unit> OnClickTagButton => _tagButton.OnClickAsObservable();

	public void Setup()
	{
		_tagListUI.Setup();
	}

	public void ButtonsSetCheck(AudioTag? setTag = null)
	{
		_tagListUI.ButtonsSetCheck(setTag);
	}

	public void CloseMusicTagList(bool immediate = false)
	{
		_tagListUI.CloseMusicTagList(immediate);
	}

	public void OpenMusicTagList()
	{
		_tagListUI.OpenMusicTagList();
	}

	public void ToggleMusicTagList()
	{
		_tagListUI.ToggleMusicTagList();
	}

	public void SetInteractableFileImport(bool enable)
	{
		_fileImportButton.SetInteractable(enable);
	}

	public void SetInteractableFolderImport(bool enable)
	{
	}
}
