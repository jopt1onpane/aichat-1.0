using UnityEngine;

namespace MagicLightProbes;

[HelpURL("https://motion-games-studio.gitbook.io/magic-light-probes/system-components/mlp-quick-editing")]
public class MLPQuickEditing : MonoBehaviour
{
	public MagicLightProbes parent;

	public float gizmoScale;

	public float drawDistance = 10f;

	private Vector3 _lastPrefabPosition = Vector3.zero;

	private Quaternion _lastPrefabRotation = Quaternion.identity;
}
