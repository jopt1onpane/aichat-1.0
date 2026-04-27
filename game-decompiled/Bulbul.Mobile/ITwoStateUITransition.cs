namespace Bulbul.Mobile;

public interface ITwoStateUITransition
{
	bool IsTransitioning { get; }

	void Transition(bool isStateA);

	void TransitionImmediate(bool isStateA);
}
