using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;

public class HeroineStateWildCloseWindow : HeroineBaseState
{
	public override void OnRegisterToStateMachine()
	{
	}

	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.Wild001_Motion9_CloseWindow);
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
		return false;
	}

	public override void ToPossibleClickReaction()
	{
	}

	public override async UniTask ReadyFinishState(CancellationToken token)
	{
		await AsyncUtil.WaitUntilWithTimeout(() => base.Owner.HeroineService.IsEndAnimation(HeroineService.AnimationType.Wild001_Motion9_CloseWindow.ToString()), 20f, token);
	}
}
