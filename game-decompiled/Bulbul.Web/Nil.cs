using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Bulbul.Web;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Nil : IWebApiResponse
{
	[JsonIgnore]
	public ErrorCode ErrorCode => ErrorCode.None;
}
