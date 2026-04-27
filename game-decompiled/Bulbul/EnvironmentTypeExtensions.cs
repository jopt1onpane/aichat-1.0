using FastEnumUtility;

namespace Bulbul;

public static class EnvironmentTypeExtensions
{
	public static bool IsTimeType(this EnvironmentType environmentType)
	{
		if (environmentType != EnvironmentType.Day && environmentType != EnvironmentType.Sunset && environmentType != EnvironmentType.Night)
		{
			return environmentType == EnvironmentType.Cloudy;
		}
		return true;
	}

	public static bool TryConvertToWindowViewType(this EnvironmentType environmentType, out WindowViewType windowViewType)
	{
		return FastEnum.TryParse<WindowViewType>(environmentType.ToName(), out windowViewType);
	}

	public static bool TryConvertToAmbientSoundType(this EnvironmentType environmentType, out AmbientSoundType ambientSoundType)
	{
		if (environmentType == EnvironmentType.Fireworks)
		{
			ambientSoundType = AmbientSoundType.Fireworks_First;
			return true;
		}
		return FastEnum.TryParse<AmbientSoundType>(environmentType.ToName(), out ambientSoundType);
	}

	public static EnvironmentControllerType GetEnvironmentControllerType(this EnvironmentType environmentType)
	{
		WindowViewType windowViewType;
		bool flag = environmentType.TryConvertToWindowViewType(out windowViewType);
		AmbientSoundType ambientSoundType;
		bool flag2 = environmentType.TryConvertToAmbientSoundType(out ambientSoundType);
		if (flag && flag2)
		{
			return EnvironmentControllerType.ViewAndSound;
		}
		if (flag)
		{
			return EnvironmentControllerType.ViewOnly;
		}
		return EnvironmentControllerType.SoundOnly;
	}
}
