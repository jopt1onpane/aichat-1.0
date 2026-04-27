namespace Bulbul;

public interface IPresetDataService
{
	void LoadPreset(int index);

	void SaveCurrentToPreset(int index, bool alsoSetSelectedIndex);

	int GetCurrentPresetIndex();

	bool HasDifferenceFromPreset(int index);
}
