using R3;

namespace Bulbul;

public interface IMusicTabUI : IMusicUIBase
{
	Observable<Unit> OnClickFileImportButton { get; }

	Observable<Unit> OnClickFolderImportButton { get; }

	Observable<Unit> OnClickTagButton { get; }

	void SetInteractableFileImport(bool enable);

	void SetInteractableFolderImport(bool enable);
}
