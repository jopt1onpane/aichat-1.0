using System.Linq;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class ChangeOrderOnClick : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	[Inject]
	private ChangeOrderService _changeOrderService;

	[SerializeField]
	private ChangeOrderService.OrderItemType _itemType;

	private void Awake()
	{
		(from x in GetComponentsInChildren<Selectable>()
			select x.OnPointerDownAsObservable()).Merge().Subscribe(delegate(PointerEventData x)
		{
			OnPointerDown(x);
		}).AddTo(this);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_changeOrderService.BringToFront(_itemType);
	}
}
