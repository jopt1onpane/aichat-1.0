using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using ObservableCollections;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulbul;

public class MusicPlayListView : MonoBehaviour
{
	private class ReorderInfo
	{
		public readonly IMusicPlayListButton Button;

		public readonly int OriginIndex;

		public ReorderInfo(IMusicPlayListButton button, int originIndex)
		{
			Button = button;
			OriginIndex = originIndex;
		}
	}

	[SerializeField]
	private GameObject _dummyButton;

	[SerializeField]
	[Header("プレイリストオブジェクト")]
	private GameObject _playlistParentObject;

	[SerializeField]
	[Header("プレイリストのボタンプレハブ")]
	private GameObject _playListButtonsPrefab;

	[SerializeField]
	[Header("プレイリストのボタンの親オブジェクト")]
	private GameObject _playListButtonsParent;

	[SerializeField]
	[Header("プレイリストのScrollRect")]
	private ScrollRect _scrollRect;

	[SerializeField]
	private Scrollbar _scrollbar;

	private IMusicPlayListButton _dummyPlayListButton;

	private FacilityMusic _facilityMusic;

	private ReorderInfo _reorderInfo;

	private readonly List<IMusicPlayListButton> _playListButtonList = new List<IMusicPlayListButton>();

	private IReadOnlyObservableList<GameAudioInfo> _playingList;

	private DisposableBag _listViewDisposable;

	private DateTime _lastScrollDragTime = DateTime.MinValue;

	public void ResetLastScrollDragTime()
	{
		_lastScrollDragTime = DateTime.MinValue;
	}

	public DateTime CopyLastScrollDragTime(float offsetSec)
	{
		return _lastScrollDragTime.AddMinutes(offsetSec);
	}

	public void ScrollToPlayingMusic(string title)
	{
		foreach (IMusicPlayListButton playListButton in _playListButtonList)
		{
			if (playListButton.AudioInfo.Title.Equals(title))
			{
				_scrollRect.ScrollToCenter(playListButton.RectTransform);
				break;
			}
		}
	}

	public void Setup(FacilityMusic facilityMusic, IReadOnlyObservableList<GameAudioInfo> playingList)
	{
		_facilityMusic = facilityMusic;
		_dummyPlayListButton = _dummyButton.GetComponent<IMusicPlayListButton>();
		_playingList = playingList;
		ObservableSubscribeExtensions.Subscribe(Observable.Merge<PointerEventData>(ObservableTriggerExtensions.OnBeginDragAsObservable(_scrollbar), ObservableTriggerExtensions.OnEndDragAsObservable(_scrollbar), ObservableTriggerExtensions.OnDragAsObservable(_scrollbar), ObservableTriggerExtensions.OnBeginDragAsObservable(_scrollRect), ObservableTriggerExtensions.OnEndDragAsObservable(_scrollRect), ObservableTriggerExtensions.OnDragAsObservable(_scrollRect)), delegate
		{
			_lastScrollDragTime = DateTime.Now;
		}).AddTo(this);
	}

