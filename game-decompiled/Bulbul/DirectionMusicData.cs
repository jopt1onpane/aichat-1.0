using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class DirectionMusicData
{
	public string Title;

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

	public MusicData ConvertMusicData()
	{
		return new MusicData
		{
			Title = Title,
			AudioClip = AudioClip
		};
	}
}
