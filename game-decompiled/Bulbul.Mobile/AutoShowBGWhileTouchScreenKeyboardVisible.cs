using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class AutoShowBGWhileTouchScreenKeyboardVisible : MonoBehaviour
{
	[SerializeField]
	private Image _bgImage;

	private void Awake()
	{
		_bgImage.gameObject.SetActive(value: true);
		_bgImage.enabled = false;
	}

	private void Update()
	{
		bool visible = TouchScreenKeyboard.visible;
		_bgImage.enabled = visible;
	}

	private void OnDisable()
	{
		_bgImage.enabled = false;
	}
}
