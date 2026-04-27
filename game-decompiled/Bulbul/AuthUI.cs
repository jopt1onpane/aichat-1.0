using System.Threading;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class AuthUI : MonoBehaviour
{
	[SerializeField]
	private AuthButton googleButton;

	[SerializeField]
	private AuthButton appleButton;

	[SerializeField]
	private AuthButton guestButton;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[Inject]
	private AppAuth appAuth;

	[Inject]
	private IUICanvasProvider uiCanvasProvider;

	[Inject]
	private SystemSeService systemSeService;

	private readonly ReactiveProperty<bool> gate = new ReactiveProperty<bool>(value: true);

	public bool Interactable
	{
		get
		{
			return canvasGroup.interactable;
		}
		set
		{
			canvasGroup.interactable = value;
		}
	}

	private void Start()
	{
		googleButton.SetState(isLinked: false);
		appleButton.SetState(isLinked: false);
		guestButton.SetState(isLinked: false);
		gate.Subscribe(this, delegate(bool enable, AuthUI @this)
		{
			@this.googleButton.LinkButton.View.interactable = enable;
			@this.appleButton.LinkButton.View.interactable = enable;
			@this.guestButton.LinkButton.View.interactable = enable;
		}).AddTo(this);
		googleButton.LinkButton.OnClick.SubscribeAwait(this, async delegate(Unit _, AuthUI @this, CancellationToken ct)
		{
			@this.systemSeService.PlayClick();
			ct.ThrowIfCancellationRequested();
			await @this.appAuth.Signin(AccountType.Google, ct, @this.uiCanvasProvider);
		}, gate).AddTo(this);
		appleButton.LinkButton.OnClick.SubscribeAwait(this, async delegate(Unit _, AuthUI @this, CancellationToken ct)
		{
			@this.systemSeService.PlayClick();
			ct.ThrowIfCancellationRequested();
			await @this.appAuth.Signin(AccountType.Apple, ct, @this.uiCanvasProvider);
		}, gate).AddTo(this);
		guestButton.LinkButton.OnClick.SubscribeAwait(this, async delegate(Unit _, AuthUI @this, CancellationToken ct)
		{
			@this.systemSeService.PlayClick();
			ct.ThrowIfCancellationRequested();
			if (await CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_confirm";
				o.BodyID = "ui_login_guest_confirm";
				o.EnableCloseOnClickButton = true;
				o.Buttons = new CommonButton[2]
				{
					new CommonButton("ui_login_guest_submit"),
					new CommonButton("ui_common_confirm_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
				};
				o.Parent = @this.uiCanvasProvider.CommonDialogParent;
			}).SubmitOrCloseWaitAsync(ct) == 0)
			{
				@this.appAuth.GuestLogin();
			}
		}, gate).AddTo(this);
	}

	private void OnDestroy()
	{
		gate.Dispose();
	}
}
