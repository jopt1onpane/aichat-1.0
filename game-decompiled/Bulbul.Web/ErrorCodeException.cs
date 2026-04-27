using System;

namespace Bulbul.Web;

public class ErrorCodeException : Exception
{
	public readonly ErrorCode ErrorCode;

	public ErrorCodeException(ErrorCode errorCode)
	{
		ErrorCode = errorCode;
	}
}
