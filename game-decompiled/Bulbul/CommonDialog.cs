using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class CommonDialog : MonoBehaviour, IUniTaskAsyncDisposable
{
	private static CommonDialog prefab;

	[SerializeField]
	private CommonDialogButtonView buttonPrefab;

	[SerializeField]
	private GameObject titleObject;

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

	private CommonDialogOption option;

	private readonly List<CommonDialogButtonView> buttonViews = new List<CommonDialogButtonView>();

	public bool IsClosed { get; private set; }

	public Observable<int> OnSubmit => onSubmit;

	public Observable<Unit> OnClosed => onClosed;

	public static CommonDialog Create(Action<CommonDialogOption> optionFunc)
	{
		CommonPrefabSupplier commonPrefabSupplier = ProjectLifetimeScope.Resolve<CommonPrefabSupplier>();
		CommonDialogOption commonDialogOption = new CommonDialogOption();
		optionFunc(commonDialogOption);
		if (prefab == null)
		{
			if (DevicePlatform.Steam.IsMobile())
			{
				prefab = commonPrefabSupplier.Get("CommonDialog").GetComponent<CommonDialog>();
			}
			else if (DevicePlatform.Steam.IsPC())
			{
				prefab = commonPrefabSupplier.Get("CommonDialog_PC").GetComponent<CommonDialog>();
			}
		}
		CommonDialog commonDialog = ProjectLifetimeScope.ResolveInstantiate(prefab, commonDialogOption.Parent);
		commonDialog.Setup(commonDialogOption);
		return commonDialog;
	}

	public void Setup(CommonDialogOption option)
	{
		this.option = option;
		base.transform.SetAsLastSibling();
		titleObject.SetActive(option.TitleID != null);
		if (option.TitleID != null)
		{
			titleText.Set(option.TitleID, option.TitleSelector);
		}
		bodyText.Set(option.BodyID, option.BodySelector);
		closeButton.gameObject.SetActive(option.UseCloseButton);
		if ((bool)option.Parent)
		{
			base.transform.SetParent(option.Parent, worldPositionStays: false);
		}
		CommonButton[] buttons = option.Buttons;
		if (buttons == null || buttons.Length <= 0)
		{
			buttonParent.gameObject.SetActive(value: false);
			return;
		}
		buttonParent.gameObject.SetActive(value: true);
		CommonButton[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			CommonButton commonButton = array[i];
			CommonDialogButtonView commonDialogButtonView = ProjectLifetimeScope.ResolveInstantiate(buttonPrefab, buttonParent);
			commonDialogButtonView.Text.Set(commonButton.TextID);
			commonDialogButtonView.SetStyle(commonButton.Style);
			commonDialogButtonView.gameObject.SetActive(value: true);
			buttonViews.Add(commonDialogButtonView);
		}
		buttonViews.Select((CommonDialogButtonView view) => view.OnClick.Select(view, (Unit _, CommonDialogButtonView result) => result)).Merge().Subscribe(this, delegate(CommonDialogButtonView view, CommonDialog @this)
		{
			if (!@this.IsClosed)
			{
				int num = @this.buttonViews.IndexOf(view);
				if (@this.buttonViews.InBounded(num))
				{
					@this.PlaySE(num);
					@this.onSubmit.OnNext(num);
					if (@this.option.EnableCloseOnClickButton)
					{
						@this.Close(@this.destroyCancellationToken).Forget();
					}
				}
			}
		})
			.AddTo(this);
		if ((bool)dialogAnimation)
		{
			dialogAnimation.Setup();
			dialogAnimation.Activate();
		}
	}

	public async UniTask Close(CancellationToken ct)
	{
		if (IsClosed)
		{
			if (!onClosed.IsDisposed)
			{
				await onClosed.ToUniTask(useFirstValue: false, ct);
			}
			return;
		}
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
		onSubmit.OnCompleted();
		onClosed.OnCompleted();
		if ((bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
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
		if (option.Buttons != null && buttonIdx < option.Buttons.Length)
		{
			SystemSeType seType = option.Buttons[buttonIdx].SeType;
			systemSeService.Play(new SystemSeParam
			{
				SeSound = seType,
				IsAllowsDuplicate = true
			});
		}
	}

	public async UniTask DisposeAsync()
	{
		try
		{
			await Close(base.destroyCancellationToken);
		}
		finally
		{
			onSubmit?.Dispose();
			onClosed?.Dispose();
		}
	}
}
