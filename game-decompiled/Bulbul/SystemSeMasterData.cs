using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class SystemSeMasterData
{
	public SystemSeType SeSound;

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
