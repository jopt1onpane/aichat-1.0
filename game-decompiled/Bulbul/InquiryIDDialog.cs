using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class InquiryIDDialog : MonoBehaviour
{
	[Inject]
	private SystemSeService systemSeService;

	[Inject]
	private SmallAnnounceService smallAnnounceService;

	[SerializeField]
	private InquiryIDView inquiryIDView;

	[SerializeField]
	private ButtonEventObservable closeButton;

	private static InquiryIDDialog prefab;

	[SerializeField]
	private FacilityAnimationBase dialogAnimation;

	public static InquiryIDDialog Create(Transform parent)
	{
		CommonPrefabSupplier commonPrefabSupplier = ProjectLifetimeScope.Resolve<CommonPrefabSupplier>();
		if (prefab == null)
		{
			prefab = commonPrefabSupplier.Get("InquiryIDDialog").GetComponent<InquiryIDDialog>();
		}
		InquiryIDDialog instance = RoomLifetimeScope.ResolveInstantiate(prefab, parent);
		instance.inquiryIDView.Setup(SaveDataManager.Instance.AccountData.DeviceID);
		instance.closeButton.OnClick.SubscribeAwait(async delegate(Unit _, CancellationToken ct)
		{
			instance.systemSeService.PlayCancel();
			await instance.Close(ct);
		}).AddTo(instance);
		if ((bool)instance.dialogAnimation)
		{
			instance.dialogAnimation.Setup();
			instance.dialogAnimation.Activate();
		}
		return instance;
	}

	public async UniTask Close(CancellationToken ct)
	{
		smallAnnounceService.Deactivate();
		if ((bool)dialogAnimation)
		{
			await dialogAnimation.Deactivate();
		}
		Object.Destroy(base.gameObject);
	}
}
