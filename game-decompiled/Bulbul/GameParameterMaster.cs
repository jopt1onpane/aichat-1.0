using UnityEngine;

namespace Bulbul;

[CreateAssetMenu(fileName = "GameParameterMaster", menuName = "ScriptableObject/GameParameterMaster")]
public class GameParameterMaster : ScriptableObject
{
	[SerializeField]
	[Header("会話スピード")]
	public TalkSpeedData TalkSpeedData;

	[SerializeField]
	[Header("ポモドーロ切替時ボイス")]
	public GamePomodoroVoiceData PomodoroVoiceData;

	[SerializeField]
	[Header("ポモドーロ終了会話")]
	public GamePomodoroTalkData PomodoroTalkData;

	[SerializeField]
	[Header("ゲーム終了会話")]
	public GameEndTalkData GameEndTalkData;

	[SerializeField]
	[Header("小話")]
	public SmallTalkData SmallTalkData;

	[Space]
	[SerializeField]
	[Header("レベル")]
	public LevelUpInfoData LevelUpInfoData;

	[Space]
	[SerializeField]
	[Header("ポイント")]
	public PointUpInfoData PointUpInfoData;

	[Space]
	[SerializeField]
	[Header("オルタエゴコラボ用")]
	public AlterEgoParameter AlterEgoData;

	[Space]
	[SerializeField]
	[Header("くまのレストランコラボ用")]
	public BearsRestaurantParameter BearsRestaurantData;

	[Space]
	[SerializeField]
	[Header("バレンタインイベント用")]
	public Valentine2026Parameter Valentine2026Data;

	[Space]
	[SerializeField]
	[Header("旧正月イベント用")]
	public LunaNewYear2026Parameter LunaNewYear2026Data;

	[Space]
	[SerializeField]
	[Header("NearSpringイベント用")]
	public NearSpring2026Parameter NearSpring2026Data;
}
