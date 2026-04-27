using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class PurchaseUIProvider : MonoBehaviour, IPurchaseUIProvider
{
	[SerializeField]
	private Image _blockRaycast;

	[SerializeField]
	private RectTransform _dialogParent;

	RectTransform IPurchaseUIProvider.DialogParent => _dialogParent;

	void IPurchaseUIProvider.SetActiveBlockRaycast(bool active)
	{
		_blockRaycast.enabled = active;
	}
}
