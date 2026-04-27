using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using Assets.SimpleSignIn.Apple.Scripts;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bulbul;

public class AppleAuthLogic : IAuthLogic
{
	private AppleAuthManager appleAuthManager;

	public async UniTask<string> AuthAsync(CancellationToken ct)
	{
		return await SigninWindows(ct);
	}

	private async UniTask<string> SigninAndroid(CancellationToken ct)
	{
		return "";
	}

	private async UniTask<string> SigniniOS(CancellationToken ct)
	{
		return "";
	}

	private async UniTask<string> SigninWindows(CancellationToken ct)
	{
		Assets.SimpleSignIn.Apple.Scripts.AppleAuth appleAuth = new Assets.SimpleSignIn.Apple.Scripts.AppleAuth();
		try
		{
			appleAuth.SignOut();
			await appleAuth.SignInAsync(caching: true, null, ct);
			return appleAuth.TokenResponse.IdToken;
		}
		catch (OperationCanceledException)
		{
			appleAuth.Cancel();
			throw;
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogError(message);
			return "";
		}
	}

	private static string GenerateRandomString(int length)
	{
		if (length <= 0)
		{
			throw new Exception("Expected nonce to have positive length");
		}
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		string text = string.Empty;
		int num = length;
		byte[] array = new byte[1];
		while (num > 0)
		{
			List<int> list = new List<int>(16);
			for (int i = 0; i < 16; i++)
			{
				rNGCryptoServiceProvider.GetBytes(array);
				list.Add(array[0]);
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (num == 0)
				{
					break;
				}
				int num2 = list[j];
				if (num2 < "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._".Length)
				{
					text += "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._"[num2];
					num--;
				}
			}
		}
		return text;
	}

	private static string GenerateSHA256NonceFromRawNonce(string rawNonce)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		byte[] bytes = Encoding.UTF8.GetBytes(rawNonce);
		byte[] array = sHA256Managed.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	private static string DescribeAppleError(IAppleError error)
	{
		if (error == null)
		{
			return "null IAppleError";
		}
		string text = (Enum.IsDefined(typeof(AuthorizationErrorCode), error.Code) ? ((AuthorizationErrorCode)error.Code/*cast due to .constrained prefix*/).ToString() : "UnknownCode");
		string text2 = ((error.LocalizedRecoveryOptions != null && error.LocalizedRecoveryOptions.Length != 0) ? string.Join(", ", error.LocalizedRecoveryOptions) : "none");
		return $"Code={error.Code}({text}), Domain={error.Domain}, Desc={error.LocalizedDescription}, FailureReason={error.LocalizedFailureReason}, Suggestion={error.LocalizedRecoverySuggestion}, RecoveryOptions=[{text2}]";
	}
}
