using UnityEngine;
using VContainer;

namespace Bulbul;

public class MusicImportFailedViewPC : MonoBehaviour, IMusicImportFailedView
{
	[Inject]
	private DirectionService _directionService;

	[SerializeField]
	private GameObject _importLimitTooltipTarget;

	public void Setup(MusicService musicService)
	{
		TooltipTarget tooltipTarget = _importLimitTooltipTarget.AddComponent<TooltipTarget>();
		tooltipTarget.SetContentLocalizeID("ui_tips_import_limit_count");
		tooltipTarget.SetLocalizeConverter(new ImportLimitCountLocalizeConverter(musicService));
	}

	public void ShowImportImpossibleLimitAnnounce()
	{
		_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportImpossibleLimit);
	}

	public void ShowImportFailedAnnounce(bool isLimitFailed, bool isImportedFailed, bool isInvalidFailed, bool isFolder)
	{
		if (isLimitFailed)
		{
			_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedLimit);
		}
		else if (isInvalidFailed)
		{
			if (isFolder)
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedInvildFolder);
			}
			else
			{
				_directionService.SlideFadeAnnounce.Play(SlideFadeAnnounceDirection.AnnounceType.ImportFailedInvildFile);
			}
		}
		else if (isImportedFailed)
		{
			SlideFadeAnnounceDirection.AnnounceType type = (isFolder ? SlideFadeAnnounceDirection.AnnounceType.ImportFailedImportedFolder : SlideFadeAnnounceDirection.AnnounceType.ImportFailedImportedFile);
			_directionService.SlideFadeAnnounce.Play(type);
		}
	}
}
