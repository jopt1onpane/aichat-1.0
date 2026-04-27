using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class HabitListUI : MonoBehaviour
{
	private class ReorderInfo
	{
		public readonly HabitItemUI Item;

		public readonly int OriginIndex;

		public ReorderInfo(HabitItemUI button, int originIndex)
		{
			Item = button;
			OriginIndex = originIndex;
		}
	}

	[Inject]
	private HabitDataService _habitService;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("Habit用プレハブ")]
	private HabitItemUI _habitItemUIPrefab;

	[SerializeField]
	[Header("Habitの親オブジェクト")]
	private Transform _habitParent;

	[SerializeField]
	[Header("Habit追加ボタン")]
	private Button _addHabitUIButton;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private HabitWeekLabelUI _weekLabelUI;

	[SerializeField]
	private float _weekLabelMoveX;

	[SerializeField]
	private float _weekLabelMoveDuration;

	[SerializeField]
	private Ease _weekLabelMoveEase;

	[SerializeField]
	private HabitEditModeButton _editModeButton;

	[SerializeField]
	private HabitDeleteWarningPopover _deleteWarningDialog;

	private ReorderInfo reorderInfo;

	private readonly List<HabitItemUI> _habitItemList = new List<HabitItemUI>();

	[SerializeField]
	private HabitItemUI dummyButton;

	private DateTime _startDate;

	private bool _isEditMode;

	private readonly Dictionary<(string habitId, DayOfWeek dayOfWeek), bool> _editModeEnableChangedDic = new Dictionary<(string, DayOfWeek), bool>();

	private Tween _weekLabelTween;

	private float _weekLabelsInitialX;

	public void Setup()
	{
		foreach (Transform item in _habitParent)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		dummyButton.Initialize();
		dummyButton.gameObject.SetActive(value: false);
		_deleteWarningDialog.Setup();
		_weekLabelUI.Initialize();
		_startDate = _habitService.GetStartDateOfWeek(DateTime.Today);
		_weekLabelUI.Setup(_startDate, DateTime.Today);
		_weekLabelsInitialX = (_weekLabelUI.transform as RectTransform).anchoredPosition.x;
		foreach (string allHabitId in _habitService.GetAllHabitIds(includeDeleted: false))
		{
			AddHabitUI(allHabitId);
		}
		UpdateCompleteMarkForAll();
		ObservableSubscribeExtensions.Subscribe(_habitService.OnChangeHabitPeriod, delegate
		{
			UpdateCompleteMarkForAll();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_addHabitUIButton.OnClickAsObservable(), delegate
		{
			_habitService.CreateHabit();
			_systemSeService.PlayClick();
		}).AddTo(this);
		_habitService.OnAddHabit.Subscribe(delegate(string habitId)
		{
			AddHabitUI(habitId);
		}).AddTo(this);
		_habitService.OnRemoveHabit.Subscribe(delegate(string habitId)
		{
			RemoveHabit(habitId);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_weekLabelUI.OnClickPrevWeek, delegate
		{
			ChangeWeek(isNext: false);
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_weekLabelUI.OnClickNextWeek, delegate
		{
			ChangeWeek(isNext: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_editModeButton.Initialize();
		ObservableSubscribeExtensions.Subscribe(_editModeButton.OnStartEditMode, delegate
		{
			StartEditMode();
		}).AddTo(this);
		_editModeButton.OnEndEditMode.Subscribe(delegate(bool isSave)
		{
			EndEditMode(isSave);
		}).AddTo(this);
		_scrollRect.onValueChanged.AddListener(delegate
		{
			_deleteWarningDialog.Hide();
		});
	}

	private void ChangeWeek(bool isNext)
	{
		_startDate = (isNext ? _startDate.AddDays(7.0) : _startDate.AddDays(-7.0));
		_weekLabelUI.Setup(_startDate, DateTime.Today);
		UpdateCompleteMarkForAll();
		for (int i = 0; i < _habitItemList.Count; i++)
		{
			SetHabitDays(_habitItemList[i], _isEditMode);
		}
		_weekLabelTween?.Kill();
		RectTransform rectTransform = _weekLabelUI.transform as RectTransform;
		int num = (isNext ? 1 : (-1));
		rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithX(_weekLabelsInitialX - _weekLabelMoveX * (float)num);
		_weekLabelTween = rectTransform.DOAnchorPosX(_weekLabelsInitialX, _weekLabelMoveDuration).SetEase(_weekLabelMoveEase).SetLink(base.gameObject);
	}

	private void AddHabitUI(string habitId)
	{
		HabitItemUI itemUI = UnityEngine.Object.Instantiate(_habitItemUIPrefab, _habitParent);
		itemUI.Initialize();
		ReactiveProperty<string> title = new ReactiveProperty<string>(_habitService.GetHabitTitle(habitId));
		_habitService.OnChangeHabitTitle.Subscribe(delegate((string habitId, string title) x)
		{
			if (x.habitId == habitId)
			{
				title.Value = x.title;
			}
		}).AddTo(this);
		itemUI.Setup(new HabitItemUI.ViewModel(habitId, title));
		SetHabitDays(itemUI, _isEditMode);
		itemUI.OnTitleEndEdit.Subscribe(delegate(string val)
		{
			_habitService.SetHabitTitle(habitId, val);
			title.Value = val;
		}).AddTo(this);
		itemUI.OnClickRemove.SubscribeAwait(async delegate
		{
			if (await _deleteWarningDialog.ShowAndWaitResultAsync(itemUI.DeletePopoverPivot.position))
			{
				_habitService.DeleteHabit(habitId);
			}
		}, AwaitOperation.Drop).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(itemUI.OnClickPlayPause, delegate
		{
			if (itemUI.IsHabitEnabled)
			{
				_systemSeService.PlayCancel();
			}
			else
			{
				_systemSeService.PlayClick();
			}
			_habitService.SetHabitPeriodAlive(habitId, !itemUI.IsHabitEnabled);
			itemUI.SetPlayPause(_habitService.IsDateInAlivePeriod(habitId, DateTime.Now));
		}).AddTo(this);
		itemUI.SetPlayPause(_habitService.IsDateInAlivePeriod(habitId, DateTime.Now));
		itemUI.OnStartReorder.Subscribe(delegate((HabitItemUI button, PointerEventData eventData) x)
		{
			OnStartReorder(x.button, x.eventData);
		}).AddTo(this);
		itemUI.OnReorderDrag.Subscribe(delegate((HabitItemUI button, PointerEventData eventData) x)
		{
			OnDragReorder(x.button, x.eventData);
		}).AddTo(this);
		itemUI.OnEndReorder.Subscribe(delegate((HabitItemUI button, PointerEventData eventData) x)
		{
			OnEndReorder(x.button, x.eventData);
		}).AddTo(this);
		_habitItemList.Add(itemUI);
		UpdateCompleteMarkForAll();
	}

	private void RemoveHabit(string habitId)
	{
		int num = _habitItemList.FindIndex((HabitItemUI x) => x.HabitId == habitId);
		if (num >= 0)
		{
			HabitItemUI habitItemUI = _habitItemList[num];
			_habitItemList.RemoveAt(num);
			UnityEngine.Object.Destroy(habitItemUI.gameObject);
			UpdateCompleteMarkForAll();
		}
	}

	private void SetHabitDays(HabitItemUI itemUI, bool editMode)
	{
		if (editMode)
		{
			SetHabitDaysEditMode(itemUI);
		}
		else
		{
			SetHabitDaysNormal(itemUI);
		}
	}

	private void SetHabitDaysNormal(HabitItemUI itemUI)
	{
		string habitId = itemUI.HabitId;
		HabitItemDateUI.ViewModel[] dateViewModels = Enumerable.Range(0, 7).Select(delegate(int i)
		{
			DateTime date = _startDate.AddDays(i);
			ReactiveProperty<HabitDateEnableState> enableStateProperty = GetEnableStateProperty(habitId, date);
			ReactiveProperty<bool> reactiveProperty = new ReactiveProperty<bool>(_habitService.IsHabitDayCompleted(habitId, date));
			reactiveProperty.Skip(1).Subscribe(delegate(bool check)
			{
				_habitService.SetHabitDayCompleted(habitId, date, check);
				if (check)
				{
					_habitService.SetHideOnCalendar(habitId, date, hide: false);
					_systemSeService.PlayTaskComplete();
				}
				else
				{
					_systemSeService.PlayCancel();
				}
				UpdateCompleteMarkForDate(date);
			}).AddTo(this);
			return new HabitItemDateUI.ViewModel(IsEditMode: false, enableStateProperty, reactiveProperty, null);
		}).ToArray();
		itemUI.SetupDays(dateViewModels);
	}

	private void SetHabitDaysEditMode(HabitItemUI itemUI)
	{
		string habitId = itemUI.HabitId;
		HabitItemDateUI.ViewModel[] dateViewModels = Enumerable.Range(0, 7).Select(delegate(int i)
		{
			DateTime date = _startDate.AddDays(i);
			bool flag = _habitService.IsHabitDayOfWeekEnabled(habitId, date.DayOfWeek);
			ReactiveProperty<HabitDateEnableState> displayEnableState = new ReactiveProperty<HabitDateEnableState>((!flag) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
			ReactiveProperty<bool> isChecked = new ReactiveProperty<bool>(value: false);
			ObservableSubscribeExtensions.Subscribe(isChecked.Skip(1), delegate
			{
				isChecked.Value = false;
			}).AddTo(this);
			return new HabitItemDateUI.ViewModel(IsEditMode: true, displayEnableState, isChecked, OnClickChangeEnable);
			void OnClickChangeEnable(bool enable)
			{
				displayEnableState.Value = ((!enable) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
				_editModeEnableChangedDic[(habitId, date.DayOfWeek)] = enable;
				if (enable)
				{
					_systemSeService.PlayClick();
				}
				else
				{
					_systemSeService.PlayCancel();
				}
			}
		}).ToArray();
		itemUI.SetupDays(dateViewModels);
	}

	private ReactiveProperty<HabitDateEnableState> GetEnableStateProperty(string habitId, DateTime date)
	{
		ReactiveProperty<HabitDateEnableState> enableState = new ReactiveProperty<HabitDateEnableState>(GetDisplayType());
		if (_isEditMode && _editModeEnableChangedDic.TryGetValue((habitId, date.DayOfWeek), out var value))
		{
			enableState.Value = ((!value) ? HabitDateEnableState.Disabled : HabitDateEnableState.Enabled);
		}
		ObservableSubscribeExtensions.Subscribe(_habitService.OnChangeHabitPeriod, delegate
		{
			enableState.Value = GetDisplayType();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_habitService.OnChangeHabitDayOfWeekEnabled, delegate
		{
			enableState.Value = GetDisplayType();
		}).AddTo(this);
		return enableState;
		HabitDateEnableState GetDisplayType()
		{
			return _habitService.GetHabitDayEnableState(habitId, date);
		}
	}

	private void UpdateCompleteMarkForDate(DateTime date)
	{
		HabitDateLabelUI dateLabelUIByDate = _weekLabelUI.GetDateLabelUIByDate(date);
		if (!(dateLabelUIByDate == null))
		{
			dateLabelUIByDate.SetCompleted(_habitService.IsAllCompletedForDate(date));
		}
	}

	private void UpdateCompleteMarkForAll()
	{
		for (int i = 0; i < 7; i++)
		{
			UpdateCompleteMarkForDate(_startDate.AddDays(i));
		}
	}

	private void StartEditMode()
	{
		_isEditMode = true;
		_weekLabelUI.SetEditMode(isEditMode: true);
		foreach (HabitItemUI habitItem in _habitItemList)
		{
			SetHabitDays(habitItem, editMode: true);
		}
	}

	private void EndEditMode(bool save)
	{
		if (save)
		{
			foreach (KeyValuePair<(string, DayOfWeek), bool> item3 in _editModeEnableChangedDic)
			{
				item3.Deconstruct(out var key, out var value);
				(string, DayOfWeek) tuple = key;
				string item = tuple.Item1;
				DayOfWeek item2 = tuple.Item2;
				bool flag = value;
				_habitService.SetHabitDayOfWeekEnabled(item, item2, flag);
			}
		}
		_editModeEnableChangedDic.Clear();
		_isEditMode = false;
		UpdateCompleteMarkForAll();
		_weekLabelUI.SetEditMode(isEditMode: false);
		foreach (HabitItemUI habitItem in _habitItemList)
		{
			SetHabitDays(habitItem, editMode: false);
		}
	}

	private void OnStartReorder(HabitItemUI item, PointerEventData eventData)
	{
		if (reorderInfo == null)
		{
			reorderInfo = new ReorderInfo(item, item.transform.GetSiblingIndex());
			ReactiveProperty<string> title = new ReactiveProperty<string>(_habitService.GetHabitTitle(item.HabitId));
			dummyButton.Setup(new HabitItemUI.ViewModel(item.HabitId, title));
			SetHabitDays(dummyButton, _isEditMode);
			dummyButton.SetPlayPause(_habitService.IsDateInAlivePeriod(item.HabitId, DateTime.Now));
			dummyButton.ActivateDragAnimation();
			dummyButton.gameObject.SetActive(value: true);
			item.Hide();
		}
	}

	private void OnDragReorder(HabitItemUI item, PointerEventData eventData)
	{
		if (reorderInfo == null || reorderInfo.Item != item)
		{
			return;
		}
		Vector3 position = dummyButton.transform.position;
		position.y = eventData.position.y;
		dummyButton.transform.position = position;
		int num = -1;
		for (int i = 0; i < _habitParent.childCount; i++)
		{
			Transform child = _habitParent.GetChild(i);
			if (!(dummyButton.transform.position.y + (dummyButton.transform as RectTransform).rect.height * 0.5f < child.position.y))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			item.transform.SetAsLastSibling();
		}
		else
		{
			item.transform.SetSiblingIndex(num);
		}
	}

	private void OnEndReorder(HabitItemUI item, PointerEventData eventData)
	{
		if (reorderInfo != null && !(reorderInfo.Item != item))
		{
			dummyButton.DeactivateDragAnimation();
			dummyButton.gameObject.SetActive(value: false);
			int originIndex = reorderInfo.OriginIndex;
			int siblingIndex = item.transform.GetSiblingIndex();
			reorderInfo = null;
			item.Show();
			if (originIndex != siblingIndex)
			{
				_habitService.ReorderHabit(item.HabitId, siblingIndex);
			}
		}
	}
}
