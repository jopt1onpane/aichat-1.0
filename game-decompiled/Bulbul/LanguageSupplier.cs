using System.Globalization;
using R3;

namespace Bulbul;

public class LanguageSupplier
{
	private readonly IDefaultLanguageSupplier defaultLanguageSupplier;

	private readonly ReactiveProperty<GameLanguageType> language = new ReactiveProperty<GameLanguageType>();

	public ReadOnlyReactiveProperty<GameLanguageType> Language => language;

	public LanguageSupplier(IDefaultLanguageSupplier defaultLanguageSupplier)
	{
		this.defaultLanguageSupplier = defaultLanguageSupplier;
	}

	public GameLanguageType Get()
	{
		ReactiveProperty<GameLanguageType> gameLanguage = SaveDataManager.Instance.SettingData.GameLanguage;
		gameLanguage.Value = ((gameLanguage.Value == GameLanguageType.None) ? defaultLanguageSupplier.DefaultLanguage : gameLanguage.Value);
		return language.Value = gameLanguage.Value;
	}

	public void Set(GameLanguageType value)
	{
		SaveDataManager.Instance.SettingData.GameLanguage.Value = value;
		language.Value = value;
		SaveDataManager.Instance.SaveSetting();
	}

	public CultureInfo GetCultureInfo()
	{
		return Get() switch
		{
			GameLanguageType.Japanese => CultureInfo.GetCultureInfo("ja-JP"), 
			GameLanguageType.ChineseSimplified => CultureInfo.GetCultureInfo("zh-CN"), 
			GameLanguageType.ChineseTraditional => CultureInfo.GetCultureInfo("zh-TW"), 
			GameLanguageType.Portuguese => CultureInfo.GetCultureInfo("pt-BR"), 
			GameLanguageType.Korean => CultureInfo.GetCultureInfo("ko-KR"), 
			GameLanguageType.Russian => CultureInfo.GetCultureInfo("ru-RU"), 
			_ => CultureInfo.GetCultureInfo("en-US"), 
		};
	}
}
