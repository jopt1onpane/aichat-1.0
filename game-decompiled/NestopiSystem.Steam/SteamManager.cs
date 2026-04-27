using System;
using System.Text;
using AOT;
using Bulbul;
using Steamworks;
using UnityEngine;
using VContainer.Unity;

namespace NestopiSystem.Steam;

public class SteamManager : IDisposable, ITickable
{
	private static class Error
	{
		public static void ThrowIfFailedPacksizeTest()
		{
			if (!Packsize.Test())
			{
				throw new PacksizeTestFailedException("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			}
		}

		public static void ThrowIfFailedDllCheckTest()
		{
			if (!Packsize.Test())
			{
				throw new DllCheckTestFailedException("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			}
		}

		public static void ThrowIfNecessaryRestartAppFromSteamAPI(bool isNecessaryRestartApp)
		{
			if (isNecessaryRestartApp)
			{
				throw new RestartAppIfNecessaryFromAPIException("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
			}
		}

		public static void ThrowIfSteamAPIInitializeFailed(SteamManager self)
		{
			if (!self.isInitialized)
			{
				throw new SteamAPIFailedInitializeException("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			}
		}
	}

	private bool isInitialized;

	protected SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;

	public bool IsInitialized => isInitialized;

	public SteamManager()
	{
		Initialize();
	}

	[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public void Initialize()
	{
		if (!isInitialized)
		{
			Error.ThrowIfFailedPacksizeTest();
			Error.ThrowIfFailedDllCheckTest();
			try
			{
				Error.ThrowIfNecessaryRestartAppFromSteamAPI(SteamAPI.RestartAppIfNecessary(AppId_t.Invalid));
				Error.ThrowIfNecessaryRestartAppFromSteamAPI(SteamAPI.RestartAppIfNecessary(new AppId_t(BulbulConstant.SteamID)));
			}
			catch (DllNotFoundException ex)
			{
				Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
				Application.Quit();
				throw;
			}
			catch (RestartAppIfNecessaryFromAPIException)
			{
				Application.Quit();
				throw;
			}
			isInitialized = SteamAPI.Init();
			Error.ThrowIfSteamAPIInitializeFailed(this);
			steamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
		}
	}

	void ITickable.Tick()
	{
		if (isInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public bool IsInstalledDLC(BulbulSteamDLC dlc)
	{
		if (!isInitialized)
		{
			return false;
		}
		return SteamApps.BIsDlcInstalled(new AppId_t((uint)dlc));
	}

	public void Dispose()
	{
		if (isInitialized)
		{
			SteamAPI.Shutdown();
		}
	}
}
