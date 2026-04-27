using System;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityHabitTrackerContentUI : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private HabitTrackerUIViewMobile _uiView;

	private HabitTrackerUIModel _model;

	public void Setup()
	{
		if (_model == null)
		{
			_model = new HabitTrackerUIModel();
			_model.EnterSetting();
		}
		_uiView.Setup();
		_uiView.OnPrepareContent.Subscribe(delegate(bool fromTab)
		{
			EnterHabitTrackerContent(fromTab);
		}).AddTo(this);
		_uiView.ContentView.OnClickHabitRemoveMode.Subscribe(delegate(bool isRemoveMode)
		{
			_model.ChangeRemoveMode(isRemoveMode);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiView.ContentView.OnClickHabitSetting, delegate
		{
			_model.ChangeRemoveMode(isRemoveMode: false);
			_model.ChangeSettingMode(isSettingMode: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_uiView.ContentView.OnClickHabitSettingComplete.Subscribe(delegate(bool isApply)
		{
			_model.FinishHabitDayEnabledSetting(isApply);
			_model.ChangeSettingMode(isSettingMode: false);
			if (isApply)
			{
				_systemSeService.PlayClick();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		_uiView.ContentView.OnChangeWeek.Subscribe(delegate(bool isNext)
		{
			_model.ChangeWeek(isNext);
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_uiView.ContentView.OnClickAddHabit, delegate
		{
			_model.AddHabit();
			_systemSeService.PlayClick();
		}).AddTo(this);
		_uiView.ListView.OnClickRemoveHabit.Subscribe(delegate(string uuid)
		{
			_model.RemoveHabit(uuid);
			_systemSeService.PlayCancel();
		}).AddTo(this);
		_uiView.ListView.OnChangeHabitTitle.Subscribe(delegate((string uuid, string title) info)
		{
			_model.SetHabitTitle(info.uuid, info.title);
		}).AddTo(this);
		_uiView.ListView.OnChangeHabitPeriod.Subscribe(delegate((string uuid, bool enable) info)
		{
			_model.ChangeHabitPeriod(info.uuid, info.enable);
			if (info.enable)
			{
				_systemSeService.PlayClick();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		_uiView.ListView.OnChangeHabitComplete.Subscribe(delegate((string uuid, DateTime date, bool isComplete) info)
		{
			_model.SetHabitDayCompleted(info.uuid, info.date, info.isComplete);
			if (info.isComplete)
			{
				_systemSeService.PlayTaskComplete();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		_uiView.ListView.OnChangeHabitEnable.Subscribe(delegate((string uuid, DayOfWeek date, bool enable) info)
		{
			_model.SetDirtyHabitDayEnabled(info.uuid, info.date, info.enable);
			if (info.enable)
			{
				_systemSeService.PlayClick();
			}
			else
			{
				_systemSeService.PlayCancel();
			}
		}).AddTo(this);
		_uiView.ListView.OnChangeReorder.Subscribe(delegate((string from, string to) info)
		{
			_model.HabitReorder(info.from, info.to);
		}).AddTo(this);
		_model.OnChangedWeek.Subscribe(delegate(bool isNext)
		{
			_uiView.ChangeWeek(isNext);
		}).AddTo(this);
		_model.OnChangeRemoveMode.Subscribe(delegate(bool isRemoveMode)
		{
			_uiView.ChangeRemoveMode(isRemoveMode);
		}).AddTo(this);
		_model.OnChangeSettingMode.Subscribe(delegate(bool isSettingMode)
		{
			_uiView.ChangeSettingMode(isSettingMode);
		}).AddTo(this);
		_model.OnAddHabit.Subscribe(delegate(string uuid)
		{
			_uiView.ContentView.UpdateMarkCompleteAll();
			_uiView.ListView.AddHabit(uuid);
		}).AddTo(this);
		_model.OnRemoveHabit.Subscribe(delegate(string uuid)
		{
			_uiView.ContentView.UpdateMarkCompleteAll();
			_uiView.ListView.RemoveHabit(uuid);
		}).AddTo(this);
		_model.OnChangeHabitTitle.Subscribe(delegate((string uuid, string title) info)
		{
			_uiView.ListView.ChangeHabitTitle(info.uuid, info.title);
		}).AddTo(this);
		_model.OnChangeHabitPeriod.Subscribe(delegate(string uuid)
		{
			_uiView.ContentView.UpdateMarkCompleteAll();
			_uiView.ListView.ChangeHabitPeriod(uuid);
		}).AddTo(this);
		_model.OnChangeHabitComplete.Subscribe(delegate((string uuid, DateTime date, bool isCompleted) info)
		{
			_uiView.ContentView.UpdateMarkCompleteForDate(info.date);
			_uiView.ListView.ChangeHabitComplete(info.uuid, info.date, info.isCompleted);
		}).AddTo(this);
		_model.OnChangeDirtyHabitDayEnable.Subscribe(delegate((string uuid, DayOfWeek date, bool isEnable) info)
		{
			_uiView.ListView.ChangeHabitEnable(info.uuid, info.date, info.isEnable);
		}).AddTo(this);
		_model.OnChangeHabitDayOfWeekEnableComplete.Subscribe(delegate(bool isApply)
		{
			_uiView.ContentView.UpdateMarkCompleteAll();
			_uiView.ListView.UpdateHabitDayOfWeekEnableComplete(isApply);
		}).AddTo(this);
	}

	private void EnterHabitTrackerContent(bool fromTab)
	{
		_model.EnterSetting();
		_uiView.ContentView.EnterSetting(_model);
		_uiView.ListView.EnterSetting(_model);
	}
}
