using UnityEngine;

namespace Bulbul;

public class ChangeOrderService : MonoBehaviour
{
	public enum OrderItemType
	{
		Story,
		Decoration,
		Environment,
		Playlist,
		Setting,
		Calendar,
		Todo,
		Note,
		Special,
		Habit
	}

	[SerializeField]
	[Header("並び変え親オブジェクト")]
	private Transform parentContainer;

	[SerializeField]
	[Header("ストーリー")]
	private Transform _storyTransform;

	[SerializeField]
	[Header("気分転換")]
	private Transform _decorationTransform;

	[SerializeField]
	[Header("環境")]
	private Transform _environmentTransform;

	[SerializeField]
	[Header("プレイリスト")]
	private Transform _playlistTransform;

	[SerializeField]
	[Header("設定")]
	private Transform _settingTransform;

	[SerializeField]
	[Header("カレンダー")]
	private Transform _calendarTransform;

	[SerializeField]
	[Header("Todo")]
	private Transform _todoTransform;

	[SerializeField]
	[Header("ノート")]
	private Transform _noteTransform;

	[SerializeField]
	[Header("スペシャル")]
	private Transform _specialTransform;

	[SerializeField]
	[Header("習慣トラッカー")]
	private Transform _habitTransform;

	public void BringToFront(OrderItemType itemType)
	{
		Transform transform = itemType switch
		{
			OrderItemType.Story => _storyTransform, 
			OrderItemType.Decoration => _decorationTransform, 
			OrderItemType.Environment => _environmentTransform, 
			OrderItemType.Playlist => _playlistTransform, 
			OrderItemType.Setting => _settingTransform, 
			OrderItemType.Calendar => _calendarTransform, 
			OrderItemType.Todo => _todoTransform, 
			OrderItemType.Note => _noteTransform, 
			OrderItemType.Special => _specialTransform, 
			OrderItemType.Habit => _habitTransform, 
			_ => null, 
		};
		if (transform != null)
		{
			transform.SetAsLastSibling();
		}
	}
}
