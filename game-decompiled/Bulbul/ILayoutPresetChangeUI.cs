using Cysharp.Threading.Tasks;
using R3;

namespace Bulbul;

public interface ILayoutPresetChangeUI
{
	Observable<bool> OnChangeCurrentData { get; }

	void Setup(IPresetDataService dataService);

	UniTask<int> ShowSaveDialogAsync();
}
