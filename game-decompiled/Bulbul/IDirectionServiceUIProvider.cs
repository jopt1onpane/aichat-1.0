using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public interface IDirectionServiceUIProvider
{
	Animator ScreenSharing { get; }

	Canvas UICanvas { get; }

	RawImage ScreenStopRawImage { get; }

	GameObject ConnectionLost { get; }
}
