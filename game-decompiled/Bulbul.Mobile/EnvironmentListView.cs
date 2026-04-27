using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.CustomAdapters.GridView;
using FastEnumUtility;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentListView : GridAdapter<EnvironmentListParams, EnvironmentItemViewsHolder>
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private EnvironmentTabSelectorView _tabSelector;

	private OSAListDataHelper<EnvironmentItemModel> _displayData;

	private readonly Subject<EnvironmentType> _onClickItemMainButton = new Subject<EnvironmentType>();

	private readonly Subject<EnvironmentType> _onClickItemWindowButton = new Subject<EnvironmentType>();

	private readonly Subject<EnvironmentType> _onClickPurchaseButton = new Subject<EnvironmentType>();

	private readonly Subject<Unit> _onClickMobileDemoEditionLocked = new Subject<Unit>();

	private readonly Dictionary<EnvironmentControllerType, List<EnvironmentItemModel>> _modelsDic = new Dictionary<EnvironmentControllerType, List<EnvironmentItemModel>>();

	private readonly Dictionary<EnvironmentControllerType, double> _tabScrollValueDic = new Dictionary<EnvironmentControllerType, double>();

	public Observable<EnvironmentType> OnClickItemMainButton => _onClickItemMainButton;

	public Observable<EnvironmentType> OnClickItemWindowButton => _onClickItemWindowButton;

	public Observable<EnvironmentType> OnClickPurchaseButton => _onClickPurchaseButton;

	public Observable<Unit> OnClickMobileDemoEditionLocked => _onClickMobileDemoEditionLocked;

	public void Initialize()
	{
		_displayData = new OSAListDataHelper<EnvironmentItemModel>(this);
		_Params.Grid.CellPrefab.gameObject.SetActive(value: false);
		Init();
		foreach (EnvironmentControllerType value in FastEnum.GetValues<EnvironmentControllerType>())
		{
			_modelsDic.Add(value, new List<EnvironmentItemModel>());
			_tabScrollValueDic.Add(value, 0.0);
		}
		_tabSelector.Initialize();
		_tabSelector.SelectTab(EnvironmentControllerType.ViewAndSound);
		_tabSelector.OnClickTab.Subscribe(delegate(EnvironmentControllerType controllerType)
		{
			_systemSeService.PlaySelect();
			ChangeTab(controllerType);
		}).AddTo(this);
	}

	protected override void OnCellViewsHolderCreated(EnvironmentItemViewsHolder cellVH, CellGroupViewsHolder<EnvironmentItemViewsHolder> cellGroup)
	{
		ObservableSubscribeExtensions.Subscribe(cellVH.View.OnClickMainButton, delegate
		{
			if (cellVH.View.IsMobileDemoEditionLocked)
			{
				_onClickMobileDemoEditionLocked.OnNext(Unit.Default);
			}
			else
			{
				_onClickItemMainButton.OnNext(cellVH.View.EnvironmentType);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(cellVH.View.OnClickWindowButton, delegate
		{
			if (cellVH.View.IsMobileDemoEditionLocked)
			{
				_onClickMobileDemoEditionLocked.OnNext(Unit.Default);
			}
			else
			{
				_onClickItemWindowButton.OnNext(cellVH.View.EnvironmentType);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(cellVH.View.OnClickPurchaseButton, delegate
		{
			_onClickPurchaseButton.OnNext(cellVH.View.EnvironmentType);
		}).AddTo(this);
	}

	protected override void UpdateCellViewsHolder(EnvironmentItemViewsHolder item)
	{
		EnvironmentItemModel model = _displayData[item.ItemIndex];
		item.View.SetModel(model, withAnimation: false);
	}

	protected override void OnBeforeRecycleOrDisableCellViewsHolder(EnvironmentItemViewsHolder item, int newItemIndex)
	{
		item.View.UnsetModel();
		base.OnBeforeRecycleOrDisableCellViewsHolder(item, newItemIndex);
	}

	public void SetItems(Dictionary<EnvironmentControllerType, IEnumerable<EnvironmentItemModel>> items)
	{
		foreach (var (key, list2) in _modelsDic)
		{
			list2.Clear();
			if (items.TryGetValue(key, out var value))
			{
				list2.AddRange(value);
				list2.Sort(CompareModelsOrder);
			}
		}
		List<EnvironmentItemModel> models = _modelsDic[_tabSelector.CurrentTab];
		_displayData.ResetItems(models);
	}

	public void ReapplyModel(EnvironmentControllerType controllerType, int modelIndex, bool lockStateChanged)
	{
		if (controllerType == _tabSelector.CurrentTab)
		{
			if (lockStateChanged)
			{
				List<EnvironmentItemModel> list = _modelsDic[controllerType];
				list.Sort(CompareModelsOrder);
				_displayData.ResetItems(list);
			}
			else
			{
				EnvironmentItemViewsHolder cellViewsHolderIfVisible = GetCellViewsHolderIfVisible(modelIndex);
				cellViewsHolderIfVisible?.View.SetModel(_displayData[cellViewsHolderIfVisible.ItemIndex], withAnimation: true);
			}
		}
	}

	private int CompareModelsOrder(EnvironmentItemModel a, EnvironmentItemModel b)
	{
		int num = (a.LockState == EnvironmentItemModel.ItemLockState.Locked).CompareTo(b.LockState == EnvironmentItemModel.ItemLockState.Locked);
		if (num != 0)
		{
			return num;
		}
		return a.SortOrder.CompareTo(b.SortOrder);
	}

	public (EnvironmentItemModel model, int modelIndex) GetModel(EnvironmentControllerType controllerType, EnvironmentType environmentType)
	{
		List<EnvironmentItemModel> list = _modelsDic[controllerType];
		for (int i = 0; i < list.Count; i++)
		{
			EnvironmentItemModel environmentItemModel = list[i];
			if (environmentItemModel.EnvironmentType == environmentType)
			{
				return (model: environmentItemModel, modelIndex: i);
			}
		}
		return (model: null, modelIndex: -1);
	}

	public IEnumerable<(EnvironmentControllerType controllerType, IReadOnlyList<EnvironmentItemModel> models)> GetAllModels()
	{
		return _modelsDic.Select((KeyValuePair<EnvironmentControllerType, List<EnvironmentItemModel>> x) => ((EnvironmentControllerType Key, IReadOnlyList<EnvironmentItemModel>))(Key: x.Key, x.Value));
	}

	private void ChangeTab(EnvironmentControllerType controllerType)
	{
		_tabScrollValueDic[_tabSelector.CurrentTab] = GetNormalizedPosition();
		_tabSelector.SelectTab(controllerType);
		List<EnvironmentItemModel> models = _modelsDic[_tabSelector.CurrentTab];
		_displayData.ResetItems(models);
		SetNormalizedPosition(_tabScrollValueDic[_tabSelector.CurrentTab]);
	}
}
