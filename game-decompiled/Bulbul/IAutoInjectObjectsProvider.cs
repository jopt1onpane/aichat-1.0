using UnityEngine;

namespace Bulbul;

public interface IAutoInjectObjectsProvider
{
	GameObject[] InjectObjects { get; }
}
