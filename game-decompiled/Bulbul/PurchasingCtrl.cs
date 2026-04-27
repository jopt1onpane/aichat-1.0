using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bulbul.Mobile;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using Firebase.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using VContainer;

namespace Bulbul;

public class PurchasingCtrl
{
	[Inject]
	private LanguageSupplier languageSupplier;

	private static PurchasingCtrl instance;

	public const string PRODUCT_ID_UPGRADE_PASS = "";

	public const string PRODUCT_ID_UPGRADE_PASS_DEFAULT = "";

	private IStoreService storeService;

	private IProductService productService;

	private IPurchaseService purchasingService;

	private ICatalogProvider catalogProvider;

	private CancellationTokenSource cts;

	private LoadingScreen loadingScreen;

	private IDisposable loading;

	private IPurchaseUIProvider uiProvider;

	public bool IsFetchingProduct { get; private set; }

	public bool IsLastFetchedProductSuccessed { get; private set; }

	public bool IsFetchingProductPurchse { get; private set; }

	public static PurchasingCtrl GetInstance()
	{
		return instance;
	}

	public PurchasingCtrl()
	{
		cts = new CancellationTokenSource();
		instance = this;
	}

	public async UniTask<bool> Initialize(CancellationToken ct)
	{
		return true;
	}

	private async Task<bool> InitializeUnityService()
	{
		bool result = false;
		try
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName("production"));
			}
			result = true;
		}
		catch (Exception)
		{
		}
		return result;
	}

	public void FetchProducts()
	{
		IsFetchingProduct = true;
		catalogProvider.FetchProducts(productService.FetchProductsWithNoRetries, DefaultStoreHelper.GetDefaultStoreName());
	}

	private async UniTask<bool> PurchaseAsync(Order order, CancellationToken ct)
	{
		string deviceID = SaveDataManager.Instance.AccountData.DeviceID;
		string receipt = order.Info.Receipt;
		WebApiResponse<PurchaseResponse> webApiResponse = await WebApi.PostAsync<Purchase, PurchaseResponse>(new Purchase(deviceID, receipt), ct);
		if (!webApiResponse.Response.IsGranted)
		{
			foreach (CartItem item in order.CartOrdered.Items())
			{
				Parameter[] parameters = new Parameter[5]
				{
					new Parameter(FirebaseAnalytics.ParameterProductID, item.Product.definition.id),
					new Parameter(FirebaseAnalytics.ParameterProductName, item.Product.metadata.localizedTitle),
					new Parameter(FirebaseAnalytics.ParameterPrice, item.Product.metadata.localizedPriceString),
					new Parameter(FirebaseAnalytics.ParameterCurrency, item.Product.metadata.isoCurrencyCode),
					new Parameter(FirebaseAnalytics.ParameterTransactionID, webApiResponse.Response.TransactionId)
				};
				FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, parameters);
			}
		}
		return !webApiResponse.HasError;
	}

	private async UniTask<bool> GetDeviceNonConsumableAsync(CancellationToken ct)
	{
		WebApiResponse<GetDeviceNonConsumableResponse> webApiResponse = await WebApi.GetAsync<GetDeviceNonConsumable, GetDeviceNonConsumableResponse>(new GetDeviceNonConsumable(SaveDataManager.Instance.AccountData.DeviceID), ct);
		string[] productIds = webApiResponse.Response.UnlockProducts.productIds;
		for (int i = 0; i < productIds.Length; i++)
		{
			_ = productIds[i];
		}
		InMemoryData.SetData(webApiResponse.Response.UnlockProducts);
		return !webApiResponse.HasError;
	}

	private void OnPurchaseConfirmed(Order order)
	{
		if (!(order is FailedOrder failedOrder))
		{
			if (order is ConfirmedOrder order2)
			{
				OnPurchaseConfirmedOrder(order2);
			}
		}
		else
		{
			OnConfirmationFailedOrder(failedOrder);
		}
	}

	private void OnConfirmationFailedOrder(FailedOrder failedOrder)
	{
		_ = failedOrder.FailureReason;
		foreach (CartItem item in failedOrder.CartOrdered.Items())
		{
			_ = item;
		}
	}

	private void OnPurchaseConfirmedOrder(ConfirmedOrder order)
	{
		foreach (CartItem item in order.CartOrdered.Items())
		{
			_ = item;
		}
	}

	private void OnPurchaseFailed(FailedOrder failedOrder)
	{
		HideLoadingScreen();
		_ = failedOrder.FailureReason;
		foreach (CartItem item in failedOrder.CartOrdered.Items())
		{
			_ = item;
		}
	}

	private void OnPurchaseDeferred(DeferredOrder deferredOrder)
	{
		HideLoadingScreen();
		foreach (CartItem item in deferredOrder.CartOrdered.Items())
		{
			_ = item;
		}
	}

	public async UniTask<(bool IsPossible, bool IsMaintenance)> ExecutePurchase(string productId)
	{
		Product product = FindProduct(productId);
		if (product != null)
		{
			CancellationToken ct = cts.Token;
			ShowLoadingScreen();
			GetCheckPurchaseResponse res = await CheckPurchaseAsync(productId, ct);
			if (res.ShopMaintenanceInfo.IsMaintenance)
			{
				HideLoadingScreen();
				await CommonDialog.Create(delegate(CommonDialogOption o)
				{
					o.TitleID = "ui_maintenance_title";
					o.BodyID = "ui_common_str_directory";
					o.BodySelector = (string _) => res.ShopMaintenanceInfo.MainText;
					o.Buttons = new CommonButton[1]
					{
						new CommonButton("ui_common_confirm")
					};
					o.EnableCloseOnClickButton = true;
					o.Parent = uiProvider.DialogParent;
				}).SubmitOrCloseWaitAsync(ct);
				return (false, true);
			}
			if (res.IsPossible)
			{
				purchasingService?.PurchaseProduct(product);
				return (true, false);
			}
			HideLoadingScreen();
			await CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_shop_purchase_error_title";
				o.BodyID = "ui_shop_purchase_error_already_purchased";
				o.Buttons = new CommonButton[1]
				{
					new CommonButton("ui_common_confirm")
				};
				o.EnableCloseOnClickButton = true;
				o.Parent = uiProvider.DialogParent;
			}).SubmitOrCloseWaitAsync(ct);
			return (false, false);
		}
		return (false, false);
	}

	private async UniTask<GetCheckPurchaseResponse> CheckPurchaseAsync(string productID, CancellationToken ct)
	{
		return (await WebApi.GetAsync<CheckPurchase, GetCheckPurchaseResponse>(new CheckPurchase(SaveDataManager.Instance.AccountData.DeviceID, productID, languageSupplier.Get()), ct)).Response;
	}

	public Product FindProduct(string productId)
	{
		return productService?.GetProducts()?.FirstOrDefault((Product product) => product.definition.id == productId);
	}

	public void RestorePurchase()
	{
		ShowLoadingScreen();
		IsFetchingProductPurchse = true;
		purchasingService.RestoreTransactions(OnTransactionsRestored);
	}

	private void OnTransactionsRestored(bool success, string error)
	{
		if (!success)
		{
			IsFetchingProductPurchse = false;
		}
		HideLoadingScreen();
	}

	private void ShowLoadingScreen()
	{
		loading?.Dispose();
		loading = null;
		loading = loadingScreen.CreateLoadingScope();
		uiProvider.SetActiveBlockRaycast(active: true);
	}

	private void HideLoadingScreen()
	{
		loading?.Dispose();
		loading = null;
		uiProvider.SetActiveBlockRaycast(active: false);
	}
}
