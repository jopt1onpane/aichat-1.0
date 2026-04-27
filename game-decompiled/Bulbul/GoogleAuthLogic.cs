using System;
using System.Threading;
using Assets.SimpleSignIn.Google.Scripts;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class GoogleAuthLogic : IAuthLogic
{
	[Inject]
	private NativeProxy nativeProxy;

	public async UniTask<string> AuthAsync(CancellationToken ct)
	{
		return await SigninWindows(ct);
	}

	private async UniTask<string> SigniniOS(CancellationToken ct)
	{
		return (await UniTask.WhenAny(nativeProxy.OnGetIDToken.ToUniTask(useFirstValue: true, ct), nativeProxy.OnGetIDTokenFailed.ToUniTask(useFirstValue: true, ct).ContinueWith((string _) => ""))).Item2;
	}

	private async UniTask<string> SigninAndroid(CancellationToken ct)
	{
		return (await UniTask.WhenAny(nativeProxy.OnGetIDToken.ToUniTask(useFirstValue: true, ct), nativeProxy.OnGetIDTokenFailed.ToUniTask(useFirstValue: true, ct).ContinueWith((string _) => ""))).Item2;
	}

	private async UniTask<string> SigninWindows(CancellationToken ct)
	{
		GoogleAuth googleAuth = new GoogleAuth();
		try
		{
			googleAuth.SignOut();
			return (await googleAuth.GetTokenResponseAsync(ct).AsUniTask()).IdToken;
		}
		catch (OperationCanceledException)
		{
			googleAuth.Cancel();
			throw;
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		return "";
	}
}
