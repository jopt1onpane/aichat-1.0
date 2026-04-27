using System.Runtime.InteropServices;

namespace Bulbul.Web;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Ping : IRequest<Nil>
{
}
