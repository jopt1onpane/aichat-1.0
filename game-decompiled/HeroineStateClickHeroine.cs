using MyUtil;

public class HeroineStateClickHeroine : HeroineBaseState
{
	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
	}

	public override bool IsPossibleClickReaction()
	{
		return true;
	}

	public override bool IsPossibleTalk()
	{
		return true;
	}

	public override void ToPossibleClickReaction()
	{
	}
}
