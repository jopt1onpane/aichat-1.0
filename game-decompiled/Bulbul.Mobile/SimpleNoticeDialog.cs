using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SimpleNoticeDialog : MonoBehaviour
{
	[SerializeField]
	private float _autoCloseSec = 2f;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private CanvasGroup _rootCanvasGroup;

	[SerializeField]
	private TextLocalizationBehaviour _textLocalizationBehaviour;

	private CancellationTokenSource _cancellationTokenSource;

	private bool _isClosing;

	private Tween _tween;

	public void Start()
	{
		if (!(_closeButton != null))
		{
			return;
		}
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			if (!_isClosing)
			{
				CancelWait();
				Deactivate();
			}
		}).AddTo(this);
	}

	private void OnDestroy()
	{
		CancelWait();
	}

	private void OnDisable()
	{
		DeactivateImmidiate();
	}

	public void SetLocalizeID(string localizeID)
	{
		_textLocalizationBehaviour.Set(localizeID);
	}

	public void Activate()
	{
		_tween?.Kill();
		_rootCanvasGroup.alpha = 1f;
		_rootCanvasGroup.blocksRaycasts = true;
		base.gameObject.SetActive(value: true);
		WaitAsync().Forget();
	}

	private async UniTask WaitAsync()
	{
		CancelWait();
		_cancellationTokenSource = new CancellationTokenSource();
		if (!(_autoCloseSec <= 0f))
		{
			await UniTask.WaitForSeconds(_autoCloseSec, ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
		}
		else
		{
			await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
		}
		Deactivate();
	}

	private void Deactivate()
	{
		_tween?.Kill();
		_isClosing = true;
		_rootCanvasGroup.blocksRaycasts = false;
		_tween = _rootCanvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			_isClosing = false;
			base.gameObject.SetActive(value: false);
		});
	}

	public void DeactivateImmidiate()
	{
		_tween?.Kill();
		_rootCanvasGroup.blocksRaycasts = false;
		base.gameObject.SetActive(value: false);
		_rootCanvasGroup.alpha = 0f;
		_isClosing = false;
	}

	private void CancelWait()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
	}
}
