using System;
using Bulbul.MasterData;
using TMPro;

namespace Bulbul;

public class LocalizationMasterWrapper
{
	private readonly MasterDataLoader masterDataLoader;

	private readonly LanguageSupplier languageSupplier;

	public LocalizationMasterWrapper(MasterDataLoader masterDataLoader, LanguageSupplier languageSupplier)
	{
		this.masterDataLoader = masterDataLoader;
		this.languageSupplier = languageSupplier;
	}

	public bool TryGet(string localizeID, out string result)
	{
		return TryGet(languageSupplier.Get(), localizeID, out result);
	}

	public bool TryGet(GameLanguageType language, string localizeID, out string result)
	{
		result = "";
		if (!masterDataLoader.LocalizationList.TryGetValue(localizeID, out var value))
		{
			return false;
		}
		result = Get(value, language);
		return true;
	}

	public string Get(LocalizationData data)
	{
		return Get(data, languageSupplier.Get());
	}

	public string Get(LocalizationData data, GameLanguageType language)
	{
		return language switch
		{
			GameLanguageType.Japanese => data.Ja, 
			GameLanguageType.English => data.En, 
			GameLanguageType.ChineseSimplified => data.ZhHans, 
			GameLanguageType.ChineseTraditional => data.ZhHant, 
			GameLanguageType.Portuguese => data.Pt, 
			GameLanguageType.Korean => data.Ko, 
			GameLanguageType.Russian => data.Ru, 
			_ => data.Ja, 
		};
	}

	public bool Bind(TMP_Text text, string localizeID, Func<string, string> bindBefore = null)
	{
		return Bind(text, localizeID, languageSupplier.Get(), bindBefore);
	}

	public bool Bind(TMP_Text text, string localizeID, GameLanguageType language, Func<string, string> bindBefore = null)
	{
		if (!masterDataLoader.LocalizationList.TryGetValue(localizeID, out var value))
		{
			return false;
		}
		string text2 = Get(value, language);
		if (bindBefore != null)
		{
			text2 = bindBefore(text2);
		}
		text.SetText(text2);
		return true;
	}

	public bool Bind(TMP_InputField text, string localizeID, Func<string, string> bindBefore = null)
	{
		return Bind(text, localizeID, languageSupplier.Get(), bindBefore);
	}

	public bool Bind(TMP_InputField text, string localizeID, GameLanguageType language, Func<string, string> bindBefore = null)
	{
		if (!masterDataLoader.LocalizationList.TryGetValue(localizeID, out var value))
		{
			return false;
		}
		string text2 = Get(value, language);
		if (bindBefore != null)
		{
			text2 = bindBefore(text2);
		}
		text.text = text2;
		return true;
	}
}
