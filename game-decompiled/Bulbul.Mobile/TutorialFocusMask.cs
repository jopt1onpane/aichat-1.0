using UnityEngine;

namespace Bulbul.Mobile;

public class TutorialFocusMask : MonoBehaviour
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private RectTransform _unMaskRectTransform;

	public void SetActive(bool active)
	{
		if (_root.activeSelf != active)
		{
			_root.SetActive(active);
		}
	}

	public void SetUnMaskPosAndSize(Vector3 position, Vector2 size)
	{
		_unMaskRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		_unMaskRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		_unMaskRectTransform.position = position;
	}
}
