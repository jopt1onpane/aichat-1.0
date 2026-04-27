using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Firebase.Internal;

internal static class FirebaseInterops
{
	private static PropertyInfo _dataCollectionProperty;

	private static Type _appCheckType;

	private static MethodInfo _appCheckGetInstanceMethod;

	private static MethodInfo _appCheckGetTokenMethod;

	private static PropertyInfo _appCheckTokenResultProperty;

	private static PropertyInfo _appCheckTokenTokenProperty;

	private static bool _appCheckReflectionInitialized;

	private const string appCheckHeader = "X-Firebase-AppCheck";

	private static Type _authType;

	private static MethodInfo _authGetAuthMethod;

	private static PropertyInfo _authCurrentUserProperty;

	private static MethodInfo _userTokenAsyncMethod;

	private static PropertyInfo _userTokenTaskResultProperty;

	private static bool _authReflectionInitialized;

	private const string authHeader = "Authorization";

	private const string _unknownSdkVersion = "unknown";

	private static readonly Lazy<string> _sdkVersionFetcher;

	static FirebaseInterops()
	{
		_dataCollectionProperty = null;
		_appCheckReflectionInitialized = false;
		_authReflectionInitialized = false;
		_sdkVersionFetcher = new Lazy<string>(delegate
		{
			try
			{
				Type type = typeof(FirebaseApp).Assembly.GetType("Firebase.VersionInfo");
				if (type == null)
				{
					LogError("Firebase.VersionInfo type not found via reflection");
					return "unknown";
				}
				PropertyInfo property = type.GetProperty("SdkVersion", BindingFlags.Static | BindingFlags.NonPublic);
				if (property == null)
				{
					LogError("Firebase.VersionInfo.SdkVersion property not found via reflection.");
					return "unknown";
				}
				return (property.GetValue(null) as string) ?? "unknown";
			}
			catch (Exception arg)
			{
				LogError($"Error accessing SdkVersion via reflection: {arg}");
				return "unknown";
			}
		});
		InitializeAppReflection();
		InitializeAppCheckReflection();
		InitializeAuthReflection();
	}

	private static void LogError(string message)
	{
	}

	private static void InitializeAppReflection()
	{
		try
		{
			_dataCollectionProperty = typeof(FirebaseApp).GetProperty("IsDataCollectionDefaultEnabled", BindingFlags.Instance | BindingFlags.NonPublic);
			if (_dataCollectionProperty == null)
			{
				LogError("Could not find FirebaseApp.IsDataCollectionDefaultEnabled property via reflection.");
			}
			else if (_dataCollectionProperty.PropertyType != typeof(bool))
			{
				LogError("FirebaseApp.IsDataCollectionDefaultEnabled is not a bool, " + $"but is {_dataCollectionProperty.PropertyType}");
			}
		}
		catch (Exception arg)
		{
			LogError($"Failed to initialize FirebaseApp reflection: {arg}");
		}
	}

	public static bool GetIsDataCollectionDefaultEnabled(FirebaseApp firebaseApp)
	{
		if (firebaseApp == null || _dataCollectionProperty == null)
		{
			return false;
		}
		try
		{
			return (bool)_dataCollectionProperty.GetValue(firebaseApp);
		}
		catch (Exception arg)
		{
			LogError($"Error accessing 'IsDataCollectionDefaultEnabled': {arg}");
			return false;
		}
	}

	internal static string GetVersionInfoSdkVersion()
	{
		return _sdkVersionFetcher.Value;
	}

