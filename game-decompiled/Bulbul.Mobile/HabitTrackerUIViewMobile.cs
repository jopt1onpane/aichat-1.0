using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class HabitTrackerUIViewMobile : MonoBehaviour, ITabChangedSetuper
{
	[SerializeField]
	private HabitTrackerContentView _contentView;

	[SerializeField]
	private HabitTrackerListView _listView;

	private Subject<bool> onPrepareContent = new Subject<bool>();

	public HabitTrackerContentView ContentView => _contentView;

	public HabitTrackerListView ListView => _listView;

	public Observable<bool> OnPrepareContent => onPrepareContent;

	void ITabChangedSetuper.SetupBeforeTabChanged(bool isChangedFromTab)
	{
		onPrepareContent?.OnNext(isChangedFromTab);
	}

	private void OnDestroy()
	{
		onPrepareContent?.Dispose();
	}

	public void Setup()
	{
		_contentView.Setup();
		_listView.OnChangeRemoveButtonState.Subscribe(_contentView.UpdateRemoveButtonState).AddTo(this);
	}

	public void ChangeWeek(bool isNext)
	{
		_contentView.ChangeWeek(isNext);
		_listView.ChangeWeek(isNext);
	}

	public void ChangeRemoveMode(bool isRemoveMode)
	{
		_contentView.ChangeRemoveMode(isRemoveMode);
		_listView.ChangeRemoveMode(isRemoveMode);
	}

	public void ChangeSettingMode(bool isSettingMode)
	{
		_contentView.ChangeSettingMode(isSettingMode);
		_listView.ChangeSettingMode(isSettingMode);
	}
}
