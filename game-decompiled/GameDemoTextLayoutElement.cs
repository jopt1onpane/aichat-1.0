using UnityEngine;
using UnityEngine.UI;

public class GameDemoTextLayoutElement : MonoBehaviour, ILayoutElement
{
	[SerializeField]
	private HorizontalOrVerticalLayoutGroup _layoutGroup;

	[SerializeField]
	private int _layoutPriority = 1;

	public float minWidth => _layoutGroup.minWidth;

	public float preferredWidth => _layoutGroup.preferredWidth;

	public float flexibleWidth => _layoutGroup.flexibleWidth;

	public float minHeight => _layoutGroup.minHeight;

	public float preferredHeight => _layoutGroup.preferredHeight;

	public float flexibleHeight => _layoutGroup.flexibleHeight;

	public int layoutPriority => _layoutPriority;

	public void CalculateLayoutInputHorizontal()
	{
		_layoutGroup.CalculateLayoutInputHorizontal();
	}

	public void CalculateLayoutInputVertical()
	{
		_layoutGroup.CalculateLayoutInputVertical();
	}
}
