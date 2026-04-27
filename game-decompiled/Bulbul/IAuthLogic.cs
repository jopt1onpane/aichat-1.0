using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul;

public interface IAuthLogic
{
	UniTask<string> AuthAsync(CancellationToken ct);
}
