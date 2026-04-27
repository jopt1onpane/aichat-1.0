using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityPlayerLevel : MonoBehaviour, IPlayerLevelDirectionState
{
	private enum MainState
	{
		Idle,
		LevelUpDirectionStartReady,
		LevelUpDirecting,
		LevelUpDirectionEnd
	}

	private MainState _mainState;

	[Inject]
	private MasterDataLoader _masterDataLoader;

	[Inject]
	private PlayerLevelService _playerLevelService;

	[Inject]
	private ISpecialService _specialService;

	[Inject]
	private IPlayerLevelUIService _playerLevelUI;

	private float _notAddedYetShowExp;

	private bool _isCurrentLevelUp;

	bool IPlayerLevelDirectionState.IsCurrentLevelUpDirection => _isCurrentLevelUp;

	private LevelData LevelSaveData()
	{
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			return SaveDataManager.Instance.PlayerData.LevelData;
		}
		return _specialService.GetCurrentLevelData();
	}

	public void Setup()
	{
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None && LevelSaveData().NextLevelNecessaryExp <= 0f)
		{
			LevelSaveData().SetupNextLevelNecessaryExp(_masterDataLoader);
		}
		_playerLevelUI.Setup();
		_playerLevelService.OnAddExp.Subscribe(delegate(float x)
		{
			AddExp(x);
		}).AddTo(this);
	}

	public void UpdateFacility()
	{
		switch (_mainState)
		{
		case MainState.Idle:
			_playerLevelUI.UpdateUI();
			if (_playerLevelUI.IsAccumulatedLevelUpExp())
			{
				_mainState = MainState.LevelUpDirectionStartReady;
			}
			break;
		case MainState.LevelUpDirecting:
			if (_playerLevelUI.IsEndLevelUpDirection())
			{
				_mainState = MainState.LevelUpDirectionEnd;
			}
			break;
		case MainState.LevelUpDirectionStartReady:
		case MainState.LevelUpDirectionEnd:
			break;
		}
	}

	public bool IsReadyStartLevelUpDirection()
	{
		return _mainState == MainState.LevelUpDirectionStartReady;
	}

	public void StartLevelUpDirection()
	{
		_playerLevelUI.StartLevelUpDirection();
		_mainState = MainState.LevelUpDirecting;
	}

	public bool IsLevelUpDirecting()
	{
		if (_mainState == MainState.Idle)
		{
			return LevelSaveData().CurrentLevel != int.Parse(_playerLevelUI.CurrentShowLevel);
		}
		return true;
	}

	public bool IsEndLevelUpDirection()
	{
		return _mainState == MainState.LevelUpDirectionEnd;
	}

	public void EndLevelUpDirection()
	{
		_playerLevelUI.EndLevelUpDirection();
		_isCurrentLevelUp = false;
		_mainState = MainState.Idle;
	}

	public void AddExp(float exp)
	{
		LevelData levelData = LevelSaveData();
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			if (SaveDataManager.Instance.ScenarioProgressData.FinishReadMainEpisodeNumber < 31f)
			{
				float num = levelData.CalculateTargetLevelNecessaryExp(_masterDataLoader, 32);
				if (exp > num)
				{
					exp = num;
				}
			}
		}
		else
		{
			exp = _specialService.AdjustUpperExp(exp);
		}
		if (exp < 0f)
		{
			exp = 0f;
		}
		int currentLevel = levelData.CurrentLevel;
		levelData.AddExp(exp);
		_playerLevelService.AddedExp();
		_playerLevelUI.OnAddExp(exp);
		while (levelData.CurrentExp >= levelData.NextLevelNecessaryExp)
		{
			LevelUp();
		}
		if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value == SpecialService.CollaborationType.None)
		{
			SaveDataManager.Instance.SavePlayerData();
		}
		else
		{
			SaveDataManager.Instance.SaveCollaborationData();
		}
		if (currentLevel != levelData.CurrentLevel)
		{
			_isCurrentLevelUp = true;
		}
		void LevelUp()
		{
			LevelData levelData2 = LevelSaveData();
			if (levelData2.CurrentExp >= levelData2.NextLevelNecessaryExp)
			{
				levelData2.LevelUp(_masterDataLoader);
				SaveDataManager.Instance.ScenarioProgressData.UpdateNextMainEpisode(_masterDataLoader);
			}
		}
	}

	public bool TryGetUICurrentLevel(out int level)
	{
		return int.TryParse(_playerLevelUI.CurrentShowLevel, out level);
	}

	public void RefreshUI()
	{
		_playerLevelUI.SyncWithSaveData();
	}
}
