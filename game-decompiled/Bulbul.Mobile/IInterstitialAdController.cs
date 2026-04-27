using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul.Mobile;

public interface IInterstitialAdController
{
	bool IsNeedAd { get; }

	UniTask ShowAdAsync(CancellationToken ct);

	bool CanShowAd();
}
