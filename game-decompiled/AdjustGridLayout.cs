using UnityEngine;
using UnityEngine.UI;

public class AdjustGridLayout : MonoBehaviour
{
	private GridLayoutGroup _gridLayoutGroup;

	private GameObject[] _contents;

	private RectTransform _rectTransform;

	private RectTransform rectTransform
	{
		get
		{
			if (!_rectTransform)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public void Setup()
	{
		_gridLayoutGroup = base.gameObject.GetComponent<GridLayoutGroup>();
		_contents = new GameObject[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			_contents[i] = base.transform.GetChild(i).gameObject;
		}
		AdjustRectSize();
	}

	public void AdjustRectSize()
	{
		int num = 0;
		for (int i = 0; i < _contents.Length; i++)
		{
			if (_contents[i].activeSelf)
			{
				num++;
			}
		}
		int num2 = num / _gridLayoutGroup.constraintCount;
		if (num % _gridLayoutGroup.constraintCount != 0)
		{
			num2++;
		}
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = (float)num2 * (_gridLayoutGroup.cellSize.y + _gridLayoutGroup.spacing.y);
		rectTransform.sizeDelta = sizeDelta;
	}
}
