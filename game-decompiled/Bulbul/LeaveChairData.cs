using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class LeaveChairData
{
	[Header("発生条件")]
	[SerializeField]
	[Header("離席が発生しうる作業時間（分）の最小値")]
	public int WorkMinutesMin = 80;

	[SerializeField]
	[Header("離席が発生しうる作業時間（分）の最大値")]
	public int WorkMinutesMax = 120;

	[SerializeField]
	[Header("離席が発生しうる休憩時間（分）の最小値")]
	public int BreakMinutesMin = 80;

	[SerializeField]
	[Header("離席が発生しうる休憩時間（分）の最大値")]
	public int BreakMinutesMax = 120;

	[SerializeField]
	[Header("休憩中の抽選間隔（分）")]
	public int LotteryIntervalMinutes = 5;

	[SerializeField]
	[Header("離席発生確率（0～100）")]
	public float OccurrenceProbabilityPercent = 30f;

	[Header("ソファ")]
	[SerializeField]
	[Header("ソファ滞在時間（秒）の最小値")]
	public float SofaStaySecondsMin = 60f;

	[SerializeField]
	[Header("ソファ滞在時間（秒）の最大値")]
	public float SofaStaySecondsMax = 180f;

	[SerializeField]
	[Header("ソファ強制滞在時間（秒） \n 強制的に戻される時も必ず不自然にならないようにこの秒数は待つ")]
	public float SofaForceStaySeconds = 10f;

	[Header("キッチン（カメラ外）")]
	[SerializeField]
	[Header("キッチン滞在時間（秒）の最小値")]
	public float KitchenStaySecondsMin = 20f;

	[SerializeField]
	[Header("キッチン滞在時間（秒）の最大値")]
	public float KitchenStaySecondsMax = 60f;

	[SerializeField]
	[Header("キッチン強制滞在時間（秒） \n 強制的に戻される時も必ず不自然にならないようにこの秒数は待つ")]
	public float KitchenForceStaySeconds = 3f;

	[SerializeField]
	[Header("カップケーキ:持ってくる確率")]
	[Range(0f, 100f)]
	public float CupcakeTakeProbability = 40f;

	[SerializeField]
	[Header("カップケーキ:Rare選出確率")]
	[Range(0f, 100f)]
	public float CupcakeRareProbability = 30f;
}
