using R3;

namespace Bulbul;

public interface IMusicPlayListShowController : IMusicUIBase
{
	Observable<Unit> OnClickListCloseButton { get; }

	void DeactivatePlayList();

	void ActivatePlayList();
}
