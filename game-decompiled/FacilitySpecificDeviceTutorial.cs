using Bulbul;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class FacilitySpecificDeviceTutorial : MonoBehaviour
{
	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	private RectTransform _commonDialogParent;

	public async UniTask OpenSpecificDeviceTutorial(bool isDeviceCheck = true)
	{
		if ((!isDeviceCheck || CheckUsingSamsungDevice()) && await BigOneButtonDialog.Create(delegate(BigOneButtonDialogOption option)
		{
			option.TitleID = null;
			option.BodyID = "ui_tutorial_samsung_setting_info";
			option.Parent = _commonDialogParent;
			option.EnableCloseOnClickButton = true;
			option.Button = new CommonButton("ui_setting_faq");
		}).SubmitOrCloseWaitAsync(base.destroyCancellationToken) == 0 && await CommonDialog.Create(delegate(CommonDialogOption option)
		{
			option.TitleID = null;
			option.BodyID = "ui_setting_confirm_jump_web";
			option.Parent = _commonDialogParent;
			option.EnableCloseOnClickButton = true;
			option.Buttons = new CommonButton[2]
			{
				new CommonButton("ui_common_confirm_yes", CommonButtonStyle.Submit),
				new CommonButton("ui_common_confirm_no", CommonButtonStyle.Normal, SystemSeType.Cancel)
			};
		}).SubmitOrCloseWaitAsync(base.destroyCancellationToken) == 0)
		{
			OpenURLFunctions.OpenFAQ(_languageSupplier);
		}
	}

	private bool CheckUsingSamsungDevice()
	{
		if (SystemInfo.deviceModel.Contains("samsung"))
		{
			return true;
		}
		return false;
	}
}
