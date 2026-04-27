using System;
using System.Linq;
using Bulbul;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using R3;
using UnityEngine;
using VContainer;

public class MotionSoundController : MonoBehaviour
{
	private enum MotionEpisodeNumber
	{
		Voice_Motion_StretchStart_001 = 1,
		Voice_Motion_StretchStart_002 = 2,
		Voice_Motion_StretchStart_003 = 3,
		Voice_Motion_StretchEnd_001 = 4,
		Voice_Motion_Guts_001 = 5,
		Voice_Motion_Guts_002 = 6,
		Voice_Motion_Guts_003 = 7,
		Voice_Motion_Guts_004 = 8,
		Voice_Motion_DrinkHot_001 = 9,
		Voice_Motion_DrinkToCool_001 = 10,
		Voice_Motion_Thinking_001 = 11,
		Voice_Motion_Thinking_002 = 12,
		Voice_Motion_Laugh_001 = 13,
		Voice_Motion_Laugh_002 = 14,
		Voice_Motion_Laugh_003 = 15,
		Voice_Motion_JumpUpStart_001 = 16,
		Voice_Motion_JumpUpEnd_001 = 17,
		Voice_Motion_Interest_001 = 18,
		Voice_Motion_Interest_002 = 19,
		Voice_Motion_Question_001 = 20,
		Voice_Motion_Understand_001 = 21,
		Voice_Motion_DrinkToCool_002 = 25,
		Voice_Motion_DropPen_001 = 28,
		Voice_Motion_DropPen_002 = 29
	}

	public enum MotionVoicePlayKind
	{
		None,
		Random,
		Type_1,
		Type_2,
		Type_3,
		Type_4,
		Type_5
	}

	[Inject]
	private AmbientSeService _ambientSeService;

	[Inject]
	private HeroineService _heroineService;

	[Inject]
	private FacilityVoiceTextScenario _facilityVoiceTextScenario;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PomodoroService _pomodoroService;

	[SerializeField]
	[Header("ストレッチのボイスが発生する確率")]
	[Range(0f, 100f)]
	public int StretchVoiceOccurrenceProbability;

	[SerializeField]
	[Header("考えるボイスが発生する確率")]
	[Range(0f, 100f)]
	public int ThinkVoiceOccurrenceProbability;

	[SerializeField]
	[Header("笑うボイスが発生する確率")]
	[Range(0f, 100f)]
	public int LaughVoiceOccurrenceProbability;

	[SerializeField]
	[Header("関心ボイスが発生する確率")]
	[Range(0f, 100f)]
	public int InterestVoiceOccurrenceProbability;

	private MotionVoicePlayKind _voicePlayKind = MotionVoicePlayKind.Random;

	private HeroineVoiceController _heroineVoiceController;

	private MotionSoundCup _soundCup = new MotionSoundCup();

	private MotionSoundKeyboard _soundKeyboard = new MotionSoundKeyboard();

	private bool _isFridgeSequenceInProgress;

	private Subject<Unit> _onPlayMotionVoice = new Subject<Unit>();

	private const float FridgeOpenToTakeDelaySeconds = 0.6f;

	private const float FridgeTakeToCloseDelaySeconds = 0.8f;

	public Observable<Unit> OnPlayMotionVoice => _onPlayMotionVoice;

	public void Setup(HeroineVoiceController heroineVoiceController)
	{
		_heroineVoiceController = heroineVoiceController;
		_soundKeyboard.Setup(this);
	}

	public void ChangeVoicePlayKind(MotionVoicePlayKind playKind)
	{
		_voicePlayKind = playKind;
	}

	private bool IsCanPlayMotionVoice()
	{
		if (!SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Value)
		{
			return false;
		}
		if ((DateTime.Now - _heroineVoiceController.LastVoiceDateTime).TotalSeconds < 10.0)
		{
			return false;
		}
		return true;
	}

