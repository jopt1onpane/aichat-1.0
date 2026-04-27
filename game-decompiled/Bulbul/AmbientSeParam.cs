namespace Bulbul;

public struct AmbientSeParam(AmbientSeType ambientType, bool isAllowsDuplicate, float volumeRate = 1f)
{
	public AmbientSeType AmbientSeSound = ambientType;

	public bool IsAllowsDuplicate = isAllowsDuplicate;

	public float VolumeRate = volumeRate;
}
