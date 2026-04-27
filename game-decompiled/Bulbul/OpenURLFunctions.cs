using UnityEngine;

namespace Bulbul;

public static class OpenURLFunctions
{
	public static void OpenTerms(LanguageSupplier language)
	{
		Application.OpenURL((language.Get() == GameLanguageType.Japanese) ? "https://www.nestopi.co.jp/chillwithyou-terms-of-service/" : "https://www.nestopi.co.jp/terms-of-service/");
	}

	public static void OpenPrivacyPolicy(LanguageSupplier language)
	{
		Application.OpenURL((language.Get() == GameLanguageType.Japanese) ? "https://www.nestopi.co.jp/privacy/" : "https://www.nestopi.co.jp/en/privacy/");
	}

	public static void OpenFAQ(LanguageSupplier language)
	{
		if (language.Get() == GameLanguageType.Japanese)
		{
			Application.OpenURL("https://www.nestopi.co.jp/chill-with-you-faq/");
		}
		else
		{
			Application.OpenURL("https://www.nestopi.co.jp/en/chill-with-you-faq/");
		}
	}
}
