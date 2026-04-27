using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class DecorationPurchaseDialog : DialogBase
{
	[Inject]
	private UnlockItemService _unlockItemService;

	[SerializeField]
	private PurchaseButtonWithPoint _purchaseButtonWithPoint;

	[SerializeField]
	private Image _thumbnailImage;

	[SerializeField]
	private DecorationSkinTypeIconDBForMobile _iconDb;

	[SerializeField]
	private Animator _purchaseCompletedEffectAnimator;

	private readonly Subject<DecorationService.DecorationSkinType> _onPurchased = new Subject<DecorationService.DecorationSkinType>();

	private DecorationService.DecorationSkinType _decorationSkinType;

	public Observable<DecorationService.DecorationSkinType> OnPurchased => _onPurchased;

	public void Initialize()
	{
		Init();
		_purchaseButtonWithPoint.Initialize();
		_purchaseButtonWithPoint.OnClick.SubscribeAwait(async delegate
		{
			if (base.IsShowing && !base.IsAnimating)
			{
				_systemSeService.PlayBuyItem();
				await PurchaseAsync();
			}
		}, AwaitOperation.Drop).AddTo(this);
	}

	public void Open(DecorationService.DecorationSkinType decorationSkinType)
	{
		if (!_unlockItemService.Decoration.IsPurchasableType(decorationSkinType, out var price))
		{
			Debug.LogError($"購入不可: {decorationSkinType}");
			return;
		}
		Show();
		_decorationSkinType = decorationSkinType;
		_thumbnailImage.sprite = _iconDb.GetSkinTypeIcon(decorationSkinType);
		bool flag = _unlockItemService.Decoration.CanPurchase(decorationSkinType);
		_purchaseButtonWithPoint.Setup(price, flag);
		_purchaseButtonWithPoint.SetButtonInteractable(flag);
	}

	private async UniTask PurchaseAsync()
	{
		if (!_unlockItemService.Decoration.Purchase(_decorationSkinType))
		{
			Debug.LogError($"購入失敗: {_decorationSkinType}");
			return;
		}
		CancellationToken ct = base.ShowCancellationToken;
		try
		{
			await PlayPurchaseCompletedAnimation(ct);
			await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224), ignoreTimeScale: false, PlayerLoopTiming.Update, ct);
		}
		finally
		{
			_onPurchased.OnNext(_decorationSkinType);
		}
		Hide();
	}

	private async UniTask PlayPurchaseCompletedAnimation(CancellationToken cancellationToken)
	{
		GameObject effectObj = _purchaseCompletedEffectAnimator.gameObject;
		effectObj.SetActive(value: true);
		await UniTask.WaitUntil(() => _purchaseCompletedEffectAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f, PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
		effectObj.SetActive(value: false);
		cancellationToken.ThrowIfCancellationRequested();
	}
}
