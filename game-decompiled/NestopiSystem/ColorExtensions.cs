using UnityEngine;

namespace NestopiSystem;

public static class ColorExtensions
{
	public static Color WithA(this Color col, float a)
	{
		return new Color(col.r, col.g, col.b, a);
	}
}
