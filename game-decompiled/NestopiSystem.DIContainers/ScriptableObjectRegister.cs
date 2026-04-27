using UnityEngine;
using VContainer;

namespace NestopiSystem.DIContainers;

public class ScriptableObjectRegister : MonoRegister
{
	[SerializeField]
	private ScriptableObject[] singletonScriptableObjects;

	public override void Register(IContainerBuilder builder)
	{
		ScriptableObject[] array = singletonScriptableObjects;
		foreach (ScriptableObject scriptableObject in array)
		{
			builder.RegisterInstance(scriptableObject).As(scriptableObject.GetType());
		}
	}
}
