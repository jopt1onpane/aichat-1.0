using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul.Mobile;

public class PoorConnectionViewAnimationRestarterForMobile : MonoBehaviour
{
	[SerializeField]
	private WallPaperManagerForMobile _wallpaperManager;

	[SerializeField]
	private PoorConnectionView _view;

	[SerializeField]
	private PoorConnectionViewAnimation _animation;

	private void Start()
	{
		_wallpaperManager.OnChangedState.Subscribe(this, delegate(bool isWallpaper, PoorConnectionViewAnimationRestarterForMobile @this)
		{
			if (!isWallpaper && @this._view.IsActive)
			{
				@this._animation.PlayActivationAnimation().Forget();
			}
		}).AddTo(this);
	}
}
