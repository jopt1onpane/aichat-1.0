using System;
using Cysharp.Text;
using NestopiSystem;
using NestopiSystem.DIContainers;
using UnityEngine;
using UnityEngine.Networking;

namespace Bulbul.Mobile;

public static class SettingUtility
{
	private const string SUPPORT_MAIL_ADDRESS = "support@nestopi.co.jp";

	private const string SUBJECT_PREFIX_LOCALIZE_ID = "ui_setting_contact_us_subject_prefix";

	private const string SUBJECT_TITLE_LOCALIZE_ID = "ui_setting_contact_us_subject";

	private const string SUBJECT_PREFIX = "[Chill with You]";

	private const string CAUTION_TEXT_LOCALIZE_ID = "ui_setting_contact_us_body_caution";

	private const string INQUERY_CAUTION_TEXT_LOCALIZE_ID = "ui_setting_contact_warning";

	public static void OpenMailer()
	{
		LocalizationMasterWrapper localizationMasterWrapper = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
		string text = "support@nestopi.co.jp";
		string result = string.Empty;
		if (!localizationMasterWrapper.TryGet("ui_setting_contact_us_subject_prefix", out result))
		{
			result = "[Chill with You]";
		}
		string result2 = string.Empty;
		if (!localizationMasterWrapper.TryGet("ui_setting_contact_us_subject", out result2))
		{
			result2 = string.Empty;
		}
		string text2 = EscapeURL(result + " " + result2);
		string text3 = EscapeURL(GetBodyText());
		Application.OpenURL("mailto:" + text + "?subject=" + text2 + "&body=" + text3);
	}

	private static string EscapeURL(string text)
	{
		return UnityWebRequest.EscapeURL(text).Replace("+", "%20");
	}

	private static string GetBodyText()
	{
		LocalizationMasterWrapper localizationMasterWrapper = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
		string result = string.Empty;
		if (!localizationMasterWrapper.TryGet("ui_setting_contact_us_body_caution", out result))
		{
			result = string.Empty;
		}
		using Utf8ValueStringBuilder utf8ValueStringBuilder = ZString.CreateUtf8StringBuilder(notNested: true);
		localizationMasterWrapper.TryGet("ui_setting_contact_warning", out var result2);
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendLine(result);
		ReadOnlySpan<char> value = MemoryExtensions.AsSpan(SaveDataManager.Instance.AccountData.DeviceID).Take(10);
		utf8ValueStringBuilder.Append("InquiryID: ");
		utf8ValueStringBuilder.Append(value);
		utf8ValueStringBuilder.AppendLine();
		if (!result2.IsNullOrEmpty())
		{
			utf8ValueStringBuilder.AppendLine(result2);
		}
		utf8ValueStringBuilder.AppendFormat("OperatingSystem: {0}", SystemInfo.operatingSystem);
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendFormat("DeviceModel: {0}", Applicationx.GetDeviceName());
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendFormat("GraphicsDeviceName: {0}", SystemInfo.graphicsDeviceName);
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendFormat("GraphicsMemorySize: {0} MB", SystemInfo.graphicsMemorySize);
		utf8ValueStringBuilder.AppendLine();
		utf8ValueStringBuilder.AppendFormat("SystemMemorySize: {0} MB", SystemInfo.systemMemorySize);
		return utf8ValueStringBuilder.ToString();
	}
}
