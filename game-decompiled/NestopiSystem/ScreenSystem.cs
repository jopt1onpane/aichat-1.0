using System;
using System.Runtime.InteropServices;
using AOT;
using Bulbul;
using NestopiSystem.DIContainers;
using R3;
using UnityEngine;

namespace NestopiSystem;

public static class ScreenSystem
{
	private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	private const int HWND_TOPMOST = -1;

	private const int HWND_NOTOPMOST = -2;

	private const int SWP_NOSIZE = 1;

	private const int SWP_NOMOVE = 2;

	private const int GWL_EXSTYLE = -20;

	private const int WS_EX_TOPMOST = 8;

	private const int WM_SYSCOMMAND = 274;

	private const int TOP_WINDOW_COMMAND_ID = 8191;

	private const uint MF_STRING = 0u;

	private const uint MF_BYPOSITION = 1024u;

	private const int GWLP_WNDPROC = -4;

	private static IntPtr oldWndProc;

	private static WndProcDelegate newWndProcDelegate;

	private static GCHandle wndProcHandle;

	private static readonly ReactiveProperty<bool> isWindowTpAlways;

	private static IDisposable languageSubscription;

	private static bool isInitialized;

	public static ReadOnlyReactiveProperty<bool> IsWindowTpAlways => isWindowTpAlways;

	private static IntPtr GetActiveWindow()
	{
		return FindWindow(null, Application.productName);
	}

	[DllImport("user32.dll")]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll")]
	private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern bool InsertMenuW(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, WndProcDelegate dwNewLong);

