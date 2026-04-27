using MagicLightmapSwitcher;
using UnityEngine;

namespace Bulbul;

public class LightmapSwitchService : MonoBehaviour
{
	[SerializeField]
	private StoredLightingScenario lightingScenario;

	private RuntimeAPI switchAPI;

	public void Setup()
	{
		if (!DevicePlatform.Steam.IsPC())
		{
			switchAPI = new RuntimeAPI();
		}
	}

	public void Switch(WindowViewType windowType)
	{
		if (switchAPI != null && !(lightingScenario == null) && windowType.IsTimeType() && windowType.TryConvertToWindowViewType(out var environmentType))
		{
			Switch(environmentType);
		}
	}

	public void Switch(EnvironmentType environment)
	{
		if (switchAPI != null && !(lightingScenario == null) && environment.IsTimeType())
		{
			switchAPI.SwitchLightmap((int)environment, lightingScenario);
		}
	}
}
