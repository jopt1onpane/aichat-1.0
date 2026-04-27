using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class HabitDeleteWarningPopover : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private Button _okButton;

	[SerializeField]
	private Button _cancelButton;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private ClickOutsideDetector _clickOutsideDetector;

	[SerializeField]
	private Vector2 _offset;

	private CancellationTokenSource _showHideCts;

	private const float _showMoveY = -10f;

	private bool _isShowing;

	public void Setup()
	{
		base.gameObject.SetActive(value: false);
	}

	public async UniTask<bool> ShowAndWaitResultAsync(Vector3 position)
	{
		_showHideCts?.Cancel();
		_showHideCts?.Dispose();
		_showHideCts = new CancellationTokenSource();
		CancellationToken ct = _showHideCts.Token;
		_isShowing = true;
		_canvasGroup.blocksRaycasts = false;
		base.gameObject.SetActive(value: true);
		RectTransform rectTransform = base.transform as RectTransform;
		rectTransform.position = position + Vector3.Scale(_offset, rectTransform.localScale);
		_systemSeService.PlayPulldownOpen();
		await DOTween.Sequence().Append(rectTransform.DOLocalMoveY(-10f, 0.2f).SetRelative()).Join(_canvasGroup.DOFade(1f, 0.2f))
			.ToUniTask(TweenCancelBehaviour.Kill, ct);
		_canvasGroup.blocksRaycasts = true;
		int num = await UniTask.WhenAny(_okButton.OnClickAsync(ct), _cancelButton.OnClickAsync(ct), _clickOutsideDetector.OnClickOutside.FirstAsync(ct).AsUniTask());
		bool flag = num == 0;
		bool flag2 = num == 1;
		if (flag)
		{
			_systemSeService.PlayCancel();
		}
		else if (flag2)
		{
			_systemSeService.PlayPulldownClose();
		}
		Hide();
		return num == 0;
	}

	public void Hide()
	{
		CancellationToken ct;
		if (_isShowing)
		{
			_isShowing = false;
			_showHideCts?.Cancel();
			_showHideCts?.Dispose();
			_showHideCts = new CancellationTokenSource();
			ct = _showHideCts.Token;
			HideAsync().Forget();
		}
		async UniTask HideAsync()
		{
			RectTransform target = base.transform as RectTransform;
			_canvasGroup.blocksRaycasts = false;
			await DOTween.Sequence().Append(target.DOLocalMoveY(10f, 0.2f).SetRelative()).Join(_canvasGroup.DOFade(0f, 0.2f))
				.ToUniTask(TweenCancelBehaviour.Kill, ct);
			base.gameObject.SetActive(value: false);
		}
	}
}
