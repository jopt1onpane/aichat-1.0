using System.Collections.Generic;
using System.Linq;
using Bulbul.MasterData;
using UnityEngine;

namespace Bulbul;

public class LimitedTimeEventService : MonoBehaviour
{
	[SerializeField]
	[Header("期間限定イベントリスト")]
	private List<LimitedTimeEventBase> limitedTimeEvents;

	private LimitedTimeEventBase currentEvent;

	public void Setup()
	{
		UpdateLimitedTimeEventType();
	}

	public void ActivateEvent()
	{
		if (currentEvent != null)
		{
			currentEvent.Setup();
			currentEvent.Activate();
		}
	}

	private void UpdateLimitedTimeEventType()
	{
		LimitedTimeEventBase limitedTimeEventBase = null;
		foreach (LimitedTimeEventBase limitedTimeEvent in limitedTimeEvents)
		{
			if (limitedTimeEvent.IsActivateConditionMet())
			{
				limitedTimeEventBase = limitedTimeEvent;
				break;
			}
		}
		if (limitedTimeEventBase == null)
		{
			SaveDataManager.Instance.LimitedTimeEventSaveData.ChangeType(LimitedTimeEventType.None);
			SaveDataManager.Instance.SaveLimitedTimeEventData();
			return;
		}
		if (SaveDataManager.Instance.LimitedTimeEventSaveData.CurrentType.Value != limitedTimeEventBase.EventType())
		{
			SaveDataManager.Instance.LimitedTimeEventSaveData.ChangeType(limitedTimeEventBase.EventType());
			SaveDataManager.Instance.SaveLimitedTimeEventData();
		}
		currentEvent = limitedTimeEventBase;
	}

	public void OnStoryReady(ScenarioType scenarioType)
	{
		if (currentEvent != null)
		{
			currentEvent.OnStoryReady(scenarioType);
		}
	}

	public void OnStoryTidying()
	{
		if (currentEvent != null)
		{
			currentEvent.OnStoryTidying();
		}
	}

	public bool IsContainCurrentEvent(LimitedTimeEventType eventType)
	{
		if (currentEvent == null)
		{
			return false;
		}
		return currentEvent.EventType() == eventType;
	}

	public LimitedTimeEventAprilFool2026 GetAprilFoolEvent()
	{
		LimitedTimeEventBase limitedTimeEventBase = limitedTimeEvents.FirstOrDefault((LimitedTimeEventBase x) => x.GetType() == typeof(LimitedTimeEventAprilFool2026));
		if (limitedTimeEventBase == null)
		{
			Debug.LogError("LimitedTimeEventService: LimitedTimeEventAprilFool2026がlimitedTimeEventsから見つかりませんでした");
			return null;
		}
		return (LimitedTimeEventAprilFool2026)limitedTimeEventBase;
	}
}
