using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityNoteContentsUIView : MonoBehaviour
{
	[Inject]
	private NoteService _noteService;

	[Inject]
	private LocalizationMasterWrapper _localizationMasterWrapper;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private NoteUIContentsView _noteUIContentsView;

	[SerializeField]
	private NoteContentsListPageView _noteContentsListPageView;

	[SerializeField]
	private NoteContentsInputPageView _noteContentsInputPageView;

	public void Setup()
	{
		_noteUIContentsView.Setup();
		_noteContentsListPageView.Setup();
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		if (SaveDataManager.Instance.NoteData.NoteList.Titles.Count == 0)
		{
			_noteService.OnAddPage.Subscribe(delegate(PageDataV2 data)
			{
				ChangeInitTitle(data);
			}).AddTo(cancellationTokenSource.Token);
		}
		_noteService.AddSelectPageUIForSaveData();
		SetupListFromSaveData();
		cancellationTokenSource.Cancel();
		cancellationTokenSource.Dispose();
		_noteUIContentsView.OnPrepare.Subscribe(delegate(bool fromTab)
		{
			_noteContentsListPageView.SetActiveToggleRemoving(CheckNoteMoreThanTwo());
			if (!fromTab)
			{
				SetupListFromSaveData();
				_noteUIContentsView.ChangeNoteListView(isImmediate: true);
			}
			_noteUIContentsView.EnterSetting(isRemovingMode: false);
		}).AddTo(this);
		_noteContentsListPageView.OnClickSelectButton.Subscribe(delegate((ulong, bool, bool) data)
		{
			_systemSeService.PlaySelect();
			_noteService.ChangePage(data.Item1);
			string pageTitle = _noteService.GetPageTitle(data.Item1);
			string mainText = _noteService.GetPageSaveData(data.Item1).MainText;
			_noteContentsInputPageView.InputFieldsView.SetText(pageTitle, mainText);
			_noteUIContentsView.ChangeInputView(data.Item2, data.Item3);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noteContentsInputPageView.OnClickReturnButton, delegate
		{
			_systemSeService.PlaySelect();
			ulong currentValue = _noteService.CurrentPageID.CurrentValue;
			string pageTitle = _noteService.GetPageTitle(currentValue);
			_noteContentsListPageView.ChangeTitleModelData(pageTitle, currentValue);
			_noteUIContentsView.ChangeNoteListView();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_noteContentsListPageView.OnClickAddNoteButton, delegate
		{
			_noteService.AddSelectPageUI(isPlaySE: false);
			_noteContentsListPageView.SetActiveToggleRemoving(CheckNoteMoreThanTwo());
		}).AddTo(this);
		_noteService.OnAddPage.Subscribe(delegate(PageDataV2 data)
		{
			string title = ChangeInitTitle(data);
			_noteContentsListPageView.AddNoteModel(new NoteContentListItemModel
			{
				Title = title,
				pageUniqueID = data.UniqueID
			});
			_noteContentsListPageView.SelectNote(data.UniqueID, isStartInput: true, isTargetTitle: true);
		}).AddTo(this);
		_noteContentsListPageView.OnClickRemoveButton.Subscribe(delegate(ulong pageUniqueID)
		{
			_noteService.RemovePage(pageUniqueID);
			_noteContentsListPageView.SetActiveToggleRemoving(CheckNoteMoreThanTwo());
		}).AddTo(this);
		_noteService.OnRemovePage.Subscribe(delegate(ulong uniqueID)
		{
			_noteContentsListPageView.RemoveNoteModel(uniqueID);
		}).AddTo(this);
		_noteContentsInputPageView.InputFieldsView.OnEndEditTitle.Subscribe(delegate(string title)
		{
			if (title.IsNullOrEmpty())
			{
				_localizationMasterWrapper.TryGet("ui_note_init_title", out title);
				_noteContentsInputPageView.InputFieldsView.SetTitle(title);
			}
			_noteService.ChangeEndTitle(_noteService.CurrentPageID.CurrentValue, title);
		}).AddTo(this);
		_noteContentsInputPageView.InputFieldsView.OnEndEditMain.Subscribe(delegate(string main)
		{
			_noteService.ChangeEndMainText(main);
		}).AddTo(this);
		_noteContentsListPageView.OnChangeOrderForReasonDragged.Subscribe(delegate((ulong, ulong) ids)
		{
			_noteService.SwapAfter(ids.Item1, ids.Item2);
		}).AddTo(this);
	}

	public void SetupListFromSaveData()
	{
		List<ulong> pageOrderList = SaveDataManager.Instance.NoteData.NoteList.PageOrderList;
		int count = pageOrderList.Count;
		NoteContentListItemModel[] array = ((count == 0) ? null : new NoteContentListItemModel[count]);
		for (int i = 0; i < count; i++)
		{
			ulong num = pageOrderList[i];
			if (SaveDataManager.Instance.NoteData.NoteList.Titles.TryGetValue(num, out var value))
			{
				array[i] = new NoteContentListItemModel
				{
					Title = value,
					pageUniqueID = num
				};
			}
		}
		_noteContentsListPageView.SetupNoteModel(array);
	}

	private bool CheckNoteMoreThanTwo()
	{
		return SaveDataManager.Instance.NoteData.NoteList.PageOrderList.Count >= 2;
	}

	private string ChangeInitTitle(PageDataV2 data)
	{
		_localizationMasterWrapper.TryGet("ui_note_init_title", out var result);
		_noteService.ChangeEndTitle(data.UniqueID, result);
		return result;
	}
}
