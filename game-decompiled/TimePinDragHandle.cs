using Bulbul;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class TimePinDragHandle : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	public void OnPointerDown(PointerEventData eventData)
	{
		RectTransform pinRoot = GetPinRoot();
		if (!(pinRoot == null))
		{
			AutoTimeWindowView componentInParent = pinRoot.GetComponentInParent<AutoTimeWindowView>();
			if (componentInParent != null)
			{
				componentInParent.BeginDragPin(pinRoot);
			}
		}
	}

	private RectTransform GetPinRoot()
	{
		Transform parent = base.transform;
		while (parent.parent != null && parent.parent.name != "TimePins")
		{
			parent = parent.parent;
		}
		if (!(parent.parent != null))
		{
			return null;
		}
		return parent as RectTransform;
	}
}
