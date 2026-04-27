using UnityEngine;

namespace Bulbul;

public interface IUICanvasProvider
{
	GameObject UIParent { get; }

	CanvasGroup UIParentCanvasGroup { get; }

	RectTransform CommonDialogParent { get; }
}
