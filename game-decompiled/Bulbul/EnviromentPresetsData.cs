using System;
using GUPS.Obfuscator.Attribute;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class EnviromentPresetsData
{
	public EnviromentData[] Presets;

	public int SelectedIndex;

	public EnviromentPresetsData()
	{
		Presets = new EnviromentData[5];
		for (int i = 0; i < Presets.Length; i++)
		{
			Presets[i] = new EnviromentData();
		}
		SelectedIndex = 0;
	}
}
