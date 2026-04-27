using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class NewScenarioMarkUI : MonoBehaviour
{
	[SerializeField]
	private Image mark;

	[Inject]
	private ScenarioGroupMasterWrapper scenarioGroupMaster;

	private void Start()
	{
		mark.gameObject.SetActive(value: false);
	}
}
