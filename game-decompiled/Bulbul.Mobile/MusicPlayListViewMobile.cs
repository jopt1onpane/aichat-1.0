using System;
using System.Collections.Generic;
using System.Linq;
using Com.ForbiddenByte.OSA.Core;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class MusicPlayListViewMobile : OSA<MusicPlayListParam, MusicPlayListItemViewsHolder>, IInfomationProviderForOSAMyAnimation, IOSAModelIndexGetter<string>
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private MusicPlayListDragReorderManipulator _dragManipulator;

	[SerializeField]
	private SimpleNoticeDialog _dialog;

	private OSAListDataHelper<MusicPlayListItemModel> _data;

	private bool _isRemovingMode;

	private IReadOnlyList<GameAudioInfo> _requestedData;

	private FacilityMusic _facilityMusic;

	private ItemRemoveAnimationState<string> _animation;

	private List<string> _removeRequest;

	private DateTime _lastDragCtrlTime = DateTime.Now;

	private MusicChangeKind _lastChangeKind;

	private Subject<(GameAudioInfo, GameAudioInfo)> _onChangeOrderForReasonDragged = new Subject<(GameAudioInfo, GameAudioInfo)>();

	public Observable<(GameAudioInfo, GameAudioInfo)> OnChangeOrderForReasonDragged => _onChangeOrderForReasonDragged;

	protected override void Start()
	{
		_data = new OSAListDataHelper<MusicPlayListItemModel>(this);
		_Params.itemPrefab.gameObject.SetActive(value: false);
		_dragManipulator.Init(this, _data);
		_dragManipulator.OnChangeOrder.Subscribe(delegate((int movedIdx, MusicPlayListItemModel draggedModel) data)
		{
			GameAudioInfo audioInfo = data.draggedModel.audioInfo;
			GameAudioInfo item = ((data.movedIdx == 0) ? null : _data[data.movedIdx - 1].audioInfo);
			_onChangeOrderForReasonDragged.OnNext((audioInfo, item));
		}).AddTo(this);
		_removeRequest = new List<string>();
		_animation = new ItemRemoveAnimationState<string>(this, this);
		_animation.OnComplete.Subscribe(delegate(string uuid)
		{
			_removeRequest.Add(uuid);
		}).AddTo(this);
		base.Start();
		if (_requestedData != null)
		{
			SetupMusicModel(_requestedData);
			_requestedData = null;
		}
	}

	protected override void OnDisable()
	{
		if (_animation != null)
		{
			_animation.Cancel();
		}
		base.OnDisable();
	}

	public void Setup(IReadOnlyList<GameAudioInfo> audioInfoList, FacilityMusic facilityMusic)
	{
		SetupMusicModel(audioInfoList);
		_facilityMusic = facilityMusic;
		_lastDragCtrlTime = DateTime.Now;
		_facilityMusic.MusicService.OnChangeMusic.Subscribe(delegate(MusicChangeKind kind)
		{
			_lastChangeKind = kind;
		}).AddTo(this);
		_facilityMusic.MusicService.OnPlayMusic.Subscribe(delegate(GameAudioInfo music)
		{
			UpdateModels(music);
			if (base.IsInitialized)
			{
				if (_lastChangeKind == MusicChangeKind.PlaylistCellClick)
				{
					_lastDragCtrlTime = DateTime.Now;
				}
				else if ((_lastChangeKind != MusicChangeKind.Auto || !(_lastDragCtrlTime.AddSeconds(10.0) >= DateTime.Now)) && !_dragManipulator.IsDragging)
				{
					(MusicPlayListItemModel, int) tuple = _data.Indexed().FirstOrDefault(((MusicPlayListItemModel item, int index) x) => x.item?.isPlaying ?? false, (null, -1));
					if (tuple.Item1 != null)
					{
						int itemIndex = _VisibleItems[0].ItemIndex;
						int itemIndex2 = _VisibleItems[_VisibleItems.Count - 1].ItemIndex;
						if (itemIndex >= tuple.Item2)
						{
							ScrollTo(tuple.Item2, 0.025f);
						}
						else if (itemIndex2 <= tuple.Item2)
						{
							ScrollTo(tuple.Item2, 0.975f, 1f);
						}
					}
				}
			}
		}).AddTo(this);
		_facilityMusic.MusicService.OnPauseMusic.Subscribe(delegate(GameAudioInfo music)
		{
			UpdateModels(music);
		}).AddTo(this);
		Scrollbar scrollbar = _Params.Scrollbar;
		ObservableSubscribeExtensions.Subscribe(Observable.Merge<PointerEventData>(this.OnBeginDragAsObservable(), this.OnDragAsObservable(), this.OnEndDragAsObservable(), scrollbar.OnBeginDragAsObservable(), scrollbar.OnDragAsObservable(), this.OnEndDragAsObservable()), delegate
		{
			_lastDragCtrlTime = DateTime.Now;
		}).AddTo(this);
	}

	public void SetupMusicModel(IReadOnlyList<GameAudioInfo> audioInfoList, bool isResetRemovingMode = false)
	{
		if (!base.IsInitialized)
		{
			_requestedData = audioInfoList;
			return;
		}
		if (isResetRemovingMode)
		{
			_isRemovingMode = false;
		}
		if (_facilityMusic.MusicService.IsPlayListDirtyForLocalImport || _facilityMusic.MusicService.IsPlayListDirtyForLocalRemove || _isRemovingMode)
		{
			return;
		}
		if (_data.Count != 0)
		{
			_data.RemoveItems(0, _data.Count);
		}
		List<MusicPlayListItemModel> list = new List<MusicPlayListItemModel>();
		foreach (GameAudioInfo audioInfo in audioInfoList)
		{
			bool flag = _facilityMusic.PlayingMusic == audioInfo;
			bool isPausing = flag && _facilityMusic.IsPaused;
			bool isExclude = _facilityMusic.MusicService.IsContainsExcludedFromPlaylist(audioInfo);
			list.Add(new MusicPlayListItemModel
			{
				audioInfo = audioInfo,
				isExclude = isExclude,
				isPlaying = flag,
				isPausing = isPausing,
				isRemoving = false
			});
		}
		_data.InsertItemsAtEnd(list);
		if (_facilityMusic.PlayingMusic != null)
		{
			ScrollToPlayingMusic(_facilityMusic.PlayingMusic.UUID);
		}
	}

	public void MusicImport(GameAudioInfo audioInfo)
	{
		if (_isRemovingMode)
		{
			return;
		}
		foreach (MusicPlayListItemModel item in (IEnumerable<MusicPlayListItemModel>)_data)
		{
			if ((!string.IsNullOrEmpty(item.audioInfo.LocalPath) && item.audioInfo.LocalPath == audioInfo.LocalPath) || item.audioInfo.UUID == audioInfo.UUID)
			{
				return;
			}
		}
		bool flag = _facilityMusic.PlayingMusic == audioInfo;
		bool isPausing = flag && _facilityMusic.IsPaused;
		bool isExclude = _facilityMusic.MusicService.IsContainsExcludedFromPlaylist(audioInfo);
		_data.InsertOneAtEnd(new MusicPlayListItemModel
		{
			audioInfo = audioInfo,
			isExclude = isExclude,
			isPlaying = flag,
			isPausing = isPausing,
			isRemoving = false
		});
	}

	public void MusicRemove(GameAudioInfo audioInfo)
	{
		int num = -1;
		foreach (var item in _data.Indexed())
		{
			if (item.item.audioInfo.UUID.Equals(audioInfo.UUID))
			{
				num = item.index;
				break;
			}
		}
		if (num != -1)
		{
			_data.RemoveOne(num);
		}
		_facilityMusic.MusicService.OnEndAdjustPlayListForLocalRemove();
	}

	public void ScrollToPlayingMusic(string uuid)
	{
		if (!base.IsInitialized)
		{
			return;
		}
		foreach (var item in _data.Indexed())
		{
			if (item.item.audioInfo.UUID.Equals(uuid))
			{
				ScrollTo(item.index, 0.5f, 0.5f);
				break;
			}
		}
	}

	public void SetPlayListMode(bool isRemoving, bool isImmediate = false)
	{
		_isRemovingMode = isRemoving;
		if (!base.IsInitialized || _VisibleItems.Count == 0)
		{
			return;
		}
		foreach (MusicPlayListItemViewsHolder visibleItem in _VisibleItems)
		{
			visibleItem.ChangeRemovingMode(isRemoving, isImmediate);
		}
	}

	private void UpdateModels(GameAudioInfo music)
	{
		if (!base.IsInitialized)
		{
			return;
		}
		bool isPaused = _facilityMusic.IsPaused;
		foreach (MusicPlayListItemModel item in (IEnumerable<MusicPlayListItemModel>)_data)
		{
			if (item != null)
			{
				item.isPlaying = item.audioInfo == music;
				item.isPausing = isPaused && item.isPlaying;
			}
		}
		_dragManipulator.UpdatePlayState(music, isPaused, _isRemovingMode);
		foreach (MusicPlayListItemViewsHolder visibleItem in _VisibleItems)
		{
			bool isPlaceHolder = _dragManipulator.IsPlaceHolderModel(visibleItem.View.PlayListItemModel);
			visibleItem.UpdateModel(visibleItem.View.PlayListItemModel, isPlaceHolder, _isRemovingMode);
		}
	}

	public void UpdateVisible()
	{
		if (!base.IsInitialized || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		_dragManipulator.UpdateState(_isRemovingMode);
		foreach (MusicPlayListItemViewsHolder visibleItem in _VisibleItems)
		{
			bool isPlaceHolder = _dragManipulator.IsPlaceHolderModel(visibleItem.View.PlayListItemModel);
			visibleItem.UpdateModel(visibleItem.View.PlayListItemModel, isPlaceHolder, _isRemovingMode);
		}
	}

	public void OnImportLocalMusic()
	{
		if (_facilityMusic.MusicService.IsPlayListDirtyForLocalImport)
		{
			_facilityMusic.MusicService.OnEndAdjustPlayListForLocalImport();
		}
		if (_facilityMusic.MusicService.HasFlagInCurrentTag(AudioTag.Local))
		{
			ScrollTo(_data.Count - 1, 0.5f, 0.5f);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!base.IsInitialized)
		{
			return;
		}
		_dragManipulator.Update();
		foreach (string request in _removeRequest)
		{
			(MusicPlayListItemModel, int) tuple = _data.Indexed().FirstOrDefault(((MusicPlayListItemModel item, int index) x) => x.item.audioInfo.UUID == request);
			_facilityMusic.MusicService.RemoveLocalMusicItem(tuple.Item1.audioInfo);
		}
		_removeRequest.Clear();
	}

	protected override MusicPlayListItemViewsHolder CreateViewsHolder(int itemIndex)
	{
		MusicPlayListItemViewsHolder instance = new MusicPlayListItemViewsHolder();
		instance.Init(_Params.itemPrefab, _Params.Content, itemIndex);
		instance.DragHandle.Init(_dragManipulator, instance);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickMobileDemoLocked, delegate
		{
			_dialog.Activate();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickPlayButton, delegate
		{
			if (_facilityMusic.PlayingMusic == instance.View.PlayListItemModel.audioInfo)
			{
				_facilityMusic.OnClickButtonPlayOrPauseMusic();
			}
			else
			{
				_facilityMusic.OnClickButtonPlayListPlayMusicButton(instance.View.PlayListItemModel.audioInfo);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickFavoriteButton, delegate
		{
			if (!instance.View.PlayListItemModel.audioInfo.Tag.HasFlagFast(AudioTag.Favorite))
			{
				_facilityMusic.MusicService.RegisterFavoriteMusic(instance.View.PlayListItemModel.audioInfo);
			}
			else
			{
				_facilityMusic.MusicService.UnregisterFavoriteMusic(instance.View.PlayListItemModel.audioInfo);
			}
			instance.View.SetFavoriteImage();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickCheckBox, delegate
		{
			if (_facilityMusic.MusicService.IsContainsExcludedFromPlaylist(instance.View.PlayListItemModel.audioInfo))
			{
				_facilityMusic.MusicService.IncludeInPlaylist(instance.View.PlayListItemModel.audioInfo);
			}
			else
			{
				_facilityMusic.MusicService.ExcludeFromPlaylist(instance.View.PlayListItemModel.audioInfo);
			}
			instance.View.PlayListItemModel.isExclude = _facilityMusic.MusicService.IsContainsExcludedFromPlaylist(instance.View.PlayListItemModel.audioInfo);
			instance.View.SetPlayCandidateImage();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(instance.View.OnClickRemoveButton, delegate
		{
			if (instance.View.PlayListItemModel.audioInfo.Tag.HasFlagFast(AudioTag.Local))
			{
				_systemSeService.PlayCancel();
				_animation.Play(instance.View, _data[instance.ItemIndex].audioInfo.UUID, _Params.DefaultItemSize, 0.1f).Forget();
			}
		}).AddTo(this);
		return instance;
	}

	protected override void UpdateViewsHolder(MusicPlayListItemViewsHolder item)
	{
		MusicPlayListItemModel model = _data[item.ItemIndex];
		bool isPlaceHolder = _dragManipulator.IsPlaceHolderModel(model);
		item.SetModel(model, isPlaceHolder, _isRemovingMode);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSizeAndUpdateLayout(int idx, float size, bool endEdgeStationary)
	{
		RequestChangeItemSizeAndUpdateLayout(idx, size, endEdgeStationary);
	}

	void IInfomationProviderForOSAMyAnimation.RequestChangeItemSize(int idx, float size)
	{
	}

	int IOSAModelIndexGetter<string>.GetModelIndex(string equatable)
	{
		foreach (var item in _data.Indexed())
		{
			if (equatable.Equals(item.item.audioInfo.UUID))
			{
				return item.index;
			}
		}
		return -1;
	}
}
