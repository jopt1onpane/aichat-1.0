using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulbul.Mobile;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldActionWhenInputRelease : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
	private TMP_InputField __inputField;

	private bool _isRequestInput;

	private Vector2 _downPos;

	private Vector2 _upPos;

	public static Vector2 BaseScreenResolution { get; set; } = new Vector2(1080f, 1920f);

	private TMP_InputField _inputField
	{
		get
		{
			if (__inputField == null)
			{
				__inputField = GetComponent<TMP_InputField>();
			}
			return __inputField;
		}
	}

	private void Cancel()
	{
		_isRequestInput = false;
		_inputField.enabled = false;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Cancel();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Cancel();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_isRequestInput = true;
		_inputField.enabled = false;
		_downPos = eventData.position;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_isRequestInput)
		{
			_upPos = eventData.position;
			Vector2 vector = _upPos - _downPos;
			if (!((vector * CalcMovedFactor()).magnitude >= 10f))
			{
				_inputField.enabled = true;
				_inputField.ActivateInputField();
				_isRequestInput = false;
			}
		}
	}

	private Vector2 CalcMovedFactor()
	{
		float x = ((Screen.width <= 0) ? 1f : ((float)Screen.width / BaseScreenResolution.x));
		float y = ((Screen.height <= 0) ? 1f : ((float)Screen.height / BaseScreenResolution.y));
		return new Vector2(x, y);
	}
}