	private static void InitializeAppCheckReflection()
	{
		try
		{
			_appCheckReflectionInitialized = false;
			_appCheckType = Type.GetType("Firebase.AppCheck.FirebaseAppCheck, Firebase.AppCheck");
			if (_appCheckType == null)
			{
				return;
			}
			_appCheckGetInstanceMethod = _appCheckType.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(FirebaseApp) }, null);
			if (_appCheckGetInstanceMethod == null)
			{
				LogError("Could not find FirebaseAppCheck.GetInstance method via reflection.");
				return;
			}
			_appCheckGetTokenMethod = _appCheckType.GetMethod("GetAppCheckTokenAsync", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(bool) }, null);
			if (_appCheckGetTokenMethod == null)
			{
				LogError("Could not find GetAppCheckTokenAsync method via reflection.");
				return;
			}
			_appCheckTokenResultProperty = _appCheckGetTokenMethod.ReturnType.GetProperty("Result");
			if (_appCheckTokenResultProperty == null)
			{
				LogError("Could not find Result property on App Check token Task.");
				return;
			}
			_appCheckTokenTokenProperty = _appCheckTokenResultProperty.PropertyType.GetProperty("Token");
			if (_appCheckTokenTokenProperty == null)
			{
				LogError("Could not find Token property on AppCheckToken.");
			}
			else
			{
				_appCheckReflectionInitialized = true;
			}
		}
		catch (Exception arg)
		{
			LogError($"Exception during static initialization of FirebaseInterops: {arg}");
		}
	}

	internal static async Task<string> GetAppCheckTokenAsync(FirebaseApp firebaseApp)
	{
		if (!_appCheckReflectionInitialized)
		{
			return null;
		}
		try
		{
			object obj = _appCheckGetInstanceMethod.Invoke(null, new object[1] { firebaseApp });
			if (obj == null)
			{
				LogError("Failed to get FirebaseAppCheck instance via reflection.");
				return null;
			}
			object obj2 = _appCheckGetTokenMethod.Invoke(obj, new object[1] { false });
			if (!(obj2 is Task appCheckTokenTask))
			{
				LogError("Invoking GetToken did not return a Task.");
				return null;
			}
			await appCheckTokenTask;
			if (appCheckTokenTask.IsFaulted)
			{
				LogError($"Error getting App Check token: {appCheckTokenTask.Exception}");
				return null;
			}
			object value = _appCheckTokenResultProperty.GetValue(appCheckTokenTask);
			if (value == null)
			{
				LogError("App Check token result was null.");
				return null;
			}
			return _appCheckTokenTokenProperty.GetValue(value) as string;
		}
		catch (Exception arg)
		{
			LogError($"An error occurred while trying to fetch App Check token: {arg}");
		}
		return null;
	}

	private static void InitializeAuthReflection()
	{
		try
		{
			_authReflectionInitialized = false;
			_authType = Type.GetType("Firebase.Auth.FirebaseAuth, Firebase.Auth");
			if (_authType == null)
			{
				return;
			}
			_authGetAuthMethod = _authType.GetMethod("GetAuth", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(FirebaseApp) }, null);
			if (_authGetAuthMethod == null)
			{
				LogError("Could not find FirebaseAuth.GetAuth method via reflection.");
				return;
			}
			_authCurrentUserProperty = _authType.GetProperty("CurrentUser", BindingFlags.Instance | BindingFlags.Public);
			if (_authCurrentUserProperty == null)
			{
				LogError("Could not find FirebaseAuth.CurrentUser property via reflection.");
				return;
			}
			_userTokenAsyncMethod = _authCurrentUserProperty.PropertyType.GetMethod("TokenAsync", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(bool) }, null);
			if (_userTokenAsyncMethod == null)
			{
				LogError("Could not find FirebaseUser.TokenAsync(bool) method via reflection.");
				return;
			}
			_userTokenTaskResultProperty = _userTokenAsyncMethod.ReturnType.GetProperty("Result");
			if (_userTokenTaskResultProperty == null)
			{
				LogError("Could not find Result property on Auth token Task.");
			}
			else if (_userTokenTaskResultProperty.PropertyType != typeof(string))
			{
				LogError("Auth token Task's Result property is not a string, " + $"but is {_userTokenTaskResultProperty.PropertyType}");
			}
			else
			{
				_authReflectionInitialized = true;
			}
		}
		catch (Exception arg)
		{
			LogError($"Exception during static initialization of Auth reflection in FirebaseInterops: {arg}");
			_authReflectionInitialized = false;
		}
	}

	internal static async Task<string> GetAuthTokenAsync(FirebaseApp firebaseApp)
	{
		if (!_authReflectionInitialized)
		{
			return null;
		}
		try
		{
			object obj = _authGetAuthMethod.Invoke(null, new object[1] { firebaseApp });
			if (obj == null)
			{
				LogError("Failed to get FirebaseAuth instance via reflection.");
				return null;
			}
			object value = _authCurrentUserProperty.GetValue(obj);
			if (value == null)
			{
				return null;
			}
			object obj2 = _userTokenAsyncMethod.Invoke(value, new object[1] { false });
			if (!(obj2 is Task tokenTask))
			{
				LogError("Invoking TokenAsync did not return a Task.");
				return null;
			}
			await tokenTask;
			if (tokenTask.IsFaulted)
			{
				LogError($"Error getting Auth token: {tokenTask.Exception}");
				return null;
			}
			return _userTokenTaskResultProperty.GetValue(tokenTask) as string;
		}
		catch (Exception arg)
		{
			LogError($"An error occurred while trying to fetch Auth token: {arg}");
		}
		return null;
	}

	internal static async Task AddFirebaseTokensAsync(HttpRequestMessage request, FirebaseApp firebaseApp)
	{
		string value = await GetAppCheckTokenAsync(firebaseApp);
		if (!string.IsNullOrEmpty(value))
		{
			request.Headers.Add("X-Firebase-AppCheck", value);
		}
		string text = await GetAuthTokenAsync(firebaseApp);
		if (!string.IsNullOrEmpty(text))
		{
			request.Headers.Add("Authorization", "Firebase " + text);
		}
	}

	internal static async Task AddFirebaseTokensAsync(ClientWebSocket socket, FirebaseApp firebaseApp)
	{
		string text = await GetAppCheckTokenAsync(firebaseApp);
		if (!string.IsNullOrEmpty(text))
		{
			socket.Options.SetRequestHeader("X-Firebase-AppCheck", text);
		}
		string text2 = await GetAuthTokenAsync(firebaseApp);
		if (!string.IsNullOrEmpty(text2))
		{
			socket.Options.SetRequestHeader("Authorization", "Firebase " + text2);
		}
	}
}
