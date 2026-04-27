using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class NoteContentListView : OSA<NoteContentListParams, NoteContentListItemViewHolder>, IOSAModelIndexGetter<ulong>, IInfomationProviderForOSAMyAnimation
{
	[SerializeField]
	private NoteContentListDragManipulator _dragManipulator;

	private OSAListDataHelper<NoteContentListItemModel> _data;

	private NoteContentListItemModel[] _requestedSetupData;

	private Subject<(ulong, bool, bool)> _onClickSelectButton = new Subject<(ulong, bool, bool)>();

	private Subject<ulong> _onClickRemoveButton = new Subject<ulong>();

	private Subject<(ulong, ulong)> _onChangeOrderForReasonDragged = new Subject<(ulong, ulong)>();

	private bool _isRemovingMode;

	private ItemRemoveAnimationState<ulong> _removeAnimationState;

	private List<ulong> _removedRequests = new List<ulong>();

	private IOSAModelIndexGetter<ulong> _idxGetter;

	public Observable<(ulong, bool, bool)> OnClickSelectButton => _onClickSelectButton;

	public Observable<ulong> OnClickRemoveButton => _onClickRemoveButton;

	public Observable<(ulong, ulong)> OnChangeOrderForReasonDragged => _onChangeOrderForReasonDragged;

	protected override void Start()
	{
		NoteContentListItemView.OnClickSelectButtonAction = ExecOnClickSelectButton;
		NoteContentListItemView.OnClickRemoveButtonAction = ExecOnClickRemoveButton;
		_idxGetter = GetComponent<IOSAModelIndexGetter<ulong>>();
		_data = new OSAListDataHelper<NoteContentListItemModel>(this);
		_Params.ItemPrefab.gameObject.SetActive(value: false);
		_dragManipulator.Init(this, _data);
		_dragManipulator.OnChangeOrder.Subscribe(delegate((int movedIdx, NoteContentListItemModel draggedModel) data)
		{
			ulong pageUniqueID = data.draggedModel.pageUniqueID;
			ulong item = ((data.movedIdx == 0) ? 0 : _data[data.movedIdx - 1].pageUniqueID);
			_onChangeOrderForReasonDragged.OnNext((pageUniqueID, item));
		}).AddTo(this);
		_removeAnimationState = new ItemRemoveAnimationState<ulong>(this, this);
		_removeAnimationState.OnComplete.Subscribe(delegate(ulong uniqueID)
		{
			_removedRequests.Add(uniqueID);
		}).AddTo(this);
		base.Start();
		if (_requestedSetupData != null)
		{
			SetupNoteModel(_requestedSetupData);
			_requestedSetupData = null;
		}
	}

	private void ExecOnClickSelectButton(ulong pageUniqueID, bool isStartInput = false, bool isTargetTitle = false)
	{
		_onClickSelectButton.OnNext((pageUniqueID, isStartInput, isTargetTitle));
	}

	public void SelectNote(ulong pageUniqueID, bool isStartInput, bool isTargetTitle)
	{
		if (base.gameObject.activeInHierarchy)
		{
			int modelIndex = _idxGetter.GetModelIndex(pageUniqueID);
			ScrollTo(modelIndex);
		}
		ExecOnClickSelectButton(pageUniqueID, isStartInput, isTargetTitle);
	}

	private void ExecOnClickRemoveButton(ulong pageUniqueID)
	{
		_onClickRemoveButton.OnNext(pageUniqueID);
	}

	public void AddNoteItemModel(NoteContentListItemModel model)
	{
		if (!base.IsInitialized)
		{
			Debug.LogWarning("NoteContentListView OSAListの初期化がされていません");
		}
		else
		{
			_data.InsertOneAtEnd(model);
		}
	}

	public void RemoveNoteItemModel(ulong pageUniqueID)
	{
		if (!base.IsInitialized)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < _data.Count; i++)
		{
			if (_data[i].pageUniqueID == pageUniqueID)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			ulong pageUniqueID2 = _data[num].pageUniqueID;
			NoteContentListItemViewHolder itemViewsHolderIfVisible = GetItemViewsHolderIfVisible(num);
			_removeAnimationState.Play(itemViewsHolderIfVisible.View, pageUniqueID2, _Params.DefaultItemSize, 0.1f).Forget();
		}
	}

	public void SetupNoteModel(NoteContentListItemModel[] models)
	{
		if (!base.IsInitialized)
		{
			_requestedSetupData = models;
			return;
		}
		if (_data.Count != 0)
		{
			_data.RemoveItems(0, _data.Count);
		}
		if (models != null)
		{
			_data.InsertItemsAtEnd(models);
		}
	}

	protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
	{
		base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
	}

	protected override NoteContentListItemViewHolder CreateViewsHolder(int itemIndex)
	{
		NoteContentListItemViewHolder noteContentListItemViewHolder = new NoteContentListItemViewHolder();
		noteContentListItemViewHolder.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		noteContentListItemViewHolder.DragHandle.Init(_dragManipulator, noteContentListItemViewHolder);
		return noteContentListItemViewHolder;
	}

	protected override void UpdateViewsHolder(NoteContentListItemViewHolder item)
	{
		NoteContentListItemModel model = _data[item.ItemIndex];
		bool isPlaceholder = _dragManipulator.IsPlaceHolderModel(model);
		item.SetModel(model, isPlaceholder, _isRemovingMode);
	}

	protected override void OnBeforeRecycleOrDisableViewsHolder(NoteContentListItemViewHolder item, int newItemIndex)
	{
		item.UnsetModel();
		base.OnBeforeRecycleOrDisableViewsHolder(item, newItemIndex);
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsInitialized)
		{
			_dragManipulator.Update();
			UpdateRequest();
		}
	}

	private void UpdateRequest()
	{
		foreach (ulong request in _removedRequests)
		{
			(NoteContentListItemModel, int) tuple = _data.Indexed().FirstOrDefault(delegate((NoteContentListItemModel item, int index) x)
			{
				var (noteContentListItemModel, _) = x;
				return (noteContentListItemModel != null && noteContentListItemModel.pageUniqueID == request) ? true : false;
			}, (null, -1));
			if (tuple.Item2 != -1)
			{
				_data.RemoveOne(tuple.Item2);
			}
		}
		_removedRequests.Clear();
	}

	public void ChangeRemovingMode(bool isRemoving, bool isImmediate = false)
	{
		_isRemovingMode = isRemoving;
		if (!base.IsInitialized || _VisibleItems.Count == 0)
		{
			return;
		}
		foreach (NoteContentListItemViewHolder visibleItem in _VisibleItems)
		{
			visibleItem.ChangeRemovingMode(isRemoving, isImmediate);
		}
	}

	public void ChangeTitleModelData(string title, ulong pageUniqueId)
	{
		(NoteContentListItemModel, int) tuple = _data.Select((NoteContentListItemModel model, int idx) => (model: model, idx: idx)).FirstOrDefault(((NoteContentListItemModel model, int idx) data) => data.model.pageUniqueID == pageUniqueId);
		if (tuple.Item1 != null)
		{
			tuple.Item1.Title = title;
			GetItemViewsHolderIfVisible(tuple.Item2)?.SetModel(tuple.Item1, isPlaceholder: false, _isRemovingMode);
		}
	}

	int IOSAModelIndexGetter<ulong>.GetModelIndex(ulong equatable)
	{
		for (int i = 0; i < _data.Count; i++)
		{
			NoteContentListItemModel noteContentListItemModel = _data[i];
			if (noteContentListItemModel != null && noteContentListItemModel.pageUniqueID == equatable)
			{
				return i;
			}
		}
		return -1;
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary)
	{
		RequestChangeItemSizeAndUpdateLayout(idx, size, endEdgeStationary);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSize(int idx, float size)
	{
	}
}
