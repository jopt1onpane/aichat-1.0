using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class MusicData
{
	public bool IsManualUnlock;

	public string Title;

	public string Credit;

	public AudioTag Tag;

	public AudioClip AudioClip;

	public string UUID;

	public string overwriteTitleForMobile;

	public string overwriteCreditForMobile;

	public bool IsUnlocked { get; set; }

	public string AudioClipName
	{
		get
		{
			if (!(AudioClip == null))
			{
				return AudioClip.name;
			}
			return "";
		}
	}

	public void SetUUID()
	{
		if (string.IsNullOrEmpty(UUID))
		{
			UUID = Guid.NewGuid().ToString();
		}
	}

	private bool IsUUIDEmpty()
	{
		return string.IsNullOrEmpty(UUID);
	}
}
