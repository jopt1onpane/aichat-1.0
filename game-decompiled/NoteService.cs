using System;
using System.Collections.Generic;
using Bulbul;
using R3;
using VContainer;

public class NoteService : IDisposable
{
	[Inject]
	private SystemSeService _systemSeService;

	private readonly List<PageDataV2> _selectPageUIList = new List<PageDataV2>();

	private ReactiveProperty<ulong> _currentPageID = new ReactiveProperty<ulong>();

	private Subject<(ulong, string)> _onChangeTitle = new Subject<(ulong, string)>();

	private Subject<PageDataV2> _onAddPage = new Subject<PageDataV2>();

	private Subject<ulong> _onRemovePage = new Subject<ulong>();

	private Subject<Unit> _onCloseNote = new Subject<Unit>();

	private Subject<Unit> _onEndEditNote = new Subject<Unit>();

	private bool _isFinishedInitialize;

	public ReadOnlyReactiveProperty<ulong> CurrentPageID => _currentPageID;

	public Observable<(ulong pageID, string title)> OnChangeTitle => _onChangeTitle;

	public Observable<PageDataV2> OnAddPage => _onAddPage;

	public Observable<ulong> OnRemovePage => _onRemovePage;

	public Observable<Unit> OnCloseNote => _onCloseNote;

	public Observable<Unit> OnEndEditNote => _onEndEditNote;

	public bool IsFinishedInitialize => _isFinishedInitialize;

	public void Dispose()
	{
	}

	public PageDataV2 GetPageSaveData(ulong pageID)
	{
		return SaveDataManager.Instance.NoteData.GetPageData(pageID);
	}

	public string GetPageTitle(ulong pageID)
	{
		if (SaveDataManager.Instance.NoteData.NoteList.Titles.TryGetValue(pageID, out var value))
		{
			return value;
		}
		return string.Empty;
	}

	public void SelectFirstPage()
	{
		if (_selectPageUIList.Count > 0)
		{
			ChangePage(_selectPageUIList[0].UniqueID);
		}
	}

	public void AddSelectPageUI(bool isPlaySE = true)
	{
		PageDataV2 pageDataV = new PageDataV2();
		if (!SaveDataManager.Instance.NoteData.AddPage(pageDataV))
		{
			Debug.LogError("すでに同じIDのPageが登録されています！通常通らないはず");
			return;
		}
		if (isPlaySE)
		{
			_systemSeService.PlayClick();
		}
		SaveDataManager.Instance.SaveNoteList();
		_onAddPage.OnNext(pageDataV);
		_selectPageUIList.Add(pageDataV);
	}

	public void AddPage(string title, string body)
	{
		PageDataV2 pageDataV = new PageDataV2();
		pageDataV.MainText = body;
		if (!SaveDataManager.Instance.NoteData.AddPage(pageDataV))
		{
			Debug.LogError("すでに同じIDのPageが登録されています！通常通らないはず");
			return;
		}
		SaveDataManager.Instance.NoteData.SetTitleText(pageDataV.UniqueID, title);
		_onAddPage.OnNext(pageDataV);
		_selectPageUIList.Add(pageDataV);
	}

	public void AddSelectPageUIForSaveData()
	{
		if (SaveDataManager.Instance.NoteData.NoteList.Titles.Count > 0)
		{
			foreach (ulong pageOrder in SaveDataManager.Instance.NoteData.NoteList.PageOrderList)
			{
				if (SaveDataManager.Instance.NoteData.NoteList.Titles.ContainsKey(pageOrder))
				{
					PageDataV2 pageDataV = new PageDataV2
					{
						UniqueID = pageOrder
					};
					_onAddPage.OnNext(pageDataV);
					_selectPageUIList.Add(pageDataV);
				}
			}
		}
		else
		{
			AddSelectPageUI(isPlaySE: false);
		}
		_isFinishedInitialize = true;
	}

	public void ChangeTitle(ulong pageID, string inputText)
	{
		_onChangeTitle.OnNext((pageID, inputText));
	}

	public void ChangeEndTitle(ulong pageID, string inputText)
	{
		SaveDataManager.Instance.NoteData.SetTitleText(pageID, inputText);
		SaveDataManager.Instance.SaveNoteList();
	}

	public void ChangePage(ulong pageID)
	{
		_currentPageID.Value = pageID;
	}

	public void RemovePage(ulong pageID)
	{
		PageDataV2 pageDataV = null;
		for (int i = 0; i < _selectPageUIList.Count; i++)
		{
			if (_selectPageUIList[i].UniqueID == pageID)
			{
				pageDataV = _selectPageUIList[i];
			}
		}
		if (pageDataV != null)
		{
			_selectPageUIList.Remove(pageDataV);
			_systemSeService.PlayCancel();
			SaveDataManager.Instance.NoteData.RemovePage(pageID);
			SaveDataManager.Instance.SaveNoteList();
			_onRemovePage.OnNext(pageID);
		}
		else
		{
			Debug.LogError($"ページの削除：指定されたID{pageID}が見つかりませんでした。");
		}
	}

	public void ChangeEndMainText(string inputText)
	{
		_onEndEditNote.OnNext(Unit.Default);
		if (SaveDataManager.Instance.NoteData.SetMainText(CurrentPageID.CurrentValue, inputText, out var pageData))
		{
			SaveDataManager.Instance.SavePageData(pageData);
		}
	}

	public void CloseNote()
	{
		_onCloseNote.OnNext(Unit.Default);
	}

	public void SwapAfter(ulong target, ulong origin)
	{
		SaveDataManager.Instance.NoteData.SwapPage(target, origin);
		SaveDataManager.Instance.SaveNoteList();
	}
}
