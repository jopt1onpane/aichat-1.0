using Bulbul;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private enum MainState
	{
		Idle,
		OnDrag
	}

	private MainState _mainState;

	[SerializeField]
	[Header("オブジェクトの範囲※中心座標必須\u3000画面外判定で使用")]
	private RectTransform _dragObjectRect;

	private Camera _canvasCamera;

	public Vector3 CurrentDragAmount { get; private set; }

	public void Setup()
	{
		_canvasCamera = GetComponent<Graphic>().canvas.worldCamera;
	}

	public Vector3 CalculateDragPos(Graphic ownerGraphic, Vector3 ownerWorldPosition)
	{
		Vector3 result = ownerWorldPosition;
		if (_mainState == MainState.OnDrag)
		{
			if (!Input.GetMouseButton(0))
			{
				_mainState = MainState.Idle;
			}
			else
			{
				Vector3 vector = InputController.Instance.GetInputPos();
				Vector3 vector2 = RectTransformUtility.WorldToScreenPoint(_canvasCamera, base.gameObject.transform.position);
				CurrentDragAmount = vector - vector2;
				Camera cam = ownerGraphic.canvas.worldCamera;
				if (ownerGraphic.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
				{
					cam = null;
				}
				RectTransformUtility.ScreenPointToWorldPointInRectangle(ownerGraphic.rectTransform, CurrentDragAmount, cam, out var worldPoint);
				result = ownerWorldPosition + worldPoint;
				result = AdjustPos(ownerWorldPosition, result);
			}
		}
		return result;
	}

	public Vector3 AdjustPos(Vector3 currentWorldPos, Vector3 newWorldPos)
	{
		Vector3 lossyScale = GetComponent<Graphic>().canvas.GetComponent<RectTransform>().lossyScale;
		Vector3 position = _dragObjectRect.position;
		Vector3 vector = newWorldPos - currentWorldPos;
		Vector3 vector2 = position + vector;
		Vector3 vector3 = newWorldPos - vector2;
		float num = _dragObjectRect.rect.width * 0.5f * lossyScale.x;
		int num2 = 0;
		int width = Screen.width;
		if (vector2.x - num < (float)num2)
		{
			vector2.x = (float)num2 + num;
		}
		else if (vector2.x + num > (float)width)
		{
			vector2.x = (float)width - num;
		}
		float num3 = _dragObjectRect.rect.height * 0.5f * lossyScale.y;
		int height = Screen.height;
		int num4 = 0;
		if (vector2.y + num3 > (float)height)
		{
			vector2.y = (float)height - num3;
		}
		else if (vector2.y - num3 < (float)num4)
		{
			vector2.y = (float)num4 + num3;
		}
		return vector2 + vector3;
	}

	public bool IsDrag()
	{
		return _mainState == MainState.OnDrag;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		InputController.Instance.GetInputPos();
		_mainState = MainState.OnDrag;
	}
}
