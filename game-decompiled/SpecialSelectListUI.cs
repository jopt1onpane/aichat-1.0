using Bulbul;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSelectListUI : MonoBehaviour, ISpecialSelectListUI
{
	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[Header("レイキャストの対象になるようにImageを同じオブジェクトにアタッチする必要がある")]
	[SerializeField]
	[Header("ストーリー選択リストオブジェクト")]
	private GameObject _uiParent;

	[SerializeField]
	[Header("ScrollRect")]
	private ScrollRect _scrollRect;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	public void Setup()
	{
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
	}

	public void Activate()
	{
		_uiParent.SetActive(value: true);
		_facilityOpenButton.ActivateUseUI();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.gameObject.GetComponent<RectTransform>());
		_scrollRect.verticalNormalizedPosition = 1f;
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			_uiParent.SetActive(value: false);
		});
	}
}
