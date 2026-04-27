using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul;

public class InputController
{
	public static InputController Instance = new InputController();

	private readonly List<RaycastResult> _currentFrameEventSystemRaycastResult = new List<RaycastResult>();

	public IReadOnlyList<RaycastResult> CurrentFrameEventSystemRaycastResult => _currentFrameEventSystemRaycastResult;

	public GameObject CurrentFrameEventSystemRaycastHitObject
	{
		get
		{
			if (_currentFrameEventSystemRaycastResult.Count > 0)
			{
				GameObject gameObject = _currentFrameEventSystemRaycastResult[0].gameObject;
				if (gameObject != null)
				{
					return gameObject;
				}
			}
			return null;
		}
	}

	public GameObject PointerDownObject { get; private set; }

	public Vector3 MouseDownPos { get; private set; }

	public bool Dragged { get; private set; }

	public void Update()
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		EventSystem.current.RaycastAll(pointerEventData, _currentFrameEventSystemRaycastResult);
		if (GetClickDown())
		{
			MouseDownPos = Input.mousePosition;
			Dragged = false;
			PointerDownObject = CurrentFrameEventSystemRaycastHitObject;
		}
		if (!Dragged && Input.GetMouseButton(0) && (Input.mousePosition - MouseDownPos).sqrMagnitude > (float)(EventSystem.current.pixelDragThreshold * EventSystem.current.pixelDragThreshold))
		{
			Dragged = true;
		}
	}

	public Vector2 GetInputPos()
	{
		return Input.mousePosition;
	}

	public bool GetClickDown()
	{
		return Input.GetMouseButtonDown(0);
	}

	public bool GetClickUp()
	{
		return Input.GetMouseButtonUp(0);
	}
}
