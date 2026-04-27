using UnityEngine;

namespace MagicLightProbes;

[ExecuteInEditMode]
public class Rotator : MonoBehaviour
{
	public Vector3 localRotationSpeed;

	public Vector3 worldRotationSpeed;

	public bool executeInEditMode;

	public bool unscaledTime;

	private void OnRenderObject()
	{
		if (executeInEditMode && !Application.isPlaying)
		{
			Rotate();
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			Rotate();
		}
	}

	private void Rotate()
	{
		float num = ((!unscaledTime) ? Time.deltaTime : Time.unscaledDeltaTime);
		if (localRotationSpeed != Vector3.zero)
		{
			base.transform.Rotate(localRotationSpeed * num, Space.Self);
		}
		if (worldRotationSpeed != Vector3.zero)
		{
			base.transform.Rotate(worldRotationSpeed * num, Space.World);
		}
	}
}
