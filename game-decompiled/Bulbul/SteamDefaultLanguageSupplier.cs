using NestopiSystem.Steam;
using Steamworks;
using VContainer;

namespace Bulbul;

public class SteamDefaultLanguageSupplier : IDefaultLanguageSupplier
{
	[Inject]
	private SteamManager steamManager;

	public GameLanguageType DefaultLanguage => GetDefaultLanguage();

	private GameLanguageType GetDefaultLanguage()
	{
		if (!steamManager.IsInitialized)
		{
			steamManager.Initialize();
		}
		return SteamUtils.GetSteamUILanguage() switch
		{
			"japanese" => GameLanguageType.Japanese, 
			"english" => GameLanguageType.English, 
			"schinese" => GameLanguageType.ChineseSimplified, 
			"tchinese" => GameLanguageType.ChineseTraditional, 
			"portuguese" => GameLanguageType.Portuguese, 
			"brazilian" => GameLanguageType.Portuguese, 
			"koreana" => GameLanguageType.Korean, 
			"russian" => GameLanguageType.Russian, 
			_ => GameLanguageType.English, 
		};
	}
}
