using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct GetNewsResponse : IWebApiResponse
{
	public NewsData[] NewsDatas { get; }

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public GetNewsResponse(NewsData[] newsDatas, ErrorCode errorCode)
	{
		NewsDatas = newsDatas;
		ErrorCode = errorCode;
	}
}
