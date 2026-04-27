using UnityEngine;

namespace Bulbul.Mobile;

public interface IPurchaseUIProvider
{
	RectTransform DialogParent { get; }

	void SetActiveBlockRaycast(bool active);
}
