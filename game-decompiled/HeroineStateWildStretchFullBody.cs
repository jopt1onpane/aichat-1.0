using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;

public class HeroineStateWildStretchFullBody : HeroineBaseState
{
	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild001_Motion1_StretchFllBody);
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
	}

	public override bool IsPossibleClickReaction()
	{
		return false;
	}

	public override bool IsPossibleTalk()
	{
		return true;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
		await WaitWithTimeout(() => base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.Wild001_Motion1_StretchFllBody.ToString()), 20f, token);
	}

	private async UniTask WaitWithTimeout(Func<bool> condition, float timeoutSeconds, CancellationToken token)
	{
		UniTask uniTask = UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
		await UniTask.WhenAny(uniTask, uniTask2);
	}
}
