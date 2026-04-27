namespace Bulbul;

public interface IMusicImportFailedView
{
	void Setup(MusicService musicService);

	void ShowImportImpossibleLimitAnnounce();

	void ShowImportFailedAnnounce(bool isLimitFailed, bool isImportedFailed, bool isInvalidFailed, bool isFolder);
}
