using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class NoteContentsListPageView : MonoBehaviour
{
	[SerializeField]
	private Button _addNoteButton;

	[SerializeField]
	private CanvasGroup _addNoteDeactivateObj;

	[SerializeField]
	private ToggleStyleButton _toggleRemoving;

	[SerializeField]
	private NoteContentListView _listView;

	private Subject<Unit> _onChangeInputPage = new Subject<Unit>();

	public Observable<Unit> OnClickAddNoteButton => _addNoteButton.OnClickAsObservable();

	public Observable<(ulong, bool, bool)> OnClickSelectButton => _listView.OnClickSelectButton;

	public Observable<ulong> OnClickRemoveButton => _listView.OnClickRemoveButton;

	public Observable<Unit> OnChangeInputPage => _onChangeInputPage;

	public Observable<(ulong, ulong)> OnChangeOrderForReasonDragged => _listView.OnChangeOrderForReasonDragged;

	public void Setup()
	{
		_toggleRemoving.OnValueChanged.Subscribe(delegate(bool isRemoving)
		{
			SetChangeRemoveMode(isRemoving);
			_listView.ChangeRemovingMode(isRemoving);
		}).AddTo(this);
	}

	public void EnterSetting(bool isRemovingMode)
	{
		_toggleRemoving.SetToggleWithoutTransition(isRemovingMode, isNotify: false);
		SetChangeRemoveMode(isRemovingMode);
		_listView.ChangeRemovingMode(isRemovingMode, isImmediate: true);
	}

	public void AddNoteModel(NoteContentListItemModel model)
	{
		_listView.AddNoteItemModel(model);
	}

	public void SetupNoteModel(NoteContentListItemModel[] models)
	{
		_listView.SetupNoteModel(models);
	}

	public void ChangeTitleModelData(string title, ulong pageUniqueID)
	{
		_listView.ChangeTitleModelData(title, pageUniqueID);
	}

	public void RemoveNoteModel(ulong pageUniqueID)
	{
		_listView.RemoveNoteItemModel(pageUniqueID);
	}

	public void SelectNote(ulong pageUniqueID, bool isStartInput, bool isTargetTitle)
	{
		_listView.SelectNote(pageUniqueID, isStartInput, isTargetTitle);
	}

	public void SetActiveToggleRemoving(bool active)
	{
		if (!active && _toggleRemoving.IsOn)
		{
			_toggleRemoving.SetToggleWithoutTransition(active, isNotify: true);
		}
		if (_toggleRemoving.gameObject.activeSelf != active)
		{
			_toggleRemoving.gameObject.SetActive(active);
		}
	}

	private void SetChangeRemoveMode(bool isRemoveMode)
	{
		_addNoteButton.enabled = !isRemoveMode;
		_addNoteButton.interactable = !isRemoveMode;
		_addNoteDeactivateObj.gameObject.SetActive(isRemoveMode);
		_addNoteDeactivateObj.alpha = (isRemoveMode ? 1 : 0);
	}
}
