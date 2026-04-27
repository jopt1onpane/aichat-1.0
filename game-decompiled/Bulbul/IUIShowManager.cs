using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul;

public interface IUIShowManager
{
	bool IsShowUI { get; }

	void Setup();

	UniTask AllUIActivate(bool isUseDoComplete = false);

	UniTask AllUIDeactivate(bool isUseDoComplete = false);

	UniTask TutorialOtherUIActivate();

	UniTask TutorialOtherUIDeactivate(CancellationToken token);

	void AdjustUIForPlayScenario();

	void AdjustUIForEndScenario();

	void AdjustUIForStartTutorialScenario();
}
