using FastEnumUtility;

namespace Bulbul;

public static class WindowViewTypeExtensions
{
	public static bool IsTimeType(this WindowViewType windowViewType)
	{
		if (windowViewType != WindowViewType.Day && windowViewType != WindowViewType.Sunset && windowViewType != WindowViewType.Night)
		{
			return windowViewType == WindowViewType.Cloudy;
		}
		return true;
	}

	public static bool TryConvertToWindowViewType(this WindowViewType windowViewType, out EnvironmentType environmentType)
	{
		return FastEnum.TryParse<EnvironmentType>(windowViewType.ToName(), out environmentType);
	}
}
