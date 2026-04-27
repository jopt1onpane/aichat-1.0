using System;
using System.Globalization;
using Bulbul;
using Bulbul.MasterData;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSelectCellLunaNewYear2026 : SpecialSelectCell
{
	[SerializeField]
	private TextLocalizationBehaviour endDateText;

	[SerializeField]
	private RectTransform timerParent;

	[SerializeField]
	private Image limitedBg;

	public override void Setup(SpecialService.CollaborationType specialType, Observable<ScenarioType> onEndStory)
	{
		_episodeCountMax = 1;
		_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber;
		base.Setup(specialType, onEndStory);
		onEndStory.Subscribe(delegate(ScenarioType scenarioType)
		{
			if (scenarioType == ScenarioType.Special_LunaNewYear2026)
			{
				_readEpisodeCount.Value = SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber;
			}
		}).AddTo(this);
		bool flag = SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber == 1;
		timerParent.gameObject.SetActive(!flag);
		limitedBg.gameObject.SetActive(!flag);
		if (!flag)
		{
			this.UpdateAsObservable().Subscribe(this, delegate(Unit _, SpecialSelectCellLunaNewYear2026 @this)
			{
				@this.RemainTextUpdate();
			}).AddTo(this);
		}
	}

	private void RemainTextUpdate()
	{
		if (SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.LastReadEpisodeNumber == 1)
		{
			timerParent.gameObject.SetActive(value: false);
			limitedBg.gameObject.SetActive(value: false);
			return;
		}
		DateTime now = DateTime.Now;
		DateTime specialLunaNewYear2026UnlockEndDate = MyDefine.SpecialLunaNewYear2026UnlockEndDate;
		TimeSpan timeSpan = specialLunaNewYear2026UnlockEndDate - now;
		if (now >= specialLunaNewYear2026UnlockEndDate)
		{
			endDateText.Text.SetText("00:00:00:00");
			return;
		}
		string sourceText = timeSpan.ToString("dd\\:hh\\:mm\\:ss", CultureInfo.InvariantCulture);
		endDateText.Text.SetText(sourceText);
	}
}
