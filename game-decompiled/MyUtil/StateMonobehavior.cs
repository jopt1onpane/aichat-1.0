using System.Collections.Generic;
using UnityEngine;

namespace MyUtil;

public abstract class StateMonobehavior<TOwner> : MonoBehaviour
{
	protected StateMachineMonobehavior<TOwner> stateMachine;

	public Dictionary<int, StateMonobehavior<TOwner>> transitions = new Dictionary<int, StateMonobehavior<TOwner>>();

	protected TOwner Owner => stateMachine.Owner;

	protected virtual void OnEnter(StateMonobehavior<TOwner> prevState)
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void SelectNextState()
	{
	}

	protected virtual void OnExit(StateMonobehavior<TOwner> nextState)
	{
	}

	public void SetStateMachine(StateMachineMonobehavior<TOwner> stateMachine)
	{
		this.stateMachine = stateMachine;
	}

	public virtual void OnRegisterToStateMachine()
	{
	}

	public void Enter(StateMonobehavior<TOwner> prevState)
	{
		OnEnter(prevState);
	}

	public void ManualUpdate()
	{
		OnUpdate();
	}

	public void Exit(StateMonobehavior<TOwner> nextState)
	{
		OnExit(nextState);
	}
}
