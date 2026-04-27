using UnityEngine;
using VContainer;

namespace NestopiSystem.DIContainers;

public abstract class MonoRegister : MonoBehaviour
{
	public abstract void Register(IContainerBuilder builder);
}
