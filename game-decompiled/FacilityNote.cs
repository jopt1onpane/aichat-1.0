using Bulbul;
using UnityEngine;
using VContainer;

public class FacilityNote : MonoBehaviour
{
	private enum MainState
	{
		Idle,
		EditPageTitleText,
		EditPageMainText
	}

	[Inject]
	private NoteService _noteService;

	[Inject]
	private INoteUI _noteUI;

	private MainState _mainState;

	public void Setup()
	{
		_noteUI.Setup();
		_noteService.AddSelectPageUIForSaveData();
		_noteService.SelectFirstPage();
		Deactivate();
	}

	public void UpdateFacility()
	{
		switch (_mainState)
		{
		case MainState.Idle:
			_noteUI.UpdateUI();
			break;
		case MainState.EditPageTitleText:
		case MainState.EditPageMainText:
			break;
		}
	}

	public bool IsActive()
	{
		return _noteUI.IsActive();
	}

	public void Activate()
	{
		_noteUI.Activate();
	}

	public void Deactivate()
	{
		_noteUI.Deactivate();
	}

	public void InitializePosition()
	{
		_noteUI.InitializePosition();
	}
}
