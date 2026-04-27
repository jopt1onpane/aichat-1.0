using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using NestopiSystem;
using NestopiSystem.DIContainers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class TodoListSelectorListItemView : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private Button selectButton;

	[SerializeField]
	private InteractableUI selectButtonInteractable;

	[SerializeField]
	private Button removeButton;

	[SerializeField]
	private InteractableUI removeButtonInteractable;

	[SerializeField]
	private Button renameButton;

	[SerializeField]
	private InteractableUI renameButtonInteractable;

	[SerializeField]
	private RemovingModeUI removingModeUI;

	private TodoListSelectorListItemModel model;

	private LocalizationMasterWrapper _localizationMaster;

	private Subject<ulong> onClickSelect = new Subject<ulong>();

	private Subject<ulong> onClickRemove = new Subject<ulong>();

	private Subject<(ulong, string)> onValueChangeTitle = new Subject<(ulong, string)>();

	private Subject<(ulong, string)> onEndEditTitle = new Subject<(ulong, string)>();

	public Observable<ulong> OnClickSelect => onClickSelect;

	public Observable<ulong> OnClickRemove => onClickRemove;

	public Observable<(ulong uuid, string title)> OnValueChangeTitle => onValueChangeTitle;

	public Observable<(ulong uuid, string title)> OnEndEditTitle => onEndEditTitle;

	public void Initialize(ReactiveProperty<bool> gate)
	{
		selectButton.BindGate(gate).AddTo(this);
		removeButton.BindGate(gate).AddTo(this);
		renameButton.BindGate(gate).AddTo(this);
		inputField.BindGate(gate).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(selectButton.OnClickAsObservable(), delegate
		{
			OnClickSelectTodoList();
		}).AddTo(this);
		removeButton.OnClickAsObservable().SubscribeAwait(async delegate(Unit _, CancellationToken ct)
		{
			CommonDialog d = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_confirm";
				o.BodyID = "ui_todo_list_remove_confirm";
				o.BodySelector = (string s) => s.FormatSmart(model.Title);
				o.Parent = RoomLifetimeScope.Resolve<IUICanvasProvider>().CommonDialogParent;
				o.Buttons = new CommonButton[2]
				{
					new CommonButton("ui_todo_list_remove_confirm_ok", CommonButtonStyle.Submit),
					new CommonButton("ui_common_confirm_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
				};
				o.EnableCloseOnClickButton = true;
			});
			object obj = null;
			int num = 0;
			try
			{
				if (await d.SubmitOrCloseWaitAsync(ct) == 0)
				{
					OnClickRemoveTodoList();
				}
				num = 1;
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)d != null)
			{
				await d.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num != 1)
			{
			}
		}, gate).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(renameButton.OnClickAsObservable(), delegate
		{
			OnClickButtonEditTitleText();
		}).AddTo(this);
		inputField.OnValueChangedAsObservable().Subscribe(delegate(string text)
		{
			OnValueChangeTodoListTitle(text);
		}).AddTo(this);
		inputField.OnEndEditAsObservable().Subscribe(delegate(string text)
		{
			OnEndEditTodoListTitle(text);
			inputField.enabled = false;
		}).AddTo(this);
	}

	public void SetModel(TodoListSelectorListItemModel model, bool isRemovable = false)
	{
		this.model = model;
		SetRemoveMode(isRemovable);
		if (string.IsNullOrEmpty(model.Title))
		{
			if (_localizationMaster == null)
			{
				_localizationMaster = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
			}
			_localizationMaster.Bind(inputField, "ui_todo_list_init_title");
			OnEndEditTodoListTitle(inputField.text);
		}
		else
		{
			inputField.SetTextWithoutNotify(model.Title);
		}
		selectButtonInteractable.Setup();
		selectButtonInteractable.DeactivateAllUI(isUseDoComplete: true);
		if (model.IsCurrentSelect)
		{
			selectButtonInteractable.ActivateUseUI(isUseDoComplete: true);
		}
		removeButtonInteractable.Setup();
		renameButtonInteractable.Setup();
		if (model.IsCreateNew)
		{
			OnClickButtonEditTitleText();
			model.IsCreateNew = false;
		}
	}

	public void SetRemoveMode(bool isRemovable, bool isImmediate = false)
	{
		if (isImmediate)
		{
			removingModeUI.TransitionImmediate(isRemovable);
		}
		else
		{
			removingModeUI.Transition(isRemovable);
		}
	}

	public void OnClickSelectTodoList()
	{
		_ = Time.time;
		_ = model.LastClickTime;
		onClickSelect.OnNext(model.UniqueId);
		model.LastClickTime = Time.time;
	}

	public void OnClickRemoveTodoList()
	{
		onClickRemove.OnNext(model.UniqueId);
	}

	public void OnClickButtonEditTitleText()
	{
		inputField.enabled = true;
		inputField.ActivateInputField();
	}

	public void OnValueChangeTodoListTitle(string text)
	{
		if (model != null)
		{
			onValueChangeTitle?.OnNext((model.UniqueId, text));
		}
	}

	public void OnEndEditTodoListTitle(string text)
	{
		if (model != null)
		{
			onEndEditTitle?.OnNext((model.UniqueId, text));
		}
	}

	private void OnDestroy()
	{
		onClickSelect?.Dispose();
		onClickRemove?.Dispose();
		onValueChangeTitle?.Dispose();
		onEndEditTitle?.Dispose();
	}
}
