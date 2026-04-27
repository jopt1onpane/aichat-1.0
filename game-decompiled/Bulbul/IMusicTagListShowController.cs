namespace Bulbul;

public interface IMusicTagListShowController : IMusicUIBase
{
	void ToggleMusicTagList();

	void OpenMusicTagList();

	void CloseMusicTagList(bool immediate = false);
}
