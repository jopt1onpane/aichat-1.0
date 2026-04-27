using Bulbul.MasterData;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class StoryCellUI : MonoBehaviour
{
	[SerializeField]
	private ButtonEventObservable subject;

	[SerializeField]
	private TextLocalizationBehaviour title;

	[SerializeField]
	private TextLocalizationBehaviour subTitle;

	[SerializeField]
	private Image newMark;

	[SerializeField]
	private Image scenarioIcon;

	[SerializeField]
	private Sprite alterEgoIcon;

	[SerializeField]
	private Sprite bearsRestaurantIcon;

	[SerializeField]
	private Sprite valentine2026Icon;

	[SerializeField]
	private Sprite lunaNewYear2026Icon;

	[SerializeField]
	private Sprite nearSpring2026Icon;

	private ScenarioGroupData scenarioGroupMaster;

	public Observable<ScenarioGroupData> OnSubmit => subject.OnClick.Select((Unit _) => scenarioGroupMaster);

	public virtual void Setup(ScenarioGroupData master)
	{
		scenarioGroupMaster = master;
		title.Set(master.TitleLocalizationID);
		subTitle.Set(master.SubtitleLocalizationID);
		if (master.Scenario != ScenarioType.MainScenario)
		{
			if (master.Scenario == ScenarioType.Special_AlterEgo)
			{
				scenarioIcon.sprite = alterEgoIcon;
			}
			if (master.Scenario == ScenarioType.Special_BearsRestaurant)
			{
				scenarioIcon.sprite = bearsRestaurantIcon;
			}
			if (master.Scenario == ScenarioType.Special_Valentine2026)
			{
				scenarioIcon.sprite = valentine2026Icon;
			}
			if (master.Scenario == ScenarioType.Special_LunaNewYear2026)
			{
				scenarioIcon.sprite = lunaNewYear2026Icon;
			}
			if (master.Scenario == ScenarioType.Special_NearSpring2026)
			{
				scenarioIcon.sprite = nearSpring2026Icon;
			}
		}
	}
}
