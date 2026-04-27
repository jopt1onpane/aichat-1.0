using TMPro;
using UnityEngine;

namespace Bulbul;

public class InputFieldForceFocusDeactivate : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField _input;

	private bool _isCurrentFocus;

	private void LateUpdate()
	{
		if (_input != null && !_input.isFocused && _isCurrentFocus != _input.isFocused)
		{
			_input.ReleaseSelection();
		}
		_isCurrentFocus = _input.isFocused;
	}
}
