using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityShopWindow : MonoBehaviour
{
	[Inject]
	private LoadingScreen _loadingScreen;

	[Inject]
	private SystemSeService _systemSeService;

	[SerializeField]
	private ShopWindowView _shopWindowView;

	[SerializeField]
	private RectTransform _dialogParent;

	private Subject<Unit> _onClickCloseButton = new Subject<Unit>();

	private bool _isPurchasing;

	private float _timer;

	private TimeoutController _timeoutController = new TimeoutController();

	public Observable<Unit> OnClickCloseButton => _onClickCloseButton;

	public void Setup()
	{
	}

	public async UniTask<bool> EntryAsync()
	{
		return true;
	}

	private void OpenFailedFetchedProductDialog()
	{
		CommonDialog.Create(delegate(CommonDialogOption op)
		{
			op.Parent = _dialogParent;
			op.EnableCloseOnClickButton = true;
			op.UseCloseButton = false;
			op.TitleID = "ui_common_error";
			op.BodyID = "ui_shop_fetch_product_err_body";
			op.Buttons = new CommonButton[1]
			{
				new CommonButton("ui_common_confirm")
			};
		}).SubmitOrCloseWaitAsync(this.GetCancellationTokenOnDestroy()).Forget();
	}

	public async UniTask ActivateAsync()
	{
		await _shopWindowView.Activate();
	}

	public async UniTask DeactivateAsync()
	{
		await _shopWindowView.Deactivate();
	}
}
