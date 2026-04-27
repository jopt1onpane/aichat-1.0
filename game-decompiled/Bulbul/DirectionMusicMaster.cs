using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "DirectionMusicMaster", menuName = "ScriptableObject/DirectionMusicMaster")]
public class DirectionMusicMaster : ScriptableObject
{
	public DirectionMusicData[] AudioCollection;
}
