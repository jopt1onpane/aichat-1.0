using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bulbul.Mobile;

public interface IRemovableItemView
{
	CancellationToken CancellationToken { get; }

	UniTask Play(ListItemViewAnimations.RemoveAnimationDirection direction = ListItemViewAnimations.RemoveAnimationDirection.Left, CancellationToken token = default(CancellationToken));
}
