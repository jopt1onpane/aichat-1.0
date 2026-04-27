using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SpecialSelectListUIForMobile : MonoBehaviour
{
	[SerializeField]
	[Header("ScrollRect")]
	private ScrollRect _scrollRect;

	public void ResetScroll()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.verticalNormalizedPosition = 1f;
	}
}
