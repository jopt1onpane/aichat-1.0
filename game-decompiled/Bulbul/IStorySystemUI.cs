using System;
using System.Threading;
using Bulbul.MasterData;
using R3;

namespace Bulbul;

public interface IStorySystemUI
{
	Observable<Unit> OnClickButtonSkip { get; }

	StorySystemUI.MessageType CurrentMessageType { get; }

	SentenceSelectionButtonsUI SelectionButtonsUI { get; }

	CancellationToken CancellationTokenOnDestroy { get; }

	FadeController FadeController { get; }

	void Setup();

	void MainStoryReady(string novelID, ScenarioType scenarioType);

	void ActivateSkipButton();

	void DeactivateSkipButton();

	void DeactivateAutoButton();

	bool IsActiveNormalText();

	void ActivateNormalText();

	void DeactivateNormalText(Action onEndAction = null);

	void DeactivateTransparentButton();

	void ActivateTransparentButton();

	void ChangeMessageType(StorySystemUI.MessageType type);

	void DeactivateBottomBackImage();

	void Begin(bool isUseMask);

	void Finish();

	void AddOnClickCallback(Action action);

	ScenarioTextMessage CurrentTextMessage();

	void DebugClearAll();

	void DebugDeactivateParentUI();

	void DebugActivateParentUI();
}
