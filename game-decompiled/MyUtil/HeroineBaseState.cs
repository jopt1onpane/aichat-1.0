using System.Threading;
using Cysharp.Threading.Tasks;

namespace MyUtil;

public abstract class HeroineBaseState : StateMonobehavior<HeroineAI>
{
	public abstract bool IsPossibleClickReaction();

	public abstract bool IsPossibleTalk();

	public abstract void ToPossibleClickReaction();

	public virtual UniTask ReadyFinishState(CancellationToken token)
	{
		return UniTask.CompletedTask;
	}
}
