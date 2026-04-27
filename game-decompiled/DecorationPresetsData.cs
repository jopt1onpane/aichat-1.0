using System;
using GUPS.Obfuscator.Attribute;

[Serializable]
[DoNotObfuscateClass]
public class DecorationPresetsData
{
	public DecorationsData[] Presets;

	public int SelectedIndex;

	public DecorationPresetsData()
	{
		Presets = new DecorationsData[5];
		for (int i = 0; i < Presets.Length; i++)
		{
			Presets[i] = new DecorationsData();
		}
		SelectedIndex = 0;
	}

	public void LoadSetup()
	{
		DecorationsData[] presets = Presets;
		for (int i = 0; i < presets.Length; i++)
		{
			presets[i].LoadSetup();
		}
	}

	public void SaveReady()
	{
		DecorationsData[] presets = Presets;
		for (int i = 0; i < presets.Length; i++)
		{
			presets[i].SaveReady();
		}
	}
}
