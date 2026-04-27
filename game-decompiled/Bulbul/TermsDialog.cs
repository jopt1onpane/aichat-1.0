using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class TermsDialog : MonoBehaviour, IDisposable
{
	[SerializeField]
	private ButtonEventObservable termsButton;

	[SerializeField]
	private ButtonEventObservable privacyPolicyButton;

	[SerializeField]
	private ButtonEventObservable agreeButton;

	[SerializeField]
	private ButtonEventObservable closeButton;

	[SerializeField]
	private LayoutElement titleLayoutElement;

	[SerializeField]
	[Header("クローズボタンがない時にテキストを伸ばせる横幅")]
	private int titlePreferredWidthForHideCloseButton;

	[SerializeField]
	[Header("クローズボタンがある時にテキストを伸ばせる横幅")]
	private int titlePreferredWidthForShowCloseButton;

	[Inject]
	private LanguageSupplier language;

	[Inject]
	private SystemSeService systemSeService;

	private static TermsDialog prefab;

	public static TermsDialog Create(Transform parent, bool isClosable)
	{
		CommonPrefabSupplier commonPrefabSupplier = ProjectLifetimeScope.Resolve<CommonPrefabSupplier>();
		if (prefab == null)
		{
			if (DevicePlatform.Steam.IsMobile())
			{
				prefab = commonPrefabSupplier.Get("TermsDialog").GetComponent<TermsDialog>();
			}
			else if (DevicePlatform.Steam.IsPC())
			{
				prefab = commonPrefabSupplier.Get("TermsDialog_PC").GetComponent<TermsDialog>();
			}
		}
		TermsDialog termsDialog = ProjectLifetimeScope.ResolveInstantiate(prefab, parent);
		termsDialog.closeButton.gameObject.SetActive(isClosable);
		termsDialog.LayoutTitleWidth(isClosable);
		return termsDialog;
	}

	private void Start()
	{
		termsButton.OnClick.Subscribe(OpenTermsPage).AddTo(this);
		privacyPolicyButton.OnClick.Subscribe(OpenPrivacyPolicyPage).AddTo(this);
	}

	private async UniTask OnEnable()
	{
		agreeButton.View.interactable = false;
		await UniTask.WaitForSeconds(1, ignoreTimeScale: false, PlayerLoopTiming.Update, base.destroyCancellationToken);
		agreeButton.View.interactable = true;
	}

	public async UniTask<bool> SubmitWaitAsync(CancellationToken ct)
	{
		(int, Unit, Unit) obj = await UniTask.WhenAny(agreeButton.OnClick.ToUniTask(useFirstValue: true, ct), closeButton.OnClick.ToUniTask(useFirstValue: true, ct));
		if (obj.Item1 == 1)
		{
			systemSeService.PlayCancel();
		}
		else
		{
			systemSeService.PlayClick();
		}
		return obj.Item1 == 0;
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OpenTermsPage()
	{
		OpenURLFunctions.OpenTerms(language);
	}

	private void OpenPrivacyPolicyPage()
	{
		OpenURLFunctions.OpenPrivacyPolicy(language);
	}

	private void LayoutTitleWidth(bool isClosable)
	{
		if (!(titleLayoutElement == null))
		{
			titleLayoutElement.preferredWidth = (isClosable ? titlePreferredWidthForShowCloseButton : titlePreferredWidthForHideCloseButton);
		}
	}
}
