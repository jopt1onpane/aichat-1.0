using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class AmbientSeSoundMasterData
{
	public AmbientSeType AmbientSeSound;

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
