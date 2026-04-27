namespace Bulbul;

public interface ITutorialView
{
	void Setup();

	void OpenTutorial(TutorialService.TutorialPageType pageType);

	void Deactivate();
}
