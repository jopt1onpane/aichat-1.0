using UnityEngine;

namespace MagicLightProbes;

[ExecuteInEditMode]
public class MLPPrefab : MonoBehaviour
{
	public new string name;

	public string uid;

	private void OnEnable()
	{
		name = base.gameObject.name;
		uid = base.gameObject.GetInstanceID().ToString();
	}
}
