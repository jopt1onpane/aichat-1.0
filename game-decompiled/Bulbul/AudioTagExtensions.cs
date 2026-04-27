namespace Bulbul;

public static class AudioTagExtensions
{
	public static bool HasFlagFast(this AudioTag value, AudioTag flag)
	{
		return (value & flag) != 0;
	}

	public static void AddFavorite(this ref AudioTag value)
	{
		value = value.AddFlag(AudioTag.Favorite);
	}

	public static void RemoveFavorite(this ref AudioTag value)
	{
		value = value.RemoveFlag(AudioTag.Favorite);
	}

	public static AudioTag AddFlag(this AudioTag value, AudioTag flag)
	{
		return value | flag;
	}

	public static AudioTag RemoveFlag(this AudioTag value, AudioTag flag)
	{
		return value & ~flag;
	}
}
