using System.Collections.Generic;
using Com.ForbiddenByte.OSA.Core;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class EnvironmentVolumeListView : OSA<EnvironmentVolumeListParams, EnvironmentVolumeItemViewsHolder>
{
	private OSAListDataHelper<EnvironmentVolumeItemModel> _data;

	private readonly Subject<EnvironmentType> _onClickItemMainButton = new Subject<EnvironmentType>();

	private readonly Subject<(EnvironmentType environmentType, float volume)> _onVolumeSliderChange = new Subject<(EnvironmentType, float)>();

	public IList<EnvironmentVolumeItemModel> ModelList => _data.List;

	public Observable<EnvironmentType> OnClickItemMainButton => _onClickItemMainButton;

	public Observable<(EnvironmentType environmentType, float volume)> OnVolumeSliderChange => _onVolumeSliderChange;

	public void Initialize()
	{
		_data = new OSAListDataHelper<EnvironmentVolumeItemModel>(this);
		_Params.Prefab.gameObject.SetActive(value: false);
		Init();
	}

	protected override EnvironmentVolumeItemViewsHolder CreateViewsHolder(int itemIndex)
	{
		EnvironmentVolumeItemViewsHolder instance = new EnvironmentVolumeItemViewsHolder();
		_ = _data[itemIndex];
		instance.Init(_Params.Prefab, _Params.Content, itemIndex);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickMainButton, delegate
		{
			_onClickItemMainButton.OnNext(instance.View.EnvironmentType);
		}).AddTo(this);
		instance.View.OnVolumeSliderChange.Subscribe(delegate(float volume)
		{
			_onVolumeSliderChange.OnNext((instance.View.EnvironmentType, volume));
		}).AddTo(this);
		return instance;
	}

	protected override void UpdateViewsHolder(EnvironmentVolumeItemViewsHolder item)
	{
		EnvironmentVolumeItemModel model = _data[item.ItemIndex];
		item.View.SetModel(model, withAnimation: false);
	}

	protected override void OnBeforeRecycleOrDisableViewsHolder(EnvironmentVolumeItemViewsHolder item, int newItemIndex)
	{
		item.View.UnsetModel();
		base.OnBeforeRecycleOrDisableViewsHolder(item, newItemIndex);
	}

	public void SetItems(IEnumerable<EnvironmentVolumeItemModel> items)
	{
		_data.ResetItems(items);
	}

	public void ReapplyModel(int modelIndex)
	{
		EnvironmentVolumeItemViewsHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(modelIndex);
		itemViewsHolderIfVisible?.View.SetModel(_data[itemViewsHolderIfVisible.ItemIndex], withAnimation: true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Velocity = Vector2.zero;
	}
}
