using System.Collections.Generic;
using Bulbul;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

public class SelectPageListUI : MonoBehaviour
{
	private class ReorderInfo
	{
		public readonly SelectPageUI Button;

		public readonly int OriginIndex;

		public ReorderInfo(SelectPageUI button, int originIndex)
		{
			Button = button;
			OriginIndex = originIndex;
		}
	}

	[Inject]
	private NoteService _noteService;

	[SerializeField]
	[Header("SelectPage用プレハブ")]
	private SelectPageUI _selectPageUIPrefab;

	[SerializeField]
	[Header("SelectPageの親オブジェクト")]
	private GameObject _selectPageUIParent;

	[SerializeField]
	[Header("ページ選択UI追加ボタン")]
	private Button _addSelectPageUIButton;

	[SerializeField]
	[Header("ページのScrollRect")]
	private ScrollRect _scrollRect;

	private ReorderInfo reorderInfo;

	private readonly List<SelectPageUI> _selectPageList = new List<SelectPageUI>();

	[SerializeField]
	private SelectPageUI dummyButton;

	public void Setup()
	{
		dummyButton.Init();
		ObservableSubscribeExtensions.Subscribe(_addSelectPageUIButton.OnClickAsObservable(), delegate
		{
			_noteService.AddSelectPageUI();
		}).AddTo(this);
		_noteService.OnAddPage.Subscribe(delegate(PageDataV2 pageData)
		{
			SelectPageUI component = Object.Instantiate(_selectPageUIPrefab.gameObject, _selectPageUIParent.transform).GetComponent<SelectPageUI>();
			component.Setup(pageData);
			component.OnStartReorder.Subscribe(delegate((SelectPageUI button, PointerEventData eventData) x)
			{
				OnStartReorder(x.button, x.eventData);
			}).AddTo(this);
			component.OnReorderDrag.Subscribe(delegate((SelectPageUI button, PointerEventData eventData) x)
			{
				OnDragReorder(x.button, x.eventData);
			}).AddTo(this);
			component.OnEndReorder.Subscribe(delegate((SelectPageUI button, PointerEventData eventData) x)
			{
				OnEndReorder(x.button, x.eventData);
			}).AddTo(this);
			_selectPageList.Add(component);
			if (_noteService.IsFinishedInitialize)
			{
				component.SelectPage();
				component.EditTitle();
				float endValue = 0f;
				_scrollRect.DOVerticalNormalizedPos(endValue, 0.5f);
			}
		}).AddTo(this);
	}

	private void OnStartReorder(SelectPageUI button, PointerEventData eventData)
	{
		if (reorderInfo == null)
		{
			button.SelectPage();
			reorderInfo = new ReorderInfo(button, button.transform.GetSiblingIndex());
			dummyButton.SetupForDummy(new PageDataV2
			{
				UniqueID = button.PageID
			});
			dummyButton.gameObject.SetActive(value: true);
			button.Hide();
		}
	}

	private void OnDragReorder(SelectPageUI button, PointerEventData eventData)
	{
		if (reorderInfo == null || reorderInfo.Button != button)
		{
			return;
		}
		Vector3 position = dummyButton.transform.position;
		position.y = eventData.position.y;
		dummyButton.transform.position = position;
		int num = -1;
		for (int i = 0; i < _selectPageUIParent.transform.childCount; i++)
		{
			Transform child = _selectPageUIParent.transform.GetChild(i);
			if (!(dummyButton.transform.position.y + (dummyButton.transform as RectTransform).rect.height * 0.5f < child.position.y))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			button.transform.SetAsLastSibling();
		}
		else
		{
			button.transform.SetSiblingIndex(num);
		}
	}

	private void OnEndReorder(SelectPageUI button, PointerEventData eventData)
	{
		if (reorderInfo != null && !(reorderInfo.Button != button))
		{
			dummyButton.gameObject.SetActive(value: false);
			dummyButton.TidyingEndDrag(isUseDoComplete: true);
			int originIndex = reorderInfo.OriginIndex;
			int siblingIndex = button.transform.GetSiblingIndex();
			reorderInfo = null;
			button.Show();
			if (originIndex != siblingIndex)
			{
				ulong origin = ((siblingIndex == 0) ? 0 : _selectPageUIParent.transform.GetChild(siblingIndex - 1).GetComponent<SelectPageUI>().PageID);
				_noteService.SwapAfter(button.PageID, origin);
			}
		}
	}
}
