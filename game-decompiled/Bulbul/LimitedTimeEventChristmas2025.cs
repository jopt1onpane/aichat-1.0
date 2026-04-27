using System;
using Bulbul.MasterData;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class LimitedTimeEventChristmas2025 : LimitedTimeEventBase
{
	[Inject]
	private DecorationService _decorationService;

	[SerializeField]
	private GameObject _santaHat;

	[SerializeField]
	private GameObject _christmasObjectsParent;

	[SerializeField]
	private SkinnedMeshRenderer _heroineHairRenderer;

	private int _hairShapeIndex;

	public override LimitedTimeEventType EventType()
	{
		return LimitedTimeEventType.Christmas2025;
	}

	public override void Setup()
	{
		_hairShapeIndex = _heroineHairRenderer.sharedMesh.GetBlendShapeIndex("blendShape5.Hair_hat");
		if ((object)_decorationService == null)
		{
			_decorationService = RoomLifetimeScope.Resolve<DecorationService>();
		}
	}

	public override bool IsActivateConditionMet()
	{
		if (SaveDataManager.Instance.PlayerData.IsFirstLogin || SaveDataManager.Instance.PlayerData.IsNeedTutorial)
		{
			return false;
		}
		if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber == 32f)
		{
			return false;
		}
		DateTime dateTime = new DateTime(2025, 12, 25);
		DateTime dateTime2 = new DateTime(2025, 12, 26);
		if (DateTime.Now >= dateTime && DateTime.Now < dateTime2)
		{
			return true;
		}
		return false;
	}

	public override void Activate()
	{
		_decorationService.ReplaceCatEarHeadphoneIfChristmasEvent();
		_santaHat.gameObject.SetActive(value: true);
		_christmasObjectsParent.gameObject.SetActive(value: true);
		_heroineHairRenderer.SetBlendShapeWeight(_hairShapeIndex, 100f);
	}

	public override void Deactivate()
	{
		_santaHat.gameObject.SetActive(value: false);
		_christmasObjectsParent.gameObject.SetActive(value: false);
		_heroineHairRenderer.SetBlendShapeWeight(_hairShapeIndex, 0f);
	}

	public override void OnStoryReady(ScenarioType scenarioType)
	{
		Deactivate();
	}

	public override void OnStoryTidying()
	{
		Activate();
	}
}
