namespace Bulbul.Mobile;

public class EnvironmentVolumeItemModel
{
	public EnvironmentType EnvironmentType { get; init; }

	public string NameLocalizeID { get; init; }

	public bool IsSoundActive => Volume > 0f;

	public float Volume { get; set; }

	public bool IsNew { get; set; }
}
