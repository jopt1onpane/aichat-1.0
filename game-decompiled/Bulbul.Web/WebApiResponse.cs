using System;
using System.Text.Json.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace Bulbul.Web;

public readonly struct WebApiResponse<TResponse> where TResponse : IWebApiResponse
{
	public readonly TResponse Response;

	public readonly WebApiError Error;

	[JsonIgnore]
	public Exception Exception => Error.Exception;

	[JsonIgnore]
	public UnityWebRequest.Result Result => Error.Result;

	[JsonIgnore]
	public ErrorCode ErrorCode => Error.ErrorCode;

	[JsonIgnore]
	public ResetReason ResetReason => Error.ResetReason;

	[JsonIgnore]
	public int ErrorValue => Error.ErrorValue;

	[JsonIgnore]
	public bool HasError => Error.HasError;

	[JsonIgnore]
	public bool HasErrorOrReset => Error.HasErrorOrReset;

	public WebApiResponse(TResponse response, Exception exception, UnityWebRequest.Result result, ErrorCode errorCode, ResetReason resetReason)
	{
		Response = response;
		int errorValue = CreateErrorValue(errorCode, exception as UnityWebRequestException, resetReason, result);
		Error = new WebApiError(exception, result, errorCode, resetReason, errorValue);
		static int CreateErrorValue(ErrorCode errorCode2, UnityWebRequestException e, ResetReason resetReason2, UnityWebRequest.Result result2)
		{
			if (errorCode2 != ErrorCode.None)
			{
				return (int)errorCode2;
			}
			if (e != null && e.ResponseCode != 0L)
			{
				return (int)e.ResponseCode;
			}
			if (resetReason2 != ResetReason.None)
			{
				return (int)resetReason2;
			}
			if (result2 != UnityWebRequest.Result.Success)
			{
				return (int)result2;
			}
			return 0;
		}
	}

	public void ThrowException()
	{
		Error.ThrowException();
	}

	public void LogException()
	{
		Error.LogException();
	}
}
