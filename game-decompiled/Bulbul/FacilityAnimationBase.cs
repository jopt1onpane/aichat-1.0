using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Bulbul;

public abstract class FacilityAnimationBase : MonoBehaviour
{
	protected Subject<Unit> onCompleteActivate = new Subject<Unit>();

	protected Subject<Unit> onCompleteDeactivate = new Subject<Unit>();

	public Observable<Unit> OnCompleteActivate => onCompleteActivate;

	public Observable<Unit> OnCompleteDeactivate => onCompleteDeactivate;

	public abstract void Setup();

	public abstract UniTask Activate();

	public abstract UniTask Deactivate();

	protected virtual void OnDestroy()
	{
		onCompleteActivate?.Dispose();
		onCompleteDeactivate?.Dispose();
	}
}
