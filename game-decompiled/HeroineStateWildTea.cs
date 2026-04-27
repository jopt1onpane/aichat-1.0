using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;

public class HeroineStateWildTea : HeroineBaseState
{
	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild001_Motion3_Tea);
		base.Owner.HeroineService.HeroineUseObjectController.Drinks.Drink();
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		base.Owner.HeroineService.HeroineUseObjectController.DeactivateCop();
	}

	public override bool IsPossibleClickReaction()
	{
		return false;
	}

	public override bool IsPossibleTalk()
	{
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
		await WaitWithTimeout(() => base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.Wild001_Motion3_Tea.ToString()), 20f, token);
	}

	private async UniTask WaitWithTimeout(Func<bool> condition, float timeoutSeconds, CancellationToken token)
	{
		UniTask uniTask = UniTask.WaitUntil(condition, PlayerLoopTiming.Update, token);
		UniTask uniTask2 = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
		await UniTask.WhenAny(uniTask, uniTask2);
	}
}
