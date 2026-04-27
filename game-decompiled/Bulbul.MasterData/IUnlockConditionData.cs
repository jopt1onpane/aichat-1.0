namespace Bulbul.MasterData;

public interface IUnlockConditionData
{
	string ConditionsType { get; }

	bool IsMobileDemoUserLockedTarget { get; }

	string Arg1 { get; }

	string Arg2 { get; }

	string Arg3 { get; }
}
