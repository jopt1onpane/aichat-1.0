using R3;

namespace Bulbul;

public interface IStorySelectUI
{
	Observable<Unit> OnClickTagChangeToMainButton { get; }

	Observable<Unit> OnClickTagChangeToSpecialButton { get; }

	Observable<Unit> OnClickCloseButton { get; }

	void Setup(FacilityStory facilityStory);

	void Activate();

	void Deactivate();
}
