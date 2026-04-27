using System;

namespace Bulbul.MasterData;

[Serializable]
public class UnlockDecorationData : IUnlockConditionData
{
	public string ItemType;

	public bool IsMobileDemoUserLockedTarget;

	public string ConditionsType;

	public string Arg1;

	public string Arg2;

	public string Arg3;

	string IUnlockConditionData.ConditionsType => ConditionsType;

	bool IUnlockConditionData.IsMobileDemoUserLockedTarget => IsMobileDemoUserLockedTarget;

	string IUnlockConditionData.Arg1 => Arg1;

	string IUnlockConditionData.Arg2 => Arg2;

	string IUnlockConditionData.Arg3 => Arg3;
}
