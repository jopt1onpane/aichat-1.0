using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bulbul.MasterData;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bulbul;

public class MasterDataLoader : IDisposable
{
	private const string addressablesGroupsLabel = "MasterData";

	private AsyncOperationHandle<IList<ScriptableObject>> _scriptableMasterHandle;

	public IReadOnlyList<MusicData> MusicDataList;

	public IReadOnlyList<DirectionMusicData> DirectionMusicDataList;

	public IReadOnlyList<AmbientSoundMasterData> AmbientMasterList;

	public IReadOnlyList<AmbientSeSoundMasterData> AmbientSeMasterList;

	public IReadOnlyList<SystemSeMasterData> SystemSeMasterList;

	public IReadOnlyList<NovelData> NovelMasterList;

	public IReadOnlyDictionary<string, LocalizationData> LocalizationList;

	public IReadOnlyList<ScenarioGroupData> ScenarioGroupMasterList;

	public IReadOnlyList<UnlockDecorationData> UnlockDecorationMasterList;

	public IReadOnlyList<UnlockEnvironmentData> UnlockEnvironmentMasterList;

	public TalkSpeedData TalkSpeedData;

	public GamePomodoroTalkData GamePomodoroTalkData;

	public GamePomodoroVoiceData GamePomodoroVoiceData;

	public GameEndTalkData GameEndTalkData;

	public SmallTalkData SmallTalkData;

	public LevelUpInfoData LevelUpInfoData;

	public PointUpInfoData PointUpInfoData;

	public AlterEgoParameter AlterEgoData;

	public BearsRestaurantParameter BearsRestaurantData;

	public Valentine2026Parameter Valentine2026Data;

	public LunaNewYear2026Parameter LunaNewYear2026Data;

	public NearSpring2026Parameter NearSpring2026Data;

	public AmbientSeVolumeMaster AmbientSEVolumeData;

	public AllVolumeMaster AllVolumeData;

	public HeroineAIMaster HeroineAIMasterData;

	public DecorationMaster DecorationMaster;

	public EnvironmentMaster EnvironmentMaster;

	private readonly UniTaskCompletionSource loadTask = new UniTaskCompletionSource();

	public bool IsLoaded { get; private set; }

	public async UniTask Load()
	{
		Debug.LogDeveloperCheck("[Entry]⇒ MasterData Start Load");
		_scriptableMasterHandle = Addressables.LoadAssetsAsync<ScriptableObject>("MasterData", null);
		foreach (ScriptableObject item in await _scriptableMasterHandle.Task.AsUniTask())
		{
			if (!(item is MusicMaster musicMaster))
			{
				if (!(item is DirectionMusicMaster directionMusicMaster))
				{
					if (!(item is AmbientMaster ambientMaster))
					{
						if (!(item is AmbientSeMaster ambientSeMaster))
						{
							if (!(item is SystemSeMaster systemSeMaster))
							{
								if (!(item is NovelMaster novelMaster))
								{
									if (!(item is LocalizationMaster localizationMaster))
									{
										if (!(item is ScenarioGroupMaster scenarioGroupMaster))
										{
											if (!(item is UnlockDecorationMaster unlockDecorationMaster))
											{
												if (!(item is UnlockEnvironmentMaster unlockEnvironmentMaster))
												{
													if (!(item is GameParameterMaster gameParameterMaster))
													{
														if (!(item is AmbientSeVolumeMaster ambientSEVolumeData))
														{
															if (!(item is AllVolumeMaster allVolumeData))
															{
																if (!(item is HeroineAIMaster heroineAIMasterData))
																{
																	if (!(item is DecorationMaster decorationMaster))
																	{
																		if (item is EnvironmentMaster environmentMaster)
																		{
																			EnvironmentMaster = environmentMaster;
																		}
																	}
																	else
																	{
																		DecorationMaster = decorationMaster;
																	}
																}
																else
																{
																	HeroineAIMasterData = heroineAIMasterData;
																}
															}
															else
															{
																AllVolumeData = allVolumeData;
															}
														}
														else
														{
															AmbientSEVolumeData = ambientSEVolumeData;
														}
													}
													else
													{
														TalkSpeedData = gameParameterMaster.TalkSpeedData;
														GamePomodoroTalkData = gameParameterMaster.PomodoroTalkData;
														GamePomodoroVoiceData = gameParameterMaster.PomodoroVoiceData;
														GameEndTalkData = gameParameterMaster.GameEndTalkData;
														SmallTalkData = gameParameterMaster.SmallTalkData;
														LevelUpInfoData = gameParameterMaster.LevelUpInfoData;
														PointUpInfoData = gameParameterMaster.PointUpInfoData;
														AlterEgoData = gameParameterMaster.AlterEgoData;
														BearsRestaurantData = gameParameterMaster.BearsRestaurantData;
														Valentine2026Data = gameParameterMaster.Valentine2026Data;
														LunaNewYear2026Data = gameParameterMaster.LunaNewYear2026Data;
														NearSpring2026Data = gameParameterMaster.NearSpring2026Data;
													}
												}
												else
												{
													UnlockEnvironmentMasterList = unlockEnvironmentMaster.Items;
												}
											}
											else
											{
												UnlockDecorationMasterList = unlockDecorationMaster.Items;
											}
										}
										else
										{
											ScenarioGroupMasterList = scenarioGroupMaster.Items;
										}
									}
									else
									{
										LocalizationList = localizationMaster.Items.ToDictionary((LocalizationData x) => x.ID);
									}
								}
								else
								{
									NovelMasterList = novelMaster.Items;
								}
							}
							else
							{
								SystemSeMasterList = systemSeMaster.SystemSes;
							}
						}
						else
						{
							AmbientSeMasterList = ambientSeMaster.AmbientSeSounds;
						}
					}
					else
					{
						AmbientMasterList = ambientMaster.AmbientSounds;
					}
				}
				else
				{
					DirectionMusicDataList = directionMusicMaster.AudioCollection;
				}
			}
			else
			{
				MusicDataList = musicMaster.AudioCollection;
			}
		}
		IsLoaded = true;
		loadTask.TrySetResult();
		Debug.LogDeveloperCheck("MasterData Finished Load");
	}

	public async UniTask LoadCompletionAsync(CancellationToken ct)
	{
		if (!IsLoaded)
		{
			await loadTask.Task.AttachExternalCancellation(ct);
		}
	}

	public void Dispose()
	{
		if (_scriptableMasterHandle.IsValid())
		{
			Addressables.Release(_scriptableMasterHandle);
		}
		_scriptableMasterHandle = default(AsyncOperationHandle<IList<ScriptableObject>>);
		MusicDataList = null;
		AmbientMasterList = null;
		AmbientSeMasterList = null;
		NovelMasterList = null;
		LocalizationList = null;
		ScenarioGroupMasterList = null;
		UnlockDecorationMasterList = null;
		UnlockEnvironmentMasterList = null;
		GamePomodoroTalkData = null;
		GamePomodoroVoiceData = null;
		GameEndTalkData = null;
		LevelUpInfoData = null;
		PointUpInfoData = null;
		AlterEgoData = null;
		BearsRestaurantData = null;
		Valentine2026Data = null;
		LunaNewYear2026Data = null;
		NearSpring2026Data = null;
		AmbientSEVolumeData = null;
		AllVolumeData = null;
		HeroineAIMasterData = null;
		DecorationMaster = null;
		EnvironmentMaster = null;
		loadTask.TrySetCanceled();
	}
}
