using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul;

public class LunaNewYear2026RoomController : MonoBehaviour
{
	[SerializeField]
	private Transform lantern;

	private ScenarioReader scenarioReader;

	private void Start()
	{
		if (SpecialLunaNewYear2026Util.Instance.IsValidPeriod())
		{
			SetActiveObjects(SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.CurrentValue);
		}
		else
		{
			SetActiveObjects(active: false);
		}
		SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Skip(1).Subscribe(SetActiveObjects).AddTo(this);
		scenarioReader = RoomLifetimeScope.Resolve<ScenarioReader>();
		scenarioReader.OnStartReady.Where((ScenarioType type) => type.IsLongStoryOrTutorial()).SubscribeAwait((ScenarioType type, CancellationToken ct) => OnScenarioStarted(type, ct), AwaitOperation.Switch).AddTo(this);
	}

	private void SetActiveObjects(bool active)
	{
		lantern.gameObject.SetActive(active);
	}

	private async UniTask OnScenarioStarted(ScenarioType scenarioType, CancellationToken ct)
	{
		if (scenarioType.IsLongStoryOrTutorial())
		{
			bool originActive = lantern.gameObject.activeSelf;
			if ((originActive || scenarioType == ScenarioType.Special_LunaNewYear2026) && (!originActive || scenarioType != ScenarioType.Special_LunaNewYear2026))
			{
				lantern.gameObject.SetActive(scenarioType == ScenarioType.Special_LunaNewYear2026);
				await scenarioReader.OnEndTidyingForStory.Merge(scenarioReader.OnEndTidyingInStory).ToUniTask(useFirstValue: true, ct);
				lantern.gameObject.SetActive(originActive && SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.CurrentValue);
			}
		}
	}
}
