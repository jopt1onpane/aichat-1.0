using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class LevelData
{
	[ES3Serializable]
	private int _currentLevel;

	[ES3Serializable]
	private float _currentExp;

	[ES3Serializable]
	private float _nextLevelNecessaryExp;

	public int CurrentLevel => _currentLevel;

	public float CurrentExp => _currentExp;

	public float NextLevelNecessaryExp => _nextLevelNecessaryExp;

	public LevelData()
	{
		_currentLevel = 1;
		_currentExp = 0f;
		_nextLevelNecessaryExp = 0f;
	}

	public LevelData(LevelData levelData)
	{
		_currentLevel = levelData._currentLevel;
		_currentExp = levelData._currentExp;
		_nextLevelNecessaryExp = levelData.NextLevelNecessaryExp;
	}

	public void AddExp(float exp)
	{
		_currentExp += exp;
	}

	public void LevelUp(MasterDataLoader masterData)
	{
		_currentLevel++;
		_currentExp -= _nextLevelNecessaryExp;
		_nextLevelNecessaryExp = CalculateNextLevelNecessaryExp(masterData);
	}

	public void SetNextLevelNecessaryExp(float nextLevelNecessaryExp)
	{
		_nextLevelNecessaryExp = nextLevelNecessaryExp;
	}

	public void SetupNextLevelNecessaryExp(MasterDataLoader masterData)
	{
		_nextLevelNecessaryExp = CalculateNextLevelNecessaryExp(masterData);
	}

	public float CalculateNextLevelNecessaryExp(MasterDataLoader masterData, int currentLevel = -1, SpecialService.CollaborationType levelType = SpecialService.CollaborationType.None)
	{
		return levelType switch
		{
			SpecialService.CollaborationType.None => CalculateMainLevel(), 
			SpecialService.CollaborationType.AlterEgo => CalculateAlterEgo(), 
			SpecialService.CollaborationType.BearsRestaurant => CalculateBearsRestaurant(), 
			SpecialService.CollaborationType.Valentine2026 => CalculateValentine(), 
			SpecialService.CollaborationType.LunaNewYear2026 => CalculateLunaNewYear(), 
			SpecialService.CollaborationType.NearSpring2026 => CalculateNearSpring(), 
			_ => 0f, 
		};
		float CalculateAlterEgo()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel >= 2)
			{
				return -1f;
			}
			return masterData.AlterEgoData.NextLevelNecessaryExp;
		}
		float CalculateBearsRestaurant()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel >= 2)
			{
				return -1f;
			}
			return masterData.BearsRestaurantData.NextLevelNecessaryExp;
		}
		float CalculateLunaNewYear()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel >= 2)
			{
				return -1f;
			}
			return masterData.LunaNewYear2026Data.NextLevelNecessaryExp;
		}
		float CalculateMainLevel()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel - 1 < masterData.LevelUpInfoData.NextLevelNecessaryExpArray.Length)
			{
				return masterData.LevelUpInfoData.NextLevelNecessaryExpArray[currentLevel - 1];
			}
			return masterData.LevelUpInfoData.NextLevelNecessaryExpBase;
		}
		float CalculateNearSpring()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel >= 2)
			{
				return -1f;
			}
			return masterData.NearSpring2026Data.NextLevelNecessaryExp;
		}
		float CalculateValentine()
		{
			if (currentLevel == -1)
			{
				currentLevel = CurrentLevel;
			}
			float num = 0f;
			if (currentLevel >= 2)
			{
				return -1f;
			}
			return masterData.Valentine2026Data.NextLevelNecessaryExp;
		}
	}

	public float CalculateTargetLevelNecessaryExp(MasterDataLoader masterData, int targetLevel, int beforeLevel = -1, bool isSubtractCurrentExp = true, SpecialService.CollaborationType levelType = SpecialService.CollaborationType.None)
	{
		switch (levelType)
		{
		case SpecialService.CollaborationType.None:
			return CalculateMainLevel();
		case SpecialService.CollaborationType.AlterEgo:
		case SpecialService.CollaborationType.BearsRestaurant:
		case SpecialService.CollaborationType.Valentine2026:
		case SpecialService.CollaborationType.LunaNewYear2026:
		case SpecialService.CollaborationType.NearSpring2026:
			return CalculateSpecial();
		default:
			return 0f;
		}
		float CalculateMainLevel()
		{
			float num = 0f;
			if (beforeLevel == -1)
			{
				beforeLevel = CurrentLevel;
			}
			for (int i = beforeLevel; i < targetLevel; i++)
			{
				num += CalculateNextLevelNecessaryExp(masterData, i, levelType);
			}
			if (isSubtractCurrentExp)
			{
				num -= CurrentExp;
			}
			return num;
		}
		float CalculateSpecial()
		{
			float num = 0f;
			if (beforeLevel == -1)
			{
				beforeLevel = CurrentLevel;
			}
			for (int i = beforeLevel; i < targetLevel; i++)
			{
				float num2 = CalculateNextLevelNecessaryExp(masterData, i, levelType);
				if (num2 < 0f)
				{
					if (num == 0f)
					{
						num = num2;
					}
					break;
				}
				num += num2;
			}
			if (isSubtractCurrentExp)
			{
				num -= CurrentExp;
			}
			return num;
		}
	}

	public static float CalculateTotalExp(MasterDataLoader masterData, SpecialService.CollaborationType levelType = SpecialService.CollaborationType.None)
	{
		return levelType switch
		{
			SpecialService.CollaborationType.None => Calculate(SaveDataManager.Instance.LevelData), 
			SpecialService.CollaborationType.AlterEgo => Calculate(SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData), 
			SpecialService.CollaborationType.BearsRestaurant => Calculate(SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData), 
			SpecialService.CollaborationType.Valentine2026 => Calculate(SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData), 
			SpecialService.CollaborationType.LunaNewYear2026 => Calculate(SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData), 
			SpecialService.CollaborationType.NearSpring2026 => Calculate(SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData), 
			_ => 0f, 
		};
		float Calculate(LevelData levelData)
		{
			int currentLevel = levelData.CurrentLevel;
			return levelData.CalculateTargetLevelNecessaryExp(masterData, currentLevel, 1, isSubtractCurrentExp: false, levelType) + levelData._currentExp;
		}
	}

	public static LevelData GetCurrentLevelData()
	{
		return SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value switch
		{
			SpecialService.CollaborationType.None => SaveDataManager.Instance.LevelData, 
			SpecialService.CollaborationType.AlterEgo => SaveDataManager.Instance.CollaborationSaveData.AlterEgoData.LevelData, 
			SpecialService.CollaborationType.BearsRestaurant => SaveDataManager.Instance.CollaborationSaveData.BearsRestaurantData.LevelData, 
			SpecialService.CollaborationType.Valentine2026 => SaveDataManager.Instance.CollaborationSaveData.Valentine2026Data.LevelData, 
			SpecialService.CollaborationType.LunaNewYear2026 => SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LevelData, 
			SpecialService.CollaborationType.NearSpring2026 => SaveDataManager.Instance.CollaborationSaveData.NearSpring2026Data.LevelData, 
			_ => null, 
		};
	}

	public void SetLevel(int level)
	{
		_currentLevel = level;
	}
}
