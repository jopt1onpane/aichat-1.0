using System;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class LimitedTimeEventAprilFool2026 : LimitedTimeEventBase
{
	[Inject]
	private DecorationService _decorationService;

	[Inject]
	private ScenarioReader _scenarioReader;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[SerializeField]
	[Header("サトネのメガネオブジェクト")]
	private GameObject _heroineGlasses;

	[SerializeField]
	[Header("サトネのサングラス")]
	private AprilFoolSunglasses _heroineSunglasses;

	[SerializeField]
	[Header("コウちゃんのサングラス")]
	private AprilFoolSunglasses _kouchanSunglasses;

	[SerializeField]
	[Header("話したがるステート")]
	private HeroineWantTalk _heroineWantTalk;

	[SerializeField]
	private float sunglassesShineStartDuration = 2f;

	[SerializeField]
	private float sunglassesShineStayDuration = 0.5f;

	[SerializeField]
	private float sunglassesShineEndDuration = 0.8f;

	[SerializeField]
	private float sunglassesShineDelayDuration = 2f;

	private int _hairShapeIndex;

	private CancellationTokenSource _cts;

	private bool _isWantTalkShine;

	private bool _isShineLoopRunning;

	public override LimitedTimeEventType EventType()
	{
		return LimitedTimeEventType.AprilFool2026;
	}

	public override void Setup()
	{
		if ((object)_decorationService == null)
		{
			_decorationService = RoomLifetimeScope.Resolve<DecorationService>();
		}
		_heroineSunglasses.Setup();
		_kouchanSunglasses.Setup();
		if (!SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains("event_2026_aprilfool_001"))
		{
			AwakeHeroineSunglassesShineLoop().Forget();
		}
		ObservableSubscribeExtensions.Subscribe(_heroineWantTalk.OnStartWantTalk, delegate
		{
			StartHeroineSunglassesShineLoop();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_heroineWantTalk.OnEndWantTalk, delegate
		{
			StopHeroineSunglassesShineLoop(isImmediate: false);
		}).AddTo(this);
		_scenarioReader.OnEndStory.Subscribe(delegate(ScenarioType scenarioType)
		{
			if (scenarioType == ScenarioType.Event_2026_AprilFool)
			{
				string playingScenarioGroupID = _scenarioReader.PlayingScenarioGroupID;
				if (!SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Contains(playingScenarioGroupID))
				{
					SaveDataManager.Instance.ScenarioProgressData.PlayedScenarioGroupIDs.Add(playingScenarioGroupID);
					SaveDataManager.Instance.SaveScenarioProgressData();
				}
			}
		}).AddTo(this);
		async UniTaskVoid AwakeHeroineSunglassesShineLoop()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(2.5));
			StartHeroineSunglassesShineLoop();
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
		if (SaveDataManager.Instance.ScenarioProgressData.NextEpisodeNumber <= 3f)
		{
			return false;
		}
		DateTime dateTime = new DateTime(2026, 4, 1);
		DateTime dateTime2 = new DateTime(2026, 4, 2);
		if (DateTime.Now >= dateTime && DateTime.Now < dateTime2)
		{
			return true;
		}
		return false;
	}

	public override void Activate()
	{
		_heroineSunglasses.Activate();
		_kouchanSunglasses.Activate();
		_decorationService.ReplaceGlassesIfAprilFoolEvent();
	}

	public override void Deactivate()
	{
		StopHeroineSunglassesShineLoop();
		_heroineSunglasses.Deactivate();
		_kouchanSunglasses.Deactivate();
	}

	private void StartHeroineSunglassesShineLoop()
	{
		_isWantTalkShine = true;
		if (!_isShineLoopRunning)
		{
			_isShineLoopRunning = true;
			_cts = new CancellationTokenSource();
			RunHeroineSunglassesShineLoopAsync(_cts.Token).Forget();
		}
	}

	private void StopHeroineSunglassesShineLoop(bool isImmediate = true)
	{
		_isWantTalkShine = false;
		if (isImmediate)
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		}
	}

	private async UniTaskVoid RunHeroineSunglassesShineLoopAsync(CancellationToken token)
	{
		_ = 1;
		try
		{
			while (_isWantTalkShine)
			{
				await _heroineSunglasses.Shine(sunglassesShineStartDuration, sunglassesShineStayDuration, sunglassesShineEndDuration, token);
				if (!_isWantTalkShine)
				{
					break;
				}
				await UniTask.Delay(TimeSpan.FromSeconds(sunglassesShineDelayDuration), ignoreTimeScale: false, PlayerLoopTiming.Update, token);
			}
		}
		finally
		{
			_isShineLoopRunning = false;
			_cts?.Dispose();
			_cts = null;
			if (_isWantTalkShine)
			{
				StartHeroineSunglassesShineLoop();
			}
		}
	}

	public override void OnStoryReady(ScenarioType scenarioType)
	{
		if (scenarioType != ScenarioType.Event_2026_AprilFool)
		{
			Deactivate();
			if (scenarioType.IsLongStoryOrTutorial())
			{
				ApplyGlassesByPreset();
			}
		}
	}

	public override void OnStoryTidying()
	{
		Activate();
		_decorationService.ReplaceGlassesIfAprilFoolEvent();
	}

	public async UniTaskVoid ShineSunglassesForSatone(float startDuration, float waitDuration, float endDuration)
	{
		_heroineSunglasses.Shine(startDuration, waitDuration, endDuration).Forget();
	}

	public async UniTaskVoid ShineSunglassesForKouchan(float startDuration, float waitDuration, float endDuration)
	{
		_kouchanSunglasses.Shine(startDuration, waitDuration, endDuration).Forget();
	}

	private void ApplyGlassesByPreset()
	{
		DecorationSkinMasterData decorationSkinMasterData = _masterDataLoader.DecorationMaster.GetSkinsByCategory(DecorationService.DecorationCategoryType.Glasses).FirstOrDefault((DecorationSkinMasterData skin) => SaveDataManager.Instance.DecorationSaveData.DecorationDic.TryGetValue(skin.SkinType, out var value) && value.IsActive.Value);
		if (decorationSkinMasterData != null)
		{
			_decorationService.ChangeDecoration(decorationSkinMasterData.SkinType, isSave: false);
		}
	}
}