	[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
	public static extern IntPtr SetWindowLongPtrRaw(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll")]
	private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern bool ModifyMenuW(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

	private static void UpdateSystemMenuText()
	{
		if (oldWndProc == IntPtr.Zero)
		{
			return;
		}
		IntPtr activeWindow = GetActiveWindow();
		if (!(activeWindow == IntPtr.Zero))
		{
			IntPtr systemMenu = GetSystemMenu(activeWindow, bRevert: false);
			if (!(systemMenu == IntPtr.Zero))
			{
				string localizedMenuText = GetLocalizedMenuText(isWindowTpAlways.Value);
				ModifyMenuW(systemMenu, 0u, 1024u, 8191u, localizedMenuText);
			}
		}
	}

	private static string GetLocalizedMenuText(bool isEnabled)
	{
		try
		{
			LocalizationMasterWrapper localizationMasterWrapper = ProjectLifetimeScope.Resolve<LocalizationMasterWrapper>();
			GameLanguageType language = ProjectLifetimeScope.Resolve<LanguageSupplier>().Get();
			if (!localizationMasterWrapper.TryGet(language, "ui_setting_general_alwaysontop_title", out var result))
			{
				result = "Always on Top";
			}
			string result2;
			if (isEnabled)
			{
				if (!localizationMasterWrapper.TryGet(language, "ui_global_off", out result2))
				{
					result2 = "Off";
				}
			}
			else if (!localizationMasterWrapper.TryGet(language, "ui_global_on", out result2))
			{
				result2 = "On";
			}
			return result + "(" + result2 + ")";
		}
		catch
		{
			return isEnabled ? "Always on Top(Off)" : "Always on Top(On)";
		}
	}

	public static void BringWindowToTopAlways()
	{
		IntPtr activeWindow = GetActiveWindow();
		int windowLong = GetWindowLong(activeWindow, -20);
		windowLong |= 8;
		SetWindowPos(activeWindow, -1, 0, 0, 0, 0, 3);
		SetWindowLong(activeWindow, -20, windowLong);
		IntPtr systemMenu = GetSystemMenu(activeWindow, bRevert: false);
		string localizedMenuText = GetLocalizedMenuText(isEnabled: true);
		ModifyMenuW(systemMenu, 0u, 1024u, 8191u, localizedMenuText);
		isWindowTpAlways.Value = true;
		SaveDataManager.Instance.SaveSetting();
	}

	public static void ResetWindowOrder()
	{
		IntPtr activeWindow = GetActiveWindow();
		int windowLong = GetWindowLong(activeWindow, -20);
		windowLong |= 8;
		SetWindowPos(activeWindow, -2, 0, 0, 0, 0, 3);
		SetWindowLong(activeWindow, -20, windowLong);
		IntPtr systemMenu = GetSystemMenu(activeWindow, bRevert: false);
		string localizedMenuText = GetLocalizedMenuText(isEnabled: false);
		ModifyMenuW(systemMenu, 0u, 1024u, 8191u, localizedMenuText);
		isWindowTpAlways.Value = false;
		SaveDataManager.Instance.SaveSetting();
	}

	public static void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		SetSystemMenuBringWindowToTopAlways();
		HookSystemMenuWindowToTopAlways();
		ObservableSubscribeExtensions.Subscribe(ProjectLifetimeScope.GameObject.transform.OnApplicationQuitAsObservable(), delegate
		{
			IntPtr activeWindow = GetActiveWindow();
			if (activeWindow != IntPtr.Zero && oldWndProc != IntPtr.Zero)
			{
				SetWindowLongPtrRaw(activeWindow, -4, oldWndProc);
				wndProcHandle.Free();
				oldWndProc = IntPtr.Zero;
			}
			languageSubscription?.Dispose();
			languageSubscription = null;
		});
		try
		{
			languageSubscription = ObservableSubscribeExtensions.Subscribe(ProjectLifetimeScope.Resolve<LanguageSupplier>().Language.Skip(1), delegate
			{
				UpdateSystemMenuText();
			});
		}
		catch
		{
		}
	}

	private static void SetSystemMenuBringWindowToTopAlways()
	{
		IntPtr systemMenu = GetSystemMenu(GetActiveWindow(), bRevert: false);
		string localizedMenuText = GetLocalizedMenuText(isEnabled: false);
		InsertMenuW(systemMenu, 0u, 1024u, 8191u, localizedMenuText);
	}

	private static void HookSystemMenuWindowToTopAlways()
	{
		IntPtr activeWindow = GetActiveWindow();
		if (activeWindow == IntPtr.Zero)
		{
			Debug.LogError("ウィンドウハンドルの取得に失敗しました");
			return;
		}
		newWndProcDelegate = CustomWndProc;
		wndProcHandle = GCHandle.Alloc(newWndProcDelegate);
		IntPtr functionPointerForDelegate = Marshal.GetFunctionPointerForDelegate(newWndProcDelegate);
		oldWndProc = SetWindowLongPtrRaw(activeWindow, -4, functionPointerForDelegate);
	}

	[MonoPInvokeCallback(typeof(WndProcDelegate))]
	private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
	{
		if (msg == 274 && wParam.ToInt32() == 8191)
		{
			if (isWindowTpAlways.Value)
			{
				ResetWindowOrder();
			}
			else
			{
				BringWindowToTopAlways();
			}
			return IntPtr.Zero;
		}
		return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
	}

	static ScreenSystem()
	{
		oldWndProc = IntPtr.Zero;
		isWindowTpAlways = new ReactiveProperty<bool>(value: false);
		ObservableSubscribeExtensions.Subscribe(ProjectLifetimeScope.GameObject.transform.OnApplicationQuitAsObservable(), delegate
		{
			IntPtr activeWindow = GetActiveWindow();
			if (activeWindow != IntPtr.Zero && oldWndProc != IntPtr.Zero)
			{
				SetWindowLongPtrRaw(activeWindow, -4, oldWndProc);
				wndProcHandle.Free();
				oldWndProc = IntPtr.Zero;
			}
			languageSubscription?.Dispose();
			languageSubscription = null;
		});
		try
		{
			languageSubscription = ObservableSubscribeExtensions.Subscribe(ProjectLifetimeScope.Resolve<LanguageSupplier>().Language.Skip(1), delegate
			{
				UpdateSystemMenuText();
			});
		}
		catch
		{
		}
	}
}