	public void PlayStretchStartVoice()
	{
		if (!IsCanPlayMotionVoice())
		{
			return;
		}
		int num = 0;
		switch (_voicePlayKind)
		{
		case MotionVoicePlayKind.Random:
			if (UnityEngine.Random.Range(1, 101) <= StretchVoiceOccurrenceProbability)
			{
				num = UnityEngine.Random.Range(1, 4);
			}
			break;
		case MotionVoicePlayKind.Type_1:
			num = 1;
			break;
		case MotionVoicePlayKind.Type_2:
			num = 2;
			break;
		case MotionVoicePlayKind.Type_3:
			num = 3;
			break;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayStretchEndVoice()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			MotionVoicePlayKind voicePlayKind = _voicePlayKind;
			if ((uint)(voicePlayKind - 1) <= 1u)
			{
				num = 4;
			}
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayGutsVoice()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			switch (_voicePlayKind)
			{
			case MotionVoicePlayKind.Random:
				num = UnityEngine.Random.Range(5, 9);
				break;
			case MotionVoicePlayKind.Type_1:
				num = 5;
				break;
			case MotionVoicePlayKind.Type_2:
				num = 6;
				break;
			case MotionVoicePlayKind.Type_3:
				num = 7;
				break;
			case MotionVoicePlayKind.Type_4:
				num = 8;
				break;
			}
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayDrinkHotVoice()
	{
		int num = 0;
		MotionVoicePlayKind voicePlayKind = _voicePlayKind;
		if ((uint)(voicePlayKind - 1) <= 1u)
		{
			num = 9;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayDrinkToCoolVoice()
	{
		int num = 0;
		MotionVoicePlayKind voicePlayKind = _voicePlayKind;
		if ((uint)(voicePlayKind - 1) <= 1u)
		{
			num = 10;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayDrinkToCoolVoice_2()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			num = 25;
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayThinkingVoice()
	{
		if (!IsCanPlayMotionVoice())
		{
			return;
		}
		int num = 0;
		switch (_voicePlayKind)
		{
		case MotionVoicePlayKind.Random:
			if (UnityEngine.Random.Range(1, 101) <= ThinkVoiceOccurrenceProbability)
			{
				num = UnityEngine.Random.Range(11, 13);
			}
			break;
		case MotionVoicePlayKind.Type_1:
			num = 11;
			break;
		case MotionVoicePlayKind.Type_2:
			num = 12;
			break;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayLaughVoice()
	{
		if (!IsCanPlayMotionVoice())
		{
			return;
		}
		int num = 0;
		switch (_voicePlayKind)
		{
		case MotionVoicePlayKind.Random:
			if (UnityEngine.Random.Range(1, 101) <= ThinkVoiceOccurrenceProbability)
			{
				num = UnityEngine.Random.Range(13, 16);
			}
			break;
		case MotionVoicePlayKind.Type_1:
			num = 13;
			break;
		case MotionVoicePlayKind.Type_2:
			num = 14;
			break;
		case MotionVoicePlayKind.Type_3:
			num = 15;
			break;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayJumpUpStartVoice()
	{
		int episodeNumber = 16;
		_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, episodeNumber);
		_onPlayMotionVoice.OnNext(Unit.Default);
	}

	public void PlayJumpUpEndVoice()
	{
		int episodeNumber = 17;
		_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, episodeNumber);
		_onPlayMotionVoice.OnNext(Unit.Default);
	}

	public void PlayInterestVoice()
	{
		if (!IsCanPlayMotionVoice())
		{
			return;
		}
		int num = 0;
		switch (_voicePlayKind)
		{
		case MotionVoicePlayKind.Random:
			if (UnityEngine.Random.Range(1, 101) <= LaughVoiceOccurrenceProbability)
			{
				num = UnityEngine.Random.Range(18, 20);
			}
			break;
		case MotionVoicePlayKind.Type_1:
			num = 18;
			break;
		case MotionVoicePlayKind.Type_2:
			num = 19;
			break;
		}
		if (num != 0)
		{
			_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
			_onPlayMotionVoice.OnNext(Unit.Default);
		}
	}

	public void PlayQuestionVoice()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			MotionVoicePlayKind voicePlayKind = _voicePlayKind;
			if ((uint)(voicePlayKind - 1) <= 1u)
			{
				num = 20;
			}
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayUnderstandVoice()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			MotionVoicePlayKind voicePlayKind = _voicePlayKind;
			if ((uint)(voicePlayKind - 1) <= 1u)
			{
				num = 21;
			}
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayDropPenVoice()
	{
		if (IsCanPlayMotionVoice())
		{
			int num = 0;
			switch (_voicePlayKind)
			{
			case MotionVoicePlayKind.Random:
				num = UnityEngine.Random.Range(28, 30);
				break;
			case MotionVoicePlayKind.Type_1:
				num = 28;
				break;
			case MotionVoicePlayKind.Type_2:
				num = 29;
				break;
			}
			if (num != 0)
			{
				_facilityVoiceTextScenario.WantPlayVoiceTextScenario(ScenarioType.MotionScenario, num);
				_onPlayMotionVoice.OnNext(Unit.Default);
			}
		}
	}

	public void PlayKeyboard()
	{
		_soundKeyboard.Play();
	}

	public void StopKeyboard()
	{
		_soundKeyboard.Stop();
	}

	public void PlayMoveKeyboard()
	{
		AmbientSeType ambientSeType = AmbientSeType.MoveKeyboard;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayPutKeyboard()
	{
		AmbientSeType ambientSeType = AmbientSeType.PutKeyboard;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayChairCreaking()
	{
		AmbientSeType ambientSeType = AmbientSeType.ChairCreaking;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayChairCaster()
	{
		AmbientSeType ambientSeType = AmbientSeType.ChairCaster;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlaySitSofa()
	{
		AmbientSeType ambientSeType = AmbientSeType.SitSofa_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayMoveNote()
	{
		AmbientSeType ambientSeType = AmbientSeType.MoveNote;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayPutNote()
	{
		AmbientSeType ambientSeType = AmbientSeType.PutNote;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void WritePen()
	{
		AmbientSeType ambientSeType = AmbientSeType.WritePen;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void StopWritePen()
	{
		_ambientSeService.Stop(AmbientSeType.WritePen);
	}

	public void StickPenIntoNote()
	{
		AmbientSeType ambientSeType = AmbientSeType.StickPenIntoNote;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayDropPen()
	{
		AmbientSeType ambientSeType = AmbientSeType.DropPen;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlaceCup(string putPlaceType)
	{
		MotionSoundCup.PutPlaceType putPlaceType2 = MotionSoundCup.TryParsePlaceType(putPlaceType);
		_soundCup.ChangePutPlace(putPlaceType2);
		_soundCup.Play();
	}

	public void RubbingClothes1()
	{
		AmbientSeType ambientSeType = AmbientSeType.RubbingClothes_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void RubbingClothes2()
	{
		AmbientSeType ambientSeType = AmbientSeType.RubbingClothes_2;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void RubbingClothes3()
	{
		AmbientSeType ambientSeType = AmbientSeType.RubbingClothes_3;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void RubbingClothes4()
	{
		AmbientSeType ambientSeType = AmbientSeType.RubbingClothes_4;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayWalk_1()
	{
		AmbientSeType ambientSeType = AmbientSeType.Walk_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void StopWalk_1()
	{
		AmbientSeType sound = AmbientSeType.Walk_1;
		_ambientSeService.Stop(sound);
	}

	public bool IsPlayingWalkSound()
	{
		return IsPlayingAmbientSe(AmbientSeType.Walk_1);
	}

	public void TextbookFlipPage()
	{
		AmbientSeType ambientSeType = AmbientSeType.PageFlipSoundTextbook;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void NovelFlipPage()
	{
		AmbientSeType ambientSeType = AmbientSeType.PageFlipSoundNovel;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayHandClap()
	{
		AmbientSeType ambientSeType = AmbientSeType.HandClap;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayOpenWindow()
	{
		AmbientSeType ambientSeType = AmbientSeType.OpenWindow;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayCloseWindow()
	{
		AmbientSeType ambientSeType = AmbientSeType.CloseWindow;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayCoffeeMakerAll()
	{
		AmbientSeType ambientSeType = AmbientSeType.CoffeeMakerAll;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = AmbientSeType.CoffeeMakerAll,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public bool IsPlayingKitchenSound()
	{
		if (!_isFridgeSequenceInProgress && !IsPlayingAmbientSe(AmbientSeType.KitchenWaterOnly_1) && !IsPlayingAmbientSe(AmbientSeType.KitchenWashingCup_1) && !IsPlayingAmbientSe(AmbientSeType.FridgeDoorOpen))
		{
			return IsPlayingAmbientSe(AmbientSeType.FridgeDoorClose);
		}
		return true;
	}

	private bool IsPlayingAmbientSe(AmbientSeType soundType)
	{
		AmbientSeSoundMasterData ambientSeSoundMasterData = _masterDataLoader.AmbientSeMasterList.FirstOrDefault((AmbientSeSoundMasterData x) => x.AmbientSeSound == soundType);
		if (ambientSeSoundMasterData == null)
		{
			return false;
		}
		AudioPlayer audioPlayerByName = SingletonMonoBehaviour<AmbientSEManager>.Instance.GetAudioPlayerByName(ambientSeSoundMasterData.AudioClipName);
		if (audioPlayerByName == null)
		{
			return false;
		}
		if (audioPlayerByName.CurrentState != AudioPlayer.State.Playing && audioPlayerByName.CurrentState != AudioPlayer.State.Delay)
		{
			return audioPlayerByName.CurrentState == AudioPlayer.State.Fading;
		}
		return true;
	}

	public void PlayKitchenWaterOnly_1()
	{
		AmbientSeType ambientSeType = AmbientSeType.KitchenWaterOnly_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayKitchenWashingCup_1()
	{
		AmbientSeType ambientSeType = AmbientSeType.KitchenWashingCup_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayFridgeDoorOpen()
	{
		AmbientSeType ambientSeType = AmbientSeType.FridgeDoorOpen;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayTakeOutFromFridge_1()
	{
		AmbientSeType ambientSeType = AmbientSeType.PlayTakeOutFromFridge_1;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayFridgeDoorClose()
	{
		AmbientSeType ambientSeType = AmbientSeType.FridgeDoorClose;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayFridgeSequence()
	{
		PlayFridgeSequenceAsync().Forget();
	}

	private async UniTaskVoid PlayFridgeSequenceAsync()
	{
		_isFridgeSequenceInProgress = true;
		try
		{
			PlayFridgeDoorOpen();
			await UniTask.WaitUntil(() => !IsPlayingAmbientSe(AmbientSeType.FridgeDoorOpen), PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
			await UniTask.Delay(TimeSpan.FromSeconds(0.6000000238418579));
			PlayTakeOutFromFridge_1();
			await UniTask.WaitUntil(() => !IsPlayingAmbientSe(AmbientSeType.PlayTakeOutFromFridge_1), PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
			await UniTask.Delay(TimeSpan.FromSeconds(0.800000011920929));
			PlayFridgeDoorClose();
			await UniTask.WaitUntil(() => !IsPlayingAmbientSe(AmbientSeType.FridgeDoorClose), PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
		}
		finally
		{
			_isFridgeSequenceInProgress = false;
		}
	}

	public void PlayEatenCupcakeCrushCup()
	{
		AmbientSeType ambientSeType = AmbientSeType.EatenCupcakeCrushCup;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void PlayEatenCupcakeRubHands()
	{
		AmbientSeType ambientSeType = AmbientSeType.EatenCupcakeRubHands;
		_ambientSeService.Play(new AmbientSeParam
		{
			AmbientSeSound = ambientSeType,
			IsAllowsDuplicate = false,
			VolumeRate = _masterDataLoader.AmbientSEVolumeData.GetVolume(ambientSeType)
		});
	}

	public void Update()
	{
		if (_heroineService.GetCurrentAnimationType() != HeroineService.AnimationType.WorkBase001 && _heroineService.GetCurrentAnimationType() != HeroineService.AnimationType.WorkBase002_KeyType)
		{
			StopKeyboard();
		}
		if (_heroineService.GetCurrentAnimationType() != HeroineService.AnimationType.WorkBase003)
		{
			StopWritePen();
		}
	}
}
