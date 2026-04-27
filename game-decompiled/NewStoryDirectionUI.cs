using DG.Tweening;
using UnityEngine;

public class NewStoryDirectionUI : MonoBehaviour
{
	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("フェード秒数")]
	private float _fadeSeconds;

	[SerializeField]
	[Header("フェードアウト待機秒数")]
	private float _fadeOutDelaySeconds;

	[SerializeField]
	[Header("Y移動量")]
	private float _moveAmountY;

	private RectTransform _rectTransform;

	private float _fromPosY;

	private float _toPosY;

	public void Setup()
	{
		_rectTransform = GetComponent<RectTransform>();
		_canvasGroup.alpha = 0f;
		_fromPosY = _rectTransform.anchoredPosition.y - _moveAmountY;
		_toPosY = _rectTransform.anchoredPosition.y;
		Vector2 anchoredPosition = _rectTransform.anchoredPosition;
		_rectTransform.anchoredPosition = new Vector3(anchoredPosition.x, _fromPosY);
	}

	public void StartDirection()
	{
		base.gameObject.SetActive(value: true);
		_rectTransform.DOAnchorPosY(_toPosY, _fadeSeconds);
		_canvasGroup.DOFade(1f, _fadeSeconds).OnComplete(delegate
		{
			_canvasGroup.DOFade(0f, _fadeSeconds).SetDelay(_fadeOutDelaySeconds).OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
			});
			_rectTransform.DOAnchorPosY(_fromPosY, _fadeSeconds).SetDelay(_fadeOutDelaySeconds);
		});
	}
}
