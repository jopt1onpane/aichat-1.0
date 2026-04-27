using System;
using System.Collections.Generic;

namespace MyUtil;

public class StateMachineMonobehavior<TOwner>
{
	public sealed class AnyState : StateMonobehavior<TOwner>
	{
	}

	private LinkedList<StateMonobehavior<TOwner>> states = new LinkedList<StateMonobehavior<TOwner>>();

	private int _currentStateKey;

	private int _beforeStateKey;

	public TOwner Owner { get; }

	public StateMonobehavior<TOwner> CurrentState { get; private set; }

	public int CurrentStateKey => _currentStateKey;

	public int BeforeStateKey => _beforeStateKey;

	private void Change(StateMonobehavior<TOwner> nextState)
	{
		CurrentState.Exit(nextState);
		nextState.Enter(CurrentState);
		CurrentState = nextState;
	}

	public StateMachineMonobehavior(TOwner owner)
	{
		Owner = owner;
		_currentStateKey = -1;
		_beforeStateKey = -1;
	}

	public T Add<T>() where T : StateMonobehavior<TOwner>, new()
	{
		T val = new T();
		val.SetStateMachine(this);
		states.AddLast(val);
		val.OnRegisterToStateMachine();
		return val;
	}

	public void Add<T>(T addState) where T : StateMonobehavior<TOwner>, new()
	{
		addState.SetStateMachine(this);
		states.AddLast(addState);
		addState.OnRegisterToStateMachine();
	}

	public T GetOrAddState<T>() where T : StateMonobehavior<TOwner>, new()
	{
		foreach (StateMonobehavior<TOwner> state in states)
		{
			if (state is T result)
			{
				return result;
			}
		}
		return Add<T>();
	}

	public void AddTransition<TFrom, TTo>(int eventId) where TFrom : StateMonobehavior<TOwner>, new() where TTo : StateMonobehavior<TOwner>, new()
	{
		TFrom orAddState = GetOrAddState<TFrom>();
		if (orAddState.transitions.ContainsKey(eventId))
		{
			throw new ArgumentException("ステート'TFrom'に対してイベントID'" + eventId + "'の遷移は定義済です");
		}
		TTo orAddState2 = GetOrAddState<TTo>();
		orAddState.transitions.Add(eventId, orAddState2);
	}

	public void AddAnyTransition<TTo>(int eventId) where TTo : StateMonobehavior<TOwner>, new()
	{
		AddTransition<AnyState, TTo>(eventId);
	}

	public void Start<TFirst>() where TFirst : StateMonobehavior<TOwner>, new()
	{
		Start(GetOrAddState<TFirst>());
	}

	public void Start(StateMonobehavior<TOwner> firstState)
	{
		CurrentState = firstState;
		CurrentState.Enter(null);
	}

	public void Update()
	{
		CurrentState.ManualUpdate();
	}

	public void Dispatch(int eventId)
	{
		if (CurrentState.transitions.TryGetValue(eventId, out var value) || GetOrAddState<AnyState>().transitions.TryGetValue(eventId, out value))
		{
			_beforeStateKey = _currentStateKey;
			_currentStateKey = eventId;
			Change(value);
		}
	}
}
