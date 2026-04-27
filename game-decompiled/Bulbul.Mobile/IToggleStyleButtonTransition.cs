namespace Bulbul.Mobile;

public interface IToggleStyleButtonTransition
{
	bool IsTrantioning { get; }

	void Transition(bool isOn, bool isImmediate = false);
}
