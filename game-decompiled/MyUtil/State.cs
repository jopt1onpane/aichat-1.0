using System.Collections.Generic;

namespace MyUtil;

public abstract class State<TOwner>
{
	protected StateMachine<TOwner> stateMachine;

	public Dictionary<int, State<TOwner>> transitions = new Dictionary<int, State<TOwner>>();

	protected TOwner Owner => stateMachine.Owner;

	protected virtual void OnEnter(State<TOwner> prevState)
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void SelectNextState()
	{
	}

	protected virtual void OnExit(State<TOwner> nextState)
	{
	}

	public void SetStateMachine(StateMachine<TOwner> stateMachine)
	{
		this.stateMachine = stateMachine;
	}

	public virtual void OnRegisterToStateMachine()
	{
	}

	public void Enter(State<TOwner> prevState)
	{
		OnEnter(prevState);
	}

	public void ManualUpdate()
	{
		OnUpdate();
	}

	public void Exit(State<TOwner> nextState)
	{
		OnExit(nextState);
	}
}
