using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "MusicMaster", menuName = "ScriptableObject/MusicMaster")]
public class MusicMaster : ScriptableObject
{
	public MusicData[] AudioCollection;
}
