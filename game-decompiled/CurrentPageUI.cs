using System.Threading;
using Bulbul;
using R3;
using TMPro;
using UnityEngine;
using VContainer;

public class CurrentPageUI : MonoBehaviour
{
	[Inject]
	private NoteService _noteService;

	[SerializeField]
	[Header("タイトル")]
	private TMP_InputField _titleInputField;

	[SerializeField]
	[Header("本文")]
	private TMP_InputField _mainInputField;

	private CancellationTokenSource pageLoadCancellation = new CancellationTokenSource();

	public void Setup()
	{
		_mainInputField.OnEndEditAsObservable().Subscribe(delegate(string inputText)
		{
			_noteService.ChangeEndMainText(inputText);
		}).AddTo(this);
		_noteService.CurrentPageID.Subscribe(delegate(ulong pageID)
		{
			_mainInputField.interactable = false;
			using (Disposable.Create(_mainInputField, delegate(TMP_InputField x)
			{
				x.interactable = true;
			}))
			{
				pageLoadCancellation?.Cancel();
				pageLoadCancellation = new CancellationTokenSource();
				PageDataV2 pageSaveData = _noteService.GetPageSaveData(pageID);
				if (pageSaveData != null)
				{
					_titleInputField.SetTextWithoutNotify(_noteService.GetPageTitle(pageID));
					_mainInputField.text = pageSaveData.MainText;
				}
			}
		}).AddTo(this);
		_titleInputField.OnValueChangedAsObservable().Subscribe(delegate(string titleText)
		{
			_noteService.ChangeTitle(_noteService.CurrentPageID.CurrentValue, titleText);
		}).AddTo(this);
		_titleInputField.OnEndEditAsObservable().Subscribe(delegate(string titleText)
		{
			_noteService.ChangeEndTitle(_noteService.CurrentPageID.CurrentValue, titleText);
		}).AddTo(this);
		_noteService.OnChangeTitle.Subscribe(delegate((ulong pageID, string title) info)
		{
			if (_noteService.CurrentPageID.CurrentValue == info.pageID)
			{
				_titleInputField.SetTextWithoutNotify(info.title);
			}
		}).AddTo(this);
		_noteService.OnRemovePage.Subscribe(delegate(ulong pageID)
		{
			if (pageID == _noteService.CurrentPageID.CurrentValue)
			{
				InitializePage();
			}
		}).AddTo(this);
	}

	public void InitializePage()
	{
		_titleInputField.SetTextWithoutNotify(string.Empty);
		_mainInputField.text = string.Empty;
	}
}
