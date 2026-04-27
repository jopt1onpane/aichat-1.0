using Bulbul;
using Bulbul.MasterData;

public class GameStartTalkSelector
{
	public int GetGameStartTalk(ScenarioType scenarioType)
	{
		int num = 1;
		switch (scenarioType)
		{
		case ScenarioType.GameStart_First_CameraTouch:
			num = 1;
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchTalkList.GetNext();
			if (num >= 13)
			{
				num = 1;
			}
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Morning:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchMorningTalkList.GetNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Noon:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchNoonTalkList.GetNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Evening:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchEveningTalkList.GetNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Night:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchNightTalkList.GetNext();
			break;
		case ScenarioType.GameStart_LessTowDays:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysTalkList.GetNext();
			break;
		case ScenarioType.GameStart_LessHarfMonth_CameraTouch:
			num = SaveDataManager.Instance.HeroineData.StartDirection.LessHarfMonthTalkList.GetNext();
			break;
		case ScenarioType.GameStart_GreaterHarfMonth_CameraTouch:
			num = SaveDataManager.Instance.HeroineData.StartDirection.GreaterMonthTalkList.GetNext();
			break;
		case ScenarioType.GameStart_GreaterMonth_CameraTouch:
			num = 1;
			break;
		case ScenarioType.GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch:
			num = 1;
			break;
		default:
			num = 1;
			break;
		}
		return num;
	}

	public void UseGameStartTalk(ScenarioType scenarioType)
	{
		switch (scenarioType)
		{
		case ScenarioType.GameStart_LessTowDays_CameraTouch:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Morning:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchMorningTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Noon:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchNoonTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Evening:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchEveningTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessTowDays_CameraTouch_Night:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysCameraTouchNightTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessTowDays:
			SaveDataManager.Instance.HeroineData.StartDirection.LessTowDaysTalkList.UseNext();
			break;
		case ScenarioType.GameStart_LessHarfMonth_CameraTouch:
			SaveDataManager.Instance.HeroineData.StartDirection.LessHarfMonthTalkList.UseNext();
			break;
		case ScenarioType.GameStart_GreaterHarfMonth_CameraTouch:
			SaveDataManager.Instance.HeroineData.StartDirection.GreaterMonthTalkList.UseNext();
			break;
		}
	}
}
