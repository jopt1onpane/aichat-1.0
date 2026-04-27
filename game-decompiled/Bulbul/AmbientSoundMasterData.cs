using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class AmbientSoundMasterData
{
	public AmbientSoundType AmbientSound;

	public AudioClip AudioClip;

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
}
