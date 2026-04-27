using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul.Mobile;

public class SettingAccountView : MonoBehaviour
{
	[Inject]
	private SystemSeService systemSeService;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[SerializeField]
	private AuthButton appleButton;

	[SerializeField]
	private AuthButton googleButton;

	[SerializeField]
	private ButtonEventObservable deleteButton;

	[SerializeField]
	private Button termsButton;

	[SerializeField]
	private Button privacyPolicyButton;

	private readonly Subject<AccountType> onLink = new Subject<AccountType>();

	private readonly Subject<AccountType> onUnlink = new Subject<AccountType>();

	private readonly Subject<Unit> onDelete = new Subject<Unit>();

	[field: SerializeField]
	public AccountTransferDialog AccountTransferDialog { get; private set; }

	public Observable<Unit> OnClickClose => closeButton.OnClickAsObservable();

	public Observable<AccountType> OnLink => onLink;

	public Observable<AccountType> OnUnlink => onUnlink;

	public Observable<Unit> OnDelete => onDelete;

	public Observable<Unit> OnClickTerms => termsButton.OnClickAsObservable();

	public Observable<Unit> OnClickPrivacyPolicy => privacyPolicyButton.OnClickAsObservable();

	public void Activate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			viewAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			viewAnimation.Deactivate().Forget();
		}
	}

	public void Setup(ReactiveProperty<bool> gate)
	{
		viewAnimation.Setup();
		UpdateState();
		gate.Subscribe(this, delegate(bool enable, SettingAccountView @this)
		{
			@this.googleButton.LinkButton.View.interactable = enable;
			@this.googleButton.UnlinkButton.View.interactable = enable;
			@this.appleButton.LinkButton.View.interactable = enable;
			@this.appleButton.UnlinkButton.View.interactable = enable;
			@this.deleteButton.View.interactable = enable;
			@this.UpdateState();
		}).AddTo(this);
		googleButton.LinkButton.OnClick.Select((Unit _) => AccountType.Google).Merge(appleButton.LinkButton.OnClick.Select((Unit _) => AccountType.Apple)).Subscribe(delegate(AccountType type)
		{
			systemSeService.PlayClick();
			onLink.OnNext(type);
		})
			.AddTo(this);
		googleButton.UnlinkButton.OnClick.Select((Unit _) => AccountType.Google).Merge(appleButton.UnlinkButton.OnClick.Select((Unit _) => AccountType.Apple)).Subscribe(delegate(AccountType type)
		{
			systemSeService.PlayClick();
			onUnlink.OnNext(type);
		})
			.AddTo(this);
		deleteButton.OnClick.Subscribe(onDelete).AddTo(this);
	}

	public void UpdateState()
	{
		InMemoryData.TryGetData<List<AccountType>>(out var data);
		if (data == null || data.Count == 0)
		{
			appleButton.SetState(isLinked: false);
			googleButton.SetState(isLinked: false);
		}
		else
		{
			appleButton.SetState(data.Contains(AccountType.Apple));
			googleButton.SetState(data.Contains(AccountType.Google));
		}
	}
}
