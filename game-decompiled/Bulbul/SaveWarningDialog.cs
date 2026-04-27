using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SaveWarningDialog : MonoBehaviour, IUniTaskAsyncDisposable
{
	[SerializeField]
	private InteractableUI neverShowAgainUI;

	[SerializeField]
	private Button neverShowAgainToggle;

	[SerializeField]
	private FacilityAnimationBase dialogAnimation;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Button closeButton;

	[Inject]
	private SystemSeService systemSeService;

	private readonly Subject<bool> onCloseClick = new Subject<bool>();

	private readonly Subject<bool> onClosed = new Subject<bool>();

	private static SaveWarningDialog prefab;

	public bool IsClosed { get; private set; }

	public Observable<bool> OnCloseClick => onCloseClick;

	public Observable<bool> OnClosed => onClosed;

	public static SaveWarningDialog Create(Transform parent)
	{
		CommonPrefabSupplier commonPrefabSupplier = ProjectLifetimeScope.Resolve<CommonPrefabSupplier>();
		if (prefab == null)
		{
			if (DevicePlatform.Steam.IsMobile())
			{
				prefab = commonPrefabSupplier.Get("SaveWarningDialog").GetComponent<SaveWarningDialog>();
			}
			else if (DevicePlatform.Steam.IsPC())
			{
				prefab = commonPrefabSupplier.Get("SaveWarningDialog_PC").GetComponent<SaveWarningDialog>();
			}
		}
		SaveWarningDialog saveWarningDialog = ProjectLifetimeScope.ResolveInstantiate(prefab, parent);
		saveWarningDialog.neverShowAgainUI.Setup();
		saveWarningDialog.neverShowAgainUI.DeactivateAllUI();
		saveWarningDialog.neverShowAgainToggle.OnClickAsObservable().Subscribe(saveWarningDialog, delegate(Unit _, SaveWarningDialog dialog)
		{
			if (dialog.neverShowAgainUI.IsUsing)
			{
				dialog.neverShowAgainUI.DeactivateUseUI();
			}
			else
			{
				dialog.neverShowAgainUI.ActivateUseUI();
			}
			dialog.systemSeService.Play(new SystemSeParam
			{
				SeSound = SystemSeType.Select,
				IsAllowsDuplicate = true
			});
		}).AddTo(saveWarningDialog);
		saveWarningDialog.closeButton.OnClickAsObservable().Subscribe(saveWarningDialog, delegate(Unit _, SaveWarningDialog dialog)
		{
			dialog.onCloseClick.OnNext(dialog.neverShowAgainUI.IsUsing);
		}).AddTo(saveWarningDialog);
		return saveWarningDialog;
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
			systemSeService.PlayClick();
			if ((bool)canvasGroup)
			{
				canvasGroup.interactable = false;
			}
			onClosed.OnNext(neverShowAgainUI.IsUsing);
			onClosed.OnCompleted();
			if ((bool)base.gameObject)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public async UniTask<bool> CloseWaitAsync(CancellationToken ct)
	{
		if (IsClosed)
		{
			throw new InvalidOperationException("Already closed");
		}
		return await onClosed.ToUniTask(useFirstValue: true, ct);
	}

	public async UniTask DisposeAsync()
	{
		await Close(base.destroyCancellationToken);
		onClosed?.Dispose();
	}
}
