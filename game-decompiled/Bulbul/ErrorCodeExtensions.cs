namespace Bulbul;

public static class ErrorCodeExtensions
{
	public static bool IsSaveDataKind(this ErrorCode errorCode)
	{
		if (errorCode != ErrorCode.SaveDataNotFound)
		{
			return errorCode == ErrorCode.SaveDataUploadFailed;
		}
		return true;
	}
}
