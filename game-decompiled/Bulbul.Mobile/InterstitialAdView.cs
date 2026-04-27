using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class InterstitialAdView : MonoBehaviour
{
	[Inject]
	private LoadingScreen _loadingScreen;

	[SerializeField]
	[Header("広告中のタップ防止用")]
	private Image _adRaycastBlocker;

	[SerializeField]
	[Header("CM入ります的な予告")]
	private CanvasGroup _noticeCanvasGroup;

	public void Setup()
	{
		_noticeCanvasGroup.alpha = 0f;
		_noticeCanvasGroup.gameObject.SetActive(value: false);
		_adRaycastBlocker.enabled = false;
	}

	public async UniTask PlayNoticeAnimation(CancellationToken ct)
	{
		_noticeCanvasGroup.gameObject.SetActive(value: true);
		_noticeCanvasGroup.alpha = 0f;
		await _noticeCanvasGroup.DOFade(1f, 0.2f).ToUniTask(TweenCancelBehaviour.CancelAwait, ct);
		await UniTask.WaitForSeconds(1f, ignoreTimeScale: false, PlayerLoopTiming.Update, ct);
		_noticeCanvasGroup.gameObject.SetActive(value: false);
		_noticeCanvasGroup.alpha = 0f;
	}

	public void SetActiveAdRaycastBlocker(bool active)
	{
		_adRaycastBlocker.enabled = active;
	}
}
