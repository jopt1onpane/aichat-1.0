using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using MyUtil;
using UnityEngine;

public class HeroineStateBreakListenMusic : HeroineBaseState
{
	protected override void OnEnter(StateMonobehavior<HeroineAI> prevState)
	{
		if (Random.Range(1, 101) <= 50)
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase003_ListenMusicLow);
		}
		else
		{
			base.Owner.HeroineService.ChangeAnimation(HeroineService.AnimationType.BreakBase003_ListenMusicHigh);
		}
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnExit(StateMonobehavior<HeroineAI> nextState)
	{
		base.Owner.WantChangeBreakAction();
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

	public override UniTask ReadyFinishState(CancellationToken token)
	{
		return UniTask.CompletedTask;
	}
}
