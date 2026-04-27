using UnityEngine;

public class ParentHeightConvertScalingY : MonoBehaviour
{
	[SerializeField]
	private float _parentBaseHeight;

	[SerializeField]
	private RectTransform _parent;

	[SerializeField]
	private RectTransform _syncTarget;

	private void LateUpdate()
	{
		float y = _parent.rect.height / _parentBaseHeight;
		Vector3 localScale = _syncTarget.localScale;
		localScale.y = y;
		_syncTarget.localScale = localScale;
	}
}
