using UnityEngine;

namespace Bulbul.Mobile;

public class SyncParentScale : MonoBehaviour
{
	[SerializeField]
	private RectTransform _parent;

	private RectTransform _rt;

	private void Awake()
	{
		_rt = base.transform as RectTransform;
	}

	private void Update()
	{
		_rt.localScale = _parent.transform.localScale;
	}
}
