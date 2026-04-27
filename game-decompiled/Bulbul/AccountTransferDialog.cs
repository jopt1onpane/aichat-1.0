using System;
using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using FastEnumUtility;
using R3;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using VContainer;

namespace Bulbul;

public class AccountTransferDialog : MonoBehaviour, IUniTaskAsyncDisposable
{
	[Inject]
	private SystemSeService systemSeService;

	[SerializeField]
	private TextLocalizationBehaviour bodyText;

	[SerializeField]
	private UserStatusView currentUserStatusView;

	[SerializeField]
	private UserStatusView otherUserStatusView;

	[SerializeField]
	private ButtonEventObservable closeButton;

	[SerializeField]
	private FacilityAnimationBase dialogAnimation;

	private readonly Subject<Unit> onClosed = new Subject<Unit>();

	public Observable<Unit> OnClosed => onClosed;

	public Observable<int> OnSubmit => currentUserStatusView.SubmitButton.OnClick.Select((Func<Unit, int>)delegate
	{
		systemSeService.PlayClick();
		return 0;
	}).Merge(otherUserStatusView.SubmitButton.OnClick.Select((Func<Unit, int>)delegate
	{
		systemSeService.PlayClick();
		return 1;
	}));

	public bool IsClosed { get; private set; }

	private void Start()
	{
		closeButton.OnClick.Subscribe(delegate(Unit _)
		{
			systemSeService.PlayCancel();
			onClosed.OnNext(_);
		}).AddTo(this);
	}

	public async UniTask<AccountTransferDialog> Open(AccountType accountType, UserStatus currentUserStatus, UserStatus otherUserStatus, CancellationToken ct)
	{
		bodyText.Set("ui_account_transfer_body", (string s) => s.FormatSmart(accountType.ToName()));
		IsClosed = false;
		currentUserStatusView.Setup(currentUserStatus);
		otherUserStatusView.Setup(otherUserStatus);
		base.gameObject.SetActive(value: true);
		if ((bool)dialogAnimation)
		{
			dialogAnimation.Setup();
			await dialogAnimation.Activate();
		}
		return this;
	}

	public async UniTask Close(CancellationToken ct)
	{
		if (!IsClosed)
		{
			IsClosed = true;
			if ((bool)dialogAnimation)
			{
				await dialogAnimation.Deactivate();
			}
			base.gameObject.SetActive(value: false);
			onClosed.OnNext(Unit.Default);
		}
	}

	public async UniTask<int> SubmitOrCloseWaitAsync(CancellationToken ct)
	{
		if (!this || IsClosed)
		{
			return -1;
		}
		(int, int, Unit) tuple = await UniTask.WhenAny(OnSubmit.ToUniTask(useFirstValue: true, ct), OnClosed.ToUniTask(useFirstValue: true, ct));
		if (tuple.Item1 == 1)
		{
			return -1;
		}
		return tuple.Item2;
	}

	public async UniTask DisposeAsync()
	{
		await Close(base.destroyCancellationToken);
	}
}
