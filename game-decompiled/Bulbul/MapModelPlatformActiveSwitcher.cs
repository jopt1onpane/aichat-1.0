using UnityEngine;

namespace Bulbul;

public class MapModelPlatformActiveSwitcher : MonoBehaviour
{
	[SerializeField]
	private GameObject _m_wall_b;

	[SerializeField]
	private GameObject _m_monitor_b;

	[SerializeField]
	private GameObject _m_monitor_b_emi;

	[SerializeField]
	private GameObject _platform_pc;

	[SerializeField]
	private GameObject _platform_mobile;

	public void Awake()
	{
		_platform_pc.SetActive(value: true);
		_platform_mobile.SetActive(value: false);
	}
}
