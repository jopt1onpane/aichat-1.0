using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class StoryNormalTextBGView : MonoBehaviour
{
	[SerializeField]
	private SubTitleAutoFitter _subTitleAutoFitter;

	[SerializeField]
	[Header("Fitterの結果に対して余分な高さを追加したり減らしたりする変数")]
	private Vector2 _addSize = new Vector2(0f, 30f);

	[SerializeField]
	[Header("最低でも確保する横幅")]
	private float _minWidth = 100f;

	private RectTransform _rectTransform;

	private void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
		anchoredPosition.y -= _addSize.y * 0.5f;
		_rectTransform.anchoredPosition = anchoredPosition;
		_subTitleAutoFitter.OnLayoutChangedSize.Subscribe(delegate(Vector2 size)
		{
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(_minWidth, size.x + _addSize.x));
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y + _addSize.y);
		}).AddTo(this);
	}
}
