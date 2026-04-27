using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineFacialController
{
	public enum StateType
	{
		None = -1,
		Story,
		Normal
	}

	private const string FacialKey = "Facial";

	private Animator _heroineAnimater;

	private StateMachine<HeroineFacialController> _stateMachine;

	private CancellationTokenSource _cts;

	public void Setup(Animator animator)
	{
		_heroineAnimater = animator;
		_stateMachine = new StateMachine<HeroineFacialController>(this);
		_stateMachine.Add<FacialStateScenario>();
		_stateMachine.AddAnyTransition<FacialStateScenario>(0);
		_stateMachine.Add<FacialStateNormal>();
		_stateMachine.AddAnyTransition<FacialStateNormal>(1);
		_stateMachine.Start<FacialStateScenario>();
	}

	public void Update()
	{
	}

	public void OnStartStory()
	{
		_stateMachine.Dispatch(0);
	}

	public void OnFinishStory()
	{
		_stateMachine.Dispatch(1);
	}

	public void OnFinishTutorial()
	{
		_stateMachine.Dispatch(1);
	}

	public void OnStartHeroineClickReaction()
	{
		_stateMachine.Dispatch(0);
	}

	public void OnFinishHeroineClickReaction()
	{
		_stateMachine.Dispatch(1);
	}

	public void ChangeFacial(int facialType)
	{
		if (facialType != -1)
		{
			_cts?.Cancel();
			_heroineAnimater.SetInteger("Facial", facialType);
		}
	}

	public void InitFacial()
	{
		_cts?.Cancel();
		ChangeFacial(0);
	}

	public async UniTaskVoid InitFacialAfterDelay(float delaySeconds)
	{
		_cts?.Cancel();
		_cts = null;
		_cts = new CancellationTokenSource();
		if (!(await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), ignoreTimeScale: false, PlayerLoopTiming.Update, _cts.Token).SuppressCancellationThrow()))
		{
			ChangeFacial(0);
		}
	}
}
