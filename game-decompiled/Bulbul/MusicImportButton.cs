using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class MusicImportButton : MonoBehaviour
{
	[SerializeField]
	private Button _button;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	private Tween _tween;

	public Observable<Unit> OnClickAsObservable()
	{
		return _button.OnClickAsObservable();
	}

	public void SetInteractable(bool isInteractable)
	{
		_canvasGroup.blocksRaycasts = isInteractable;
		_tween?.Kill();
		float duration = (isInteractable ? 0.2f : 0.2f);
		_tween = _canvasGroup.DOFade(isInteractable ? 1f : 0.5f, duration).SetLink(base.gameObject);
	}
}
