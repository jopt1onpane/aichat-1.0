using Bulbul.MasterData;
using R3;

namespace Bulbul;

public interface IRoomGameLongTalkNotice
{
	Observable<ScenarioType> OnReadyLongStoryAfterFade { get; }

	Observable<ScenarioType> OnEndLongStoryBeforeFade { get; }
}
