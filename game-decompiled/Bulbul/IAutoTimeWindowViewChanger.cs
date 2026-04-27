namespace Bulbul;

public interface IAutoTimeWindowViewChanger
{
	void SetPossibleUseAutoChanger(bool possible);

	void ApplyTimeOfDayFromCurrentTime();
}
