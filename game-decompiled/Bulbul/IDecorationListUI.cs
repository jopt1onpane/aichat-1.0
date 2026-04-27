using R3;

namespace Bulbul;

public interface IDecorationListUI
{
	Observable<Unit> OnClickCloseButton { get; }

	void Setup();

	void Activate();

	void Deactivate();
}
