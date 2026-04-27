using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class EnvironmentPurchaseDialog : DialogBase
{
	[SerializeField]
	private PurchaseButtonWithPoint _purchaseButtonWithPoint;

	[SerializeField]
	private Image _thumbnailImage;

	[SerializeField]
	private EnvironmentIconDBBase _iconDb;

	[SerializeField]
	private Animator _purchaseCompletedEffectAnimator;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private UnlockItemService _unlockItemService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[Inject]
	private EnvironmentApplicationService _applicationService;

	private readonly Subject<EnvironmentType> _onPurchased = new Subject<EnvironmentType>();

	private EnvironmentType _environmentType;

	public Observable<EnvironmentType> OnPurchased => _onPurchased;

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

	public void Open(EnvironmentType environmentType)
	{
		if (!_unlockItemService.Environment.IsPurchasableType(environmentType, out var price))
		{
			Debug.LogError($"購入不可: {environmentType}");
			return;
		}
		Show();
		_environmentType = environmentType;
		_thumbnailImage.sprite = _iconDb.GetShopThumbnail(environmentType);
		bool flag = _unlockItemService.Environment.CanPurchase(environmentType);
		_purchaseButtonWithPoint.Setup(price, flag);
		_purchaseButtonWithPoint.SetButtonInteractable(flag);
	}

	private async UniTask PurchaseAsync()
	{
		if (!_unlockItemService.Environment.Purchase(_environmentType))
		{
			Debug.LogError($"購入失敗: {_environmentType}");
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
			_onPurchased.OnNext(_environmentType);
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
