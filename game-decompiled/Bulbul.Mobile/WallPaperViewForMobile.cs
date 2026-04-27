using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class WallPaperViewForMobile : MonoBehaviour
{
	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	[Header("フェード表示に使う")]
	private CanvasGroup _rootCanvasGroup;

	[SerializeField]
	private ViewportOverTextAutoScroller _musicTitle;

	[SerializeField]
	private GameObject _noMusicObj;

	[SerializeField]
	private GameObject _playingEffectObj;

	[SerializeField]
	private Image _blackImage;

	[SerializeField]
	private CanvasGroup _leftTopUIsCanvasGroup;

	[SerializeField]
	private Button _noAdNoticeButton;

	[SerializeField]
	private Vector2 _portraitNoAdNoticeButtonAncPos;

	[SerializeField]
	private Vector2 _landscapeNoAdNoticeButtonAncPos;

	[SerializeField]
	private SimpleNoticeDialog _noAdsNoticeDialog;

	[SerializeField]
	[Header("親ではなく実際のダイアログ部分を指定")]
	private RectTransform _noAdsNoticeDialogRectTransform;

	[SerializeField]
	private Vector2 _portraitNoAdNoticeDialogAncPos;

	[SerializeField]
	private Vector2 _landscapeNoAdNoticeDialogAncPos;

	[SerializeField]
	[Header("クリックしても壁紙モードを解除させないUI")]
	private RectTransform[] _blockTransitionNormalModeUIObjs;

	private CancellationTokenSource _cancellationTokenSource;

	private CancellationTokenSource _leftTopCancellationTokenSource;

	private GameObject _noAdsObj;

	private RectTransform _noAdsRectTransform;

	private void OnDestroy()
	{
		Cancel();
	}

	private void OnDisable()
	{
		CancelLeftTopUI();
		_leftTopUIsCanvasGroup.alpha = 1f;
	}

	public void Setup()
	{
		_noAdsRectTransform = _noAdNoticeButton.transform as RectTransform;
		_noAdsObj = _noAdNoticeButton.gameObject;
		ObservableSubscribeExtensions.Subscribe(_noAdNoticeButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlaySelect();
			_noAdsNoticeDialog.Activate();
		}).AddTo(this);
	}

	public void Cancel()
	{
		if (_cancellationTokenSource != null)
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = null;
		}
	}

	public void Deactivate()
	{
		_rootCanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
	}

	public async UniTask ActivateAsync(bool isImmediate = false)
	{
		Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		base.gameObject.SetActive(value: true);
		TweenerCore<float, float, FloatOptions> tweenerCore = _rootCanvasGroup.DOFade(1f, 0.2f);
		if (isImmediate)
		{
			tweenerCore.Complete();
		}
		else
		{
			await tweenerCore.WithCancellation(_cancellationTokenSource.Token);
		}
	}

	public async UniTask DeactivateAsync(bool isImmediate = false)
	{
		Cancel();
		_cancellationTokenSource = new CancellationTokenSource();
		base.gameObject.SetActive(value: true);
		TweenerCore<float, float, FloatOptions> tweenerCore = _rootCanvasGroup.DOFade(0f, 0.2f);
		tweenerCore.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		if (isImmediate)
		{
			tweenerCore.Complete();
		}
		else
		{
			await tweenerCore.WithCancellation(_cancellationTokenSource.Token);
		}
	}

	public void CancelLeftTopUI()
	{
		if (_leftTopCancellationTokenSource != null)
		{
			_leftTopCancellationTokenSource.Cancel();
			_leftTopCancellationTokenSource.Dispose();
			_leftTopCancellationTokenSource = null;
		}
	}

	public async UniTask ActivateLeftTopUIsAsync()
	{
		CancelLeftTopUI();
		_leftTopCancellationTokenSource = new CancellationTokenSource();
		await _leftTopUIsCanvasGroup.DOFade(1f, 0.2f).WithCancellation(_leftTopCancellationTokenSource.Token);
	}

	public async UniTask DeactivateLeftTopUIsAsync()
	{
		CancelLeftTopUI();
		_leftTopCancellationTokenSource = new CancellationTokenSource();
		await _leftTopUIsCanvasGroup.DOFade(0f, 0.2f).WithCancellation(_leftTopCancellationTokenSource.Token);
	}

	public void SetMusicTitle(string title)
	{
		_musicTitle.SetText(title);
		_musicTitle.gameObject.SetActive(value: true);
		_noMusicObj.SetActive(value: false);
		_playingEffectObj.SetActive(value: true);
	}

	public void SetNoMusic()
	{
		_noMusicObj.SetActive(value: true);
		_musicTitle.gameObject.SetActive(value: false);
		_playingEffectObj.SetActive(value: false);
	}

	public void SetActiveBlackImage(bool active)
	{
		if (_blackImage.enabled != active)
		{
			_blackImage.enabled = active;
		}
	}

	public bool CheckBlock(GameObject target)
	{
		RectTransform[] blockTransitionNormalModeUIObjs = _blockTransitionNormalModeUIObjs;
		for (int i = 0; i < blockTransitionNormalModeUIObjs.Length; i++)
		{
			if (blockTransitionNormalModeUIObjs[i].gameObject == target)
			{
				return true;
			}
		}
		return false;
	}

	public void SetActiveNoAdsButton(bool active)
	{
		if (_noAdsObj.activeSelf != active)
		{
			_noAdsObj.SetActive(active);
		}
	}

	public void SetPosNoAdsFromScreenOrientation(bool isPortrait)
	{
		if (isPortrait)
		{
			_noAdsRectTransform.anchoredPosition = _portraitNoAdNoticeButtonAncPos;
			_noAdsNoticeDialogRectTransform.anchoredPosition = _portraitNoAdNoticeDialogAncPos;
		}
		else
		{
			_noAdsRectTransform.anchoredPosition = _landscapeNoAdNoticeButtonAncPos;
			_noAdsNoticeDialogRectTransform.anchoredPosition = _landscapeNoAdNoticeDialogAncPos;
		}
	}
}
