using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct CheckAccountResponse : IWebApiResponse
{
	public readonly bool IsUserIdLinked;

	public readonly CheckAccountStatus Status;

	public readonly string LinkSessionID;

	public readonly UserStatus SenderUserStatus;

	public readonly UserStatus OtherUserStatus;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public CheckAccountResponse(bool isUserIdLinked, CheckAccountStatus status, string linkSessionID, UserStatus senderUserStatus, UserStatus otherUserStatus, ErrorCode errorCode)
	{
		IsUserIdLinked = isUserIdLinked;
		Status = status;
		LinkSessionID = linkSessionID;
		SenderUserStatus = senderUserStatus;
		OtherUserStatus = otherUserStatus;
		ErrorCode = errorCode;
	}
}
