using UnityEngine;

namespace Bulbul;

public class UICanvasProvider : MonoBehaviour, IUICanvasProvider
{
	[SerializeField]
	private GameObject _uiParent;

	[SerializeField]
	private CanvasGroup _uiParentCanvasGroup;

	public GameObject UIParent => _uiParent;

	public CanvasGroup UIParentCanvasGroup => _uiParentCanvasGroup;

	[field: SerializeField]
	public RectTransform CommonDialogParent { get; private set; }
}
