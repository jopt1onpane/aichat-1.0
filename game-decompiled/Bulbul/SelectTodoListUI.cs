using System;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SelectTodoListUI : MonoBehaviour
{
	[Inject]
	private LocalizationMasterWrapper _localizationMaster;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private TMP_InputField _titleInputField;

	[SerializeField]
	private Button _selectTodoListButton;

	[SerializeField]
	private Button _removeTodoListButton;

	[SerializeField]
	private InteractableUI _interactableUI;

	private Action<ulong, InteractableUI> _onSelectTodoListAction;

	private Action<ulong> _onRemoveTodoListAction;

	private Action<ulong, string> _onValueChangedTitleAction;

	private Action<ulong, string> _onChangedTitleAction;

	private float _lastClickTime = -1f;

	public ulong TodoListID { get; private set; }

	public void Setup(TodoListData todoListData, Action<ulong, InteractableUI> onSelectPage, Action<ulong, string> onValueChangedTitleAction, Action<ulong, string> onChangedTitle, Action<ulong> onDeleteAction)
	{
		TodoListID = todoListData.UniqueID;
		_onSelectTodoListAction = (Action<ulong, InteractableUI>)Delegate.Combine(_onSelectTodoListAction, onSelectPage);
		ObservableSubscribeExtensions.Subscribe(_selectTodoListButton.OnClickAsObservable(), delegate
		{
			OnClickButtonSelectTodoList(isPlaySe: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_removeTodoListButton.OnClickAsObservable(), delegate
		{
			OnClickButtonRemoveTodoList();
		}).AddTo(this);
		_onValueChangedTitleAction = (Action<ulong, string>)Delegate.Combine(_onValueChangedTitleAction, onValueChangedTitleAction);
		_onChangedTitleAction = (Action<ulong, string>)Delegate.Combine(_onChangedTitleAction, onChangedTitle);
		_onRemoveTodoListAction = (Action<ulong>)Delegate.Combine(_onRemoveTodoListAction, onDeleteAction);
		if (todoListData.TitleText == null)
		{
			if (_localizationMaster == null)
			{
				_localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
			}
			_localizationMaster.Bind(_titleInputField, "ui_todo_list_init_title");
		}
		else
		{
			_titleInputField.text = todoListData.TitleText;
		}
		if (_systemSeService == null)
		{
			_systemSeService = RoomLifetimeScope.Resolve<SystemSeService>();
		}
		_interactableUI.Setup();
		OnEndEditTitleText();
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void EnableRemoveButton()
	{
		_removeTodoListButton.gameObject.SetActive(value: true);
	}

	public void DisableRemoveButton()
	{
		_removeTodoListButton.gameObject.SetActive(value: false);
	}

	public void OnClickButtonSelectTodoList(bool isPlaySe = false)
	{
		if (Time.time - _lastClickTime <= 0.5f)
		{
			OnClickButtonEditTitleText();
			_lastClickTime = -1f;
			return;
		}
		if (isPlaySe)
		{
			_systemSeService.PlayClick();
		}
		_onSelectTodoListAction?.Invoke(TodoListID, _interactableUI);
		_lastClickTime = Time.time;
	}

	public void OnClickButtonRemoveTodoList()
	{
		_systemSeService.PlayCancel();
		_onRemoveTodoListAction?.Invoke(TodoListID);
	}

	public void OnClickButtonEditTitleText()
	{
		_titleInputField.ActivateInputField();
	}

	public void OnValueChangedTitleText()
	{
		_onValueChangedTitleAction?.Invoke(TodoListID, _titleInputField.text);
	}

	public void OnEndEditTitleText()
	{
		_onChangedTitleAction?.Invoke(TodoListID, _titleInputField.text);
	}
}
