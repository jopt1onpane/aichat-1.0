using System;

namespace Bulbul;

public interface IPlatformRoot
{
	void Setup(Action TutorialSkip);

	void UpdatePlatform();

	void UpdateMusicOnly();
}
