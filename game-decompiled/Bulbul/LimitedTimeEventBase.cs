using Bulbul.MasterData;
using UnityEngine;

namespace Bulbul;

public abstract class LimitedTimeEventBase : MonoBehaviour
{
	public abstract LimitedTimeEventType EventType();

	public abstract bool IsActivateConditionMet();

	public abstract void Setup();

	public abstract void Activate();

	public abstract void Deactivate();

	public abstract void OnStoryReady(ScenarioType scenarioType);

	public abstract void OnStoryTidying();
}
