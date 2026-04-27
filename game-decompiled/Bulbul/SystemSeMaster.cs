using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "SystemSeMaster", menuName = "ScriptableObject/SystemSeMaster")]
public class SystemSeMaster : ScriptableObject
{
	public SystemSeMasterData[] SystemSes;
}
