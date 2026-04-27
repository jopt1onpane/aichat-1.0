namespace Bulbul.MasterData;

public static class ScenarioTypeExtensions
{
	public static bool IsSpecial(this ScenarioType value)
	{
		if (value >= ScenarioType.Special_AlterEgo)
		{
			return value < ScenarioType.Event_2025_Christmas_GameStart;
		}
		return false;
	}

	public static bool IsLongStory(this ScenarioType value)
	{
		if (value != ScenarioType.MainScenario && value != ScenarioType.AfterScenario && value != ScenarioType.DLCScenario && value != ScenarioType.ExtraScenario && value != ScenarioType.Special_AlterEgo && value != ScenarioType.Special_BearsRestaurant && value != ScenarioType.Special_Valentine2026 && value != ScenarioType.Special_LunaNewYear2026)
		{
			return value == ScenarioType.Special_NearSpring2026;
		}
		return true;
	}

	public static bool IsLongStoryOrTutorial(this ScenarioType value)
	{
		if (!value.IsLongStory())
		{
			return value == ScenarioType.Tutorial;
		}
		return true;
	}
}
