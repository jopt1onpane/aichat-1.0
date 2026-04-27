using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ViewportOverTextAutoScroller : MonoBehaviour
{
	private static readonly string space = "    ";

	[SerializeField]
	private RectTransform _viewport;

	[SerializeField]
	private RectTransform _target;

	[SerializeField]
	private TextMeshProUGUI _text;

	[SerializeField]
	private float _startScrollWaitSec = 5f;

	[SerializeField]
	private float _scrollDurationSec = 15f;

	[SerializeField]
	private string baseStr;

	private StringBuilder strBuilder = new StringBuilder();

	private CancellationTokenSource _cancellationTokenSource;

	private Tweener _curScrollTweener;

	private void Start()
	{
		_text.overflowMode = TextOverflowModes.Overflow;
		_text.enableWordWrapping = false;
	}

	private void OnDestroy()
	{
		Cancel();
	}

	private void Cancel()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
		TweenerKill();
	}

	private void TweenerKill()
	{
		if (_curScrollTweener != null)
		{
			_curScrollTweener.Kill();
			_curScrollTweener = null;
		}
	}

	private void OnDisable()
	{
		Cancel();
	}

	private void OnEnable()
	{
		if (!string.IsNullOrEmpty(_text.text))
		{
			SetText(baseStr);
		}
	}

	public void SetText(string str)
	{
		Cancel();
		_text.SetText(str);
		baseStr = str;
		float preferredWidth = _text.preferredWidth;
		ResetPosForHorizontal();
		if (preferredWidth > _viewport.rect.width)
		{
			strBuilder.Clear();
			strBuilder.Append(str);
			strBuilder.Append(space);
			strBuilder.Append(str);
			_text.SetText(strBuilder.ToString());
			float num = _text.preferredWidth - preferredWidth * 2f;
			float targetX = 0f - preferredWidth - num;
			_cancellationTokenSource = new CancellationTokenSource();
			UpdateScrollHorizontal(targetX, _cancellationTokenSource.Token).Forget();
		}
	}

	private void ResetPosForHorizontal()
	{
		if (!(_target == null))
		{
			Vector2 anchoredPosition = _target.anchoredPosition;
			anchoredPosition.x = 0f;
			_target.anchoredPosition = anchoredPosition;
		}
	}

	private async UniTask UpdateScrollHorizontal(float targetX, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		while (true)
		{
			if (_startScrollWaitSec > 0f && await UniTask.WaitForSeconds(_startScrollWaitSec, ignoreTimeScale: false, PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow())
			{
				TweenerKill();
				return;
			}
			_curScrollTweener = _target.DOAnchorPosX(targetX, _scrollDurationSec).SetEase(Ease.Linear);
			if (await UniTask.WaitWhile(() => _curScrollTweener.IsPlaying(), PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow())
			{
				break;
			}
			ResetPosForHorizontal();
		}
		TweenerKill();
	}
}
