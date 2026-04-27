using System;
using System.Runtime.ExceptionServices;
using System.Text.Json.Serialization;
using UnityEngine.Networking;

namespace Bulbul.Web;

public readonly struct WebApiError
{
	[JsonIgnore]
	public readonly Exception Exception;

	public readonly UnityWebRequest.Result Result;

	public readonly ErrorCode ErrorCode;

	public readonly ResetReason ResetReason;

	public readonly int ErrorValue;

	public readonly bool IsValid;

	public bool HasError
	{
		get
		{
			if (ErrorValue == 0)
			{
				return Exception != null;
			}
			return true;
		}
	}

	public bool HasErrorOrReset
	{
		get
		{
			if (!HasError)
			{
				return ResetReason != ResetReason.None;
			}
			return true;
		}
	}

	public WebApiError(Exception exception, UnityWebRequest.Result result, ErrorCode errorCode, ResetReason resetReason, int errorValue)
	{
		Exception = exception;
		Result = result;
		ErrorCode = errorCode;
		ResetReason = resetReason;
		ErrorValue = errorValue;
		IsValid = true;
	}

	public void ThrowException()
	{
		if (Exception != null)
		{
			ExceptionDispatchInfo.Throw(Exception);
		}
	}

	public void LogException()
	{
		if (Exception != null)
		{
			Debug.LogError($"Exception: {Exception}, Message: {Exception.Message}");
		}
	}
}
