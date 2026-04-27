using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class BigOneButtonDialog : MonoBehaviour, IUniTaskAsyncDisposable
{
	private static BigOneButtonDialog prefab;

	[SerializeField]
	private CommonDialogButtonView buttonPrefab;

	[SerializeField]
	private Image headerLine;

	[SerializeField]
	private TextLocalizationBehaviour titleText;

	[SerializeField]
	private TextLocalizationBehaviour bodyText;

	[SerializeField]
	private ButtonEventObservable closeButton;

	[SerializeField]
	private Transform buttonParent;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private FacilityAnimationBase dialogAnimation;

	[Inject]
	private SystemSeService systemSeService;

	private readonly Subject<int> onSubmit = new Subject<int>();

	private readonly Subject<Unit> onClosed = new Subject<Unit>();

	private BigOneButtonDialogOption option;

	private readonly List<CommonDialogButtonView> buttonViews = new List<CommonDialogButtonView>();

	public bool IsClosed { get; private set; }

	public Observable<int> OnSubmit => onSubmit;

	public Observable<Unit> OnClosed => onClosed;

	public static BigOneButtonDialog Create(Action<BigOneButtonDialogOption> optionFunc)
	{
		CommonPrefabSupplier commonPrefabSupplier = ProjectLifetimeScope.Resolve<CommonPrefabSupplier>();
		BigOneButtonDialogOption bigOneButtonDialogOption = new BigOneButtonDialogOption();
		optionFunc(bigOneButtonDialogOption);
		if (prefab == null)
		{
			if (DevicePlatform.Steam.IsMobile())
			{
				prefab = commonPrefabSupplier.Get("BigOneButtonDialog").GetComponent<BigOneButtonDialog>();
			}
			else if (DevicePlatform.Steam.IsPC())
			{
				prefab = commonPrefabSupplier.Get("BigOneButtonDialog").GetComponent<BigOneButtonDialog>();
			}
		}
		BigOneButtonDialog bigOneButtonDialog = ProjectLifetimeScope.ResolveInstantiate(prefab, bigOneButtonDialogOption.Parent);
		bigOneButtonDialog.Setup(bigOneButtonDialogOption);
		return bigOneButtonDialog;
	}

	public void Setup(BigOneButtonDialogOption option)
	{
		this.option = option;
		closeButton.OnClick.Subscribe(this, delegate(Unit value, BigOneButtonDialog @this)
		{
			@this.systemSeService.PlayCancel();
			@this.Close(@this.destroyCancellationToken).Forget();
		}).AddTo(this);
		base.transform.SetAsLastSibling();
		if (option.TitleID == null)
		{
			titleText.Text.enabled = false;
			headerLine.enabled = false;
		}
		if (option.TitleID != null)
		{
			titleText.Set(option.TitleID, option.TitleSelector);
		}
		bodyText.Set(option.BodyID, option.BodySelector);
		if ((bool)option.Parent)
		{
			base.transform.SetParent(option.Parent, worldPositionStays: false);
		}
		buttonParent.gameObject.SetActive(value: true);
		CommonButton button = option.Button;
		CommonDialogButtonView commonDialogButtonView = ProjectLifetimeScope.ResolveInstantiate(buttonPrefab, buttonParent);
		commonDialogButtonView.Text.Set(button.TextID);
		commonDialogButtonView.SetStyle(button.Style);
		commonDialogButtonView.gameObject.SetActive(value: true);
		buttonViews.Add(commonDialogButtonView);
		commonDialogButtonView.OnClick.Subscribe((this, commonDialogButtonView), delegate(Unit state, (BigOneButtonDialog @this, CommonDialogButtonView view) value)
		{
			CommonDialogButtonView item = value.view;
			var (bigOneButtonDialog, _) = value;
			if (!bigOneButtonDialog.IsClosed)
			{
				int num = bigOneButtonDialog.buttonViews.IndexOf(item);
				if (bigOneButtonDialog.buttonViews.InBounded(num))
				{
					bigOneButtonDialog.PlaySE(num);
					bigOneButtonDialog.onSubmit.OnNext(num);
					if (bigOneButtonDialog.option.EnableCloseOnClickButton)
					{
						bigOneButtonDialog.Close(bigOneButtonDialog.destroyCancellationToken).Forget();
					}
				}
			}
		}).AddTo(this);
		if ((bool)dialogAnimation)
		{
			dialogAnimation.Setup();
			dialogAnimation.Activate();
		}
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
			if ((bool)canvasGroup)
			{
				canvasGroup.interactable = false;
			}
			onClosed.OnNext(Unit.Default);
			onClosed.OnCompleted();
			onSubmit.OnCompleted();
			if ((bool)base.gameObject)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
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

	private void PlaySE(int buttonIdx)
	{
		SystemSeType seType = option.Button.SeType;
		systemSeService.Play(new SystemSeParam
		{
			SeSound = seType,
			IsAllowsDuplicate = true
		});
	}

	public async UniTask DisposeAsync()
	{
		await Close(base.destroyCancellationToken);
		onSubmit?.Dispose();
		onClosed?.Dispose();
	}
}
