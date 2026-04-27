using System;
using Bulbul.MasterData;

namespace Bulbul;

public interface IStoryController
{
	bool IsTalkLog { get; }

	bool IsStoryStartReady { get; }

	bool IsStoryPlayEnd { get; }

	float EpisodeNumber { get; }

	void StartTutorialStory(Action onEndAction);

	void SaveScenarioPlayedLog();

	void StartStory(ScenarioType scenarioType, float episodeNum, bool isTalkLog = false);

	void Ready();

	void StartStory();

	void EndStory();
}
