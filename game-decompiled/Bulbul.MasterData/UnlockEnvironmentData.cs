using System;
using System.Globalization;

namespace Bulbul.MasterData;

[Serializable]
public class UnlockEnvironmentData : IUnlockConditionData
{
	public string ItemType;

	public bool IsMobileDemoUserLockedTarget;

	public string ConditionsType;

	public string UnlockExpire;

	public string Arg1;

	public string Arg2;

	public string Arg3;

	string IUnlockConditionData.ConditionsType => ConditionsType;

	bool IUnlockConditionData.IsMobileDemoUserLockedTarget => IsMobileDemoUserLockedTarget;

	string IUnlockConditionData.Arg1 => Arg1;

	string IUnlockConditionData.Arg2 => Arg2;

	string IUnlockConditionData.Arg3 => Arg3;

	public bool TryGetUnlockExpire(out DateTime dateTime)
	{
		return DateTime.TryParseExact(UnlockExpire, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
	}
}
