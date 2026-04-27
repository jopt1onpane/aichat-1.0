using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class StartDirectionData
{
	[ES3Serializable]
	private RandomList _lessTowDaysCameraTouchTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessTowDaysTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessHarfMonthTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _greaterMonthTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessTowDaysCameraTouchMorningTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessTowDaysCameraTouchNoonTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessTowDaysCameraTouchEveningTalkList = new RandomList();

	[ES3Serializable]
	private RandomList _lessTowDaysCameraTouchNightTalkList = new RandomList();

	public RandomList LessTowDaysCameraTouchTalkList => _lessTowDaysCameraTouchTalkList;

	public RandomList LessTowDaysTalkList => _lessTowDaysTalkList;

	public RandomList LessHarfMonthTalkList => _lessHarfMonthTalkList;

	public RandomList GreaterMonthTalkList => _greaterMonthTalkList;

	public RandomList LessTowDaysCameraTouchMorningTalkList => _lessTowDaysCameraTouchMorningTalkList;

	public RandomList LessTowDaysCameraTouchNoonTalkList => _lessTowDaysCameraTouchNoonTalkList;

	public RandomList LessTowDaysCameraTouchEveningTalkList => _lessTowDaysCameraTouchEveningTalkList;

	public RandomList LessTowDaysCameraTouchNightTalkList => _lessTowDaysCameraTouchNightTalkList;

	public void Setup()
	{
		Action onEndUseNext = delegate
		{
			SaveDataManager.Instance.SaveHeroineData();
		};
		_lessTowDaysCameraTouchTalkList.Setup(1, 12, onEndUseNext);
		_lessTowDaysCameraTouchMorningTalkList.Setup(1, 9, onEndUseNext);
		_lessTowDaysCameraTouchNoonTalkList.Setup(1, 4, onEndUseNext);
		_lessTowDaysCameraTouchEveningTalkList.Setup(1, 5, onEndUseNext);
		_lessTowDaysCameraTouchNightTalkList.Setup(1, 7, onEndUseNext);
		_lessTowDaysTalkList.Setup(1, 17, onEndUseNext);
		_lessHarfMonthTalkList.Setup(1, 8, onEndUseNext);
		_greaterMonthTalkList.Setup(1, 2, onEndUseNext);
	}
}
