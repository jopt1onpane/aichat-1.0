using System.IO;
using Steamworks;

namespace Bulbul;

public static class BulbulConstant
{
	public static readonly uint SteamID = 3548580u;

	public const string SaveDataFormatVersion = "v2";

	public const bool IsDemo = false;

	public const string SaveDataRootDirectory = "SaveData";

	public const string StoreURL = "https://store.steampowered.com/app/3548580";

	public static string SaveDirectoryPath => GetSaveDirectoryPath();

	private static string GetSaveDirectoryPath()
	{
		return CreateSaveDirectoryPath(isDemo: false, "v2");
	}

	public static string CreateSaveDirectoryPath(bool isDemo, string version)
	{
		string text = "";
		text = SteamUser.GetSteamID().ToString();
		return Path.Combine("SaveData", isDemo ? "Demo" : "Release", version, text);
	}

	public static string CreateSaveDirectoryPath(string versionDirectory)
	{
		string text = "";
		text = SteamUser.GetSteamID().ToString();
		return Path.Combine("SaveData", versionDirectory, text);
	}
}
