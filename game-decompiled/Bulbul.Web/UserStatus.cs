using System.Text.Json.Serialization;

namespace Bulbul.Web;

public class UserStatus
{
	public readonly int Level;

	public readonly int PomodoroSeconds;

	public readonly string LastSaveDate;

	[JsonConstructor]
	public UserStatus(int level, int pomodoroSeconds, string lastSaveDate)
	{
		Level = level;
		PomodoroSeconds = pomodoroSeconds;
		LastSaveDate = lastSaveDate;
	}

	public UserStatus(PlayerDataV3 playerData)
	{
		Level = (playerData?.LevelData?.CurrentLevel).GetValueOrDefault();
		PomodoroSeconds = (int)(playerData?.PomodoroTotalWorkSeconds ?? 0.0);
		LastSaveDate = null;
	}

	public UserStatus(PlayerDataV3 playerData, string lastSaveDate)
	{
		Level = (playerData?.LevelData?.CurrentLevel).GetValueOrDefault();
		PomodoroSeconds = (int)(playerData?.PomodoroTotalWorkSeconds ?? 0.0);
		LastSaveDate = lastSaveDate;
	}
}