	public void ViewPlayList()
	{
		foreach (IMusicPlayListButton playListButton in _playListButtonList)
		{
			UnityEngine.Object.DestroyImmediate(playListButton.RectTransform.gameObject);
		}
		_playListButtonList.Clear();
		IMusicPlayListButton playing = null;
		foreach (GameAudioInfo playing2 in _playingList)
		{
			GameObject obj = UnityEngine.Object.Instantiate(_playListButtonsPrefab, _playListButtonsParent.transform, worldPositionStays: false);
			Vector3 localPosition = obj.gameObject.transform.localPosition;
			localPosition.z = 0f;
			obj.gameObject.transform.localPosition = localPosition;
			obj.gameObject.transform.localScale = Vector3.one;
			IMusicPlayListButton component = obj.GetComponent<IMusicPlayListButton>();
			component.Setup(playing2, _facilityMusic);
			_playListButtonList.Add(component);
			if (playing2 == _facilityMusic.PlayingMusic)
			{
				playing = component;
			}
		}
		_listViewDisposable.DisposeAndRecreate();
		_playListButtonList.Select((IMusicPlayListButton b) => b.OnStartReorder).Merge().Subscribe(delegate((IMusicPlayListButton button, PointerEventData eventData) x)
		{
			OnStartReorder(x.button, x.eventData);
		})
			.AddTo(ref _listViewDisposable);
		_playListButtonList.Select((IMusicPlayListButton b) => b.OnReorderDrag).Merge().Subscribe(delegate((IMusicPlayListButton button, PointerEventData eventData) x)
		{
			OnDragReorder(x.button, x.eventData);
		})
			.AddTo(ref _listViewDisposable);
		_playListButtonList.Select((IMusicPlayListButton b) => b.OnEndReorder).Merge().Subscribe(delegate((IMusicPlayListButton button, PointerEventData eventData) x)
		{
			OnEndReorder(x.button, x.eventData);
		})
			.AddTo(ref _listViewDisposable);
		if (playing == null && !_facilityMusic.MusicService.IsPlayListDirtyForLocalImport && !_facilityMusic.MusicService.IsPlayListDirtyForLocalRemove)
		{
			return;
		}
		UniTask.Void(async delegate
		{
			await UniTask.NextFrame();
			if (_facilityMusic.MusicService.IsPlayListDirtyForLocalImport)
			{
				_scrollRect.verticalNormalizedPosition = 0f;
				_facilityMusic.MusicService.OnEndAdjustPlayListForLocalImport();
			}
			else if (_facilityMusic.MusicService.IsPlayListDirtyForLocalRemove)
			{
				_facilityMusic.MusicService.OnEndAdjustPlayListForLocalRemove();
			}
			else
			{
				_scrollRect.ScrollToCenter(playing.RectTransform);
			}
		});
	}

	private void OnStartReorder(IMusicPlayListButton button, PointerEventData eventData)
	{
		if (_reorderInfo == null)
		{
			_reorderInfo = new ReorderInfo(button, button.RectTransform.GetSiblingIndex());
			_dummyPlayListButton.Setup(button.AudioInfo, _facilityMusic);
			_dummyPlayListButton.ActivateDragAnimation();
			_dummyPlayListButton.RectTransform.gameObject.SetActive(value: true);
			button.Hide();
		}
	}

	private void OnDragReorder(IMusicPlayListButton button, PointerEventData eventData)
	{
		if (_reorderInfo == null || _reorderInfo.Button != button)
		{
			return;
		}
		Vector3 position = _dummyPlayListButton.RectTransform.position;
		position.y = eventData.position.y;
		_dummyPlayListButton.RectTransform.position = position;
		int num = -1;
		for (int i = 0; i < _playListButtonsParent.transform.childCount; i++)
		{
			Transform child = _playListButtonsParent.transform.GetChild(i);
			if (!(_dummyPlayListButton.RectTransform.position.y + _dummyPlayListButton.RectTransform.rect.height * 0.5f < child.position.y))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			button.RectTransform.SetAsLastSibling();
		}
		else
		{
			button.RectTransform.SetSiblingIndex(num);
		}
	}

	private void OnEndReorder(IMusicPlayListButton button, PointerEventData eventData)
	{
		if (_reorderInfo != null && _reorderInfo.Button == button)
		{
			_dummyPlayListButton.DeactivateDragAnimation();
			_dummyPlayListButton.RectTransform.gameObject.SetActive(value: false);
			int originIndex = _reorderInfo.OriginIndex;
			int siblingIndex = button.RectTransform.GetSiblingIndex();
			_reorderInfo = null;
			button.Show();
			if (originIndex != siblingIndex)
			{
				GameAudioInfo origin = ((siblingIndex == 0) ? null : _playListButtonsParent.transform.GetChild(siblingIndex - 1).GetComponent<IMusicPlayListButton>().AudioInfo);
				_facilityMusic.MusicService.SwapAfter(button.AudioInfo, origin);
			}
		}
	}

	public T[] CreateButtonArray<T>() where T : class, IMusicPlayListButton
	{
		return (from _ in _playListButtonList
			where _ is T
			select (T)_).ToArray();
	}
}
