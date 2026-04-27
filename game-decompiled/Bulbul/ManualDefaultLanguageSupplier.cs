namespace Bulbul;

public class ManualDefaultLanguageSupplier : IDefaultLanguageSupplier
{
	public GameLanguageType DefaultLanguage;

	GameLanguageType IDefaultLanguageSupplier.DefaultLanguage => DefaultLanguage;

	public ManualDefaultLanguageSupplier(GameLanguageType defaultLanguage)
	{
		DefaultLanguage = defaultLanguage;
	}
}
