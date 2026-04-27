using UnityEngine;

namespace Bulbul;

public class AutoInjectObjectsProvider : MonoBehaviour, IAutoInjectObjectsProvider
{
	[SerializeField]
	private GameObject[] autoInjectObjects;

	public GameObject[] InjectObjects => autoInjectObjects;
}
