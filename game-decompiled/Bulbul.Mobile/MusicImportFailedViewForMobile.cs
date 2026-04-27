using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class MusicImportFailedViewForMobile : MonoBehaviour, IMusicImportFailedView
{
	private DirectionService directionService;

	[Inject]
	public void Construct(DirectionService directionService)
	{
		this.directionService = directionService;
	}

	void IMusicImportFailedView.Setup(MusicService musicService)
	{
	}

	void IMusicImportFailedView.ShowImportFailedAnnounce(bool isLimitFailed, bool isImportedFailed, bool isInvalidFailed, bool isFolder)
	{
		if (isLimitFailed)
		{
			directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedLimit);
		}
		else if (isInvalidFailed)
		{
			if (isFolder)
			{
				directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedInvildFolder);
			}
			else
			{
				directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedInvildFile);
			}
		}
		else if (isImportedFailed)
		{
			directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedImportedFolder);
		}
	}

	void IMusicImportFailedView.ShowImportImpossibleLimitAnnounce()
	{
		directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportImpossibleLimit);
	}
}
