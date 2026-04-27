using R3;
using UnityEngine;

namespace Bulbul;

public class TransformHasChangedTrigger : MonoBehaviour
{
	private readonly Subject<Unit> _onTransformChanged = new Subject<Unit>();

	public Observable<Unit> OnTransformChanged => _onTransformChanged;

	private void Update()
	{
		if (base.transform.hasChanged)
		{
			base.transform.hasChanged = false;
			_onTransformChanged.OnNext(Unit.Default);
		}
	}
}
