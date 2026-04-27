using TMPro;
using UnityEngine;

namespace Bulbul.ScrollListSample;

public class DraggableItemView : MonoBehaviour
{
	public GameObject Content;

	public TMP_Text TitleText;

	public ItemDragReorderHandle DragHandle;

	public float DraggingScale = 0.8f;
}
