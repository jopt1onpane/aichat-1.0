using R3;
using UnityEngine;

namespace Bulbul;

public abstract class EnvironmentControllerBase : MonoBehaviour
{
	public abstract ReadOnlyReactiveProperty<bool> IsNewIconActive { get; }

	public abstract void DeactivateNewIcon();

	public abstract void ApplyWindowBySaveData();
}
