using UnityEngine;

namespace Bulbul;

public class SystemDefaultLanguageSupplier : IDefaultLanguageSupplier
{
	public GameLanguageType DefaultLanguage => Application.systemLanguage switch
	{
		SystemLanguage.Chinese => GameLanguageType.ChineseSimplified, 
		SystemLanguage.English => GameLanguageType.English, 
		SystemLanguage.Japanese => GameLanguageType.Japanese, 
		SystemLanguage.ChineseSimplified => GameLanguageType.ChineseSimplified, 
		SystemLanguage.ChineseTraditional => GameLanguageType.ChineseTraditional, 
		SystemLanguage.Portuguese => GameLanguageType.Portuguese, 
		SystemLanguage.Korean => GameLanguageType.Korean, 
		SystemLanguage.Russian => GameLanguageType.Russian, 
		SystemLanguage.Unknown => GameLanguageType.English, 
		_ => GameLanguageType.English, 
	};
}
