using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilityFailedReactionIconEffect : MonoBehaviour
{
	[Inject]
	private FacilityClickHeroine _facilityClickHeroine;

	[SerializeField]
	private FailedReactionIconEffectView _effect;

	public void Setup()
	{
		_effect.Setup();
		_facilityClickHeroine.OnUnableReactionToTap.Subscribe(delegate(Vector2 position)
		{
			_effect.Activate(position);
		}).AddTo(this);
	}
}
