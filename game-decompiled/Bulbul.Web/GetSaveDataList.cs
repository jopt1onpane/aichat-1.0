namespace Bulbul.Web;

public readonly struct GetSaveDataList(string deviceID) : IRequest<GetSaveDataListResponse>
{
	[WebApiQueryParam("d")]
	public readonly string DeviceID = deviceID;
}
