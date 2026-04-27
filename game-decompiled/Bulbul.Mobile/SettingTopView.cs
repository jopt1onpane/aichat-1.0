using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingTopView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private ClickOutsideDetector topViewOutsideDetector;

	[SerializeField]
	private FacilityAnimationBase facilityAnimation;

	[SerializeField]
	private Button generalSettingButton;

	[SerializeField]
	private Button graphicsSettingButton;

	[SerializeField]
	private Button audioSettingButton;

	[SerializeField]
	private Button creditButton;

	[SerializeField]
	private Button linkAccountButton;

	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private Button contactButton;

	[SerializeField]
	private Button newsButton;

	[SerializeField]
	private Button uiHelpButton;

	[SerializeField]
	private Button xOpenButton;

	[SerializeField]
	private Button instagramOpenButton;

	[SerializeField]
	private Button youtubeOpenButton;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	private Subject<Unit> onCloseTweenComplete = new Subject<Unit>();

	private Subject<Unit> onClickGeneralSettingButton = new Subject<Unit>();

	private Subject<Unit> onClickGraphicsSettingButton = new Subject<Unit>();

	private Subject<Unit> onClickAudioSettingButton = new Subject<Unit>();

	private Subject<Unit> onClickCreditButton = new Subject<Unit>();

	private Subject<Unit> onClickLinkAccountButton = new Subject<Unit>();

	private Subject<Unit> onClickShopButton = new Subject<Unit>();

	private Subject<Unit> onClickContactButton = new Subject<Unit>();

	private Subject<Unit> onClickNewsButton = new Subject<Unit>();

	private Subject<Unit> onClickUIHelpButton = new Subject<Unit>();

	private readonly Subject<OfficialSNS> onClickSNSOpen = new Subject<OfficialSNS>();

	public Observable<Unit> OnClickClose => onClickClose;

	public Observable<Unit> OnCloseTweenComplete => onCloseTweenComplete;

	public Observable<Unit> OnClickGeneralSettingButton => onClickGeneralSettingButton;

	public Observable<Unit> OnClickGraphicsSettingButton => onClickGraphicsSettingButton;

	public Observable<Unit> OnClickAudioSettingButton => onClickAudioSettingButton;

	public Observable<Unit> OnClickCreditButton => onClickCreditButton;

	public Observable<Unit> OnClickLinkAccountButton => onClickLinkAccountButton;

	public Observable<Unit> OnClickShopButton => onClickShopButton;

	public Observable<Unit> OnClickContactButton => onClickContactButton;

	public Observable<Unit> OnClickNewsButton => onClickNewsButton;

	public Observable<Unit> OnClickUIHelpButton => onClickUIHelpButton;

	public Observable<OfficialSNS> OnClickSNSOpen => onClickSNSOpen;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
		onCloseTweenComplete?.Dispose();
		onClickGeneralSettingButton?.Dispose();
		onClickGraphicsSettingButton?.Dispose();
		onClickAudioSettingButton?.Dispose();
		onClickCreditButton?.Dispose();
		onClickLinkAccountButton?.Dispose();
		onClickShopButton?.Dispose();
		onClickContactButton?.Dispose();
		onClickNewsButton?.Dispose();
		onClickUIHelpButton?.Dispose();
		onClickSNSOpen?.Dispose();
	}

	public void Activate()
	{
		if (facilityAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			facilityAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (facilityAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			facilityAnimation.Deactivate().Forget();
		}
	}

	public void Setup()
	{
		facilityAnimation.Setup();
		ObservableSubscribeExtensions.Subscribe(closeButton.OnClickAsObservable(), delegate
		{
			onClickClose.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(topViewOutsideDetector.OnClickOutside, delegate
		{
			onClickClose.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(generalSettingButton.OnClickAsObservable(), delegate
		{
			onClickGeneralSettingButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(graphicsSettingButton.OnClickAsObservable(), delegate
		{
			onClickGraphicsSettingButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(audioSettingButton.OnClickAsObservable(), delegate
		{
			onClickAudioSettingButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(creditButton.OnClickAsObservable(), delegate
		{
			onClickCreditButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(linkAccountButton.OnClickAsObservable(), delegate
		{
			onClickLinkAccountButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(shopButton.OnClickAsObservable(), delegate
		{
			onClickShopButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(contactButton.OnClickAsObservable(), delegate
		{
			onClickContactButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(newsButton.OnClickAsObservable(), delegate
		{
			onClickNewsButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(facilityAnimation.OnCompleteDeactivate, delegate
		{
			onCloseTweenComplete.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(uiHelpButton.OnClickAsObservable(), delegate
		{
			topViewOutsideDetector.enabled = false;
			onClickUIHelpButton.OnNext(Unit.Default);
		}).AddTo(this);
		Observable.Merge<OfficialSNS>(from _ in xOpenButton.OnClickAsObservable()
			select OfficialSNS.X, from _ in instagramOpenButton.OnClickAsObservable()
			select OfficialSNS.Instagram, from _ in youtubeOpenButton.OnClickAsObservable()
			select OfficialSNS.YouTube).Subscribe(onClickSNSOpen).AddTo(this);
	}

	public void UIHelpClosed()
	{
		topViewOutsideDetector.enabled = true;
	}
}
