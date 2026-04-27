using System;
using System.Text.Json.Serialization;

namespace Bulbul.Web;

public readonly struct GetSaveDataListResponse : IWebApiResponse
{
	public readonly string[] List;

	public ErrorCode ErrorCode { get; }

	[JsonConstructor]
	public GetSaveDataListResponse(string[] list, ErrorCode errorCode)
	{
		List = list ?? Array.Empty<string>();
		ErrorCode = errorCode;
	}
}
