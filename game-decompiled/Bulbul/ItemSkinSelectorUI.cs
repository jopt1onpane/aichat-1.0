using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.Pool;

namespace Bulbul;

public class ItemSkinSelectorUI : MonoBehaviour
{
	[SerializeField]
	private ItemSkinButton _itemButtonOriginal;

	private ObjectPool<ItemSkinButton> _itemButtonPool;

	private List<ItemSkinButton> _itemButtons = new List<ItemSkinButton>();

	private DisposableBag _showDisposables;

	public string Target { get; private set; }

	public void Initialize()
	{
		_itemButtonOriginal.gameObject.SetActive(value: false);
		_itemButtonPool = new ObjectPool<ItemSkinButton>(() => UnityEngine.Object.Instantiate(_itemButtonOriginal, _itemButtonOriginal.transform.parent), delegate(ItemSkinButton button)
		{
			button.gameObject.SetActive(value: true);
		}, delegate(ItemSkinButton button)
		{
			button.gameObject.SetActive(value: false);
		});
	}

	public void SetData<T>(string target, IEnumerable<T> items, Action<ItemSkinButton, T> setupButton, IDisposable showDisposable)
	{
		_showDisposables.Clear();
		Target = target;
		ReleaseButtons();
		foreach (T item in items)
		{
			ItemSkinButton itemSkinButton = _itemButtonPool.Get();
			_itemButtons.Add(itemSkinButton);
			itemSkinButton.transform.SetAsLastSibling();
			setupButton(itemSkinButton, item);
		}
		showDisposable.AddTo(ref _showDisposables);
	}

	private void ReleaseButtons()
	{
		foreach (ItemSkinButton itemButton in _itemButtons)
		{
			itemButton.Reset();
			_itemButtonPool.Release(itemButton);
		}
		_itemButtons.Clear();
	}
}
