using System;

namespace Bulbul.Web;

public class AppResetException : Exception
{
	public readonly ResetReason ResetReason;

	public AppResetException(ResetReason resetReason)
	{
		ResetReason = resetReason;
	}
}
