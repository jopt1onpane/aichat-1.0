using NestopiSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul;

public class ScrollForwarder : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private IScrollHandler _scrollRect;

	private IScrollHandler scrollRect
	{
		get
		{
			if (_scrollRect.IsNullOrDestroy())
			{
				_scrollRect = base.transform.parent.GetComponentInParent<IScrollHandler>();
			}
			return _scrollRect;
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!scrollRect.IsNullOrDestroy())
		{
			scrollRect.OnScroll(eventData);
		}
	}
}
