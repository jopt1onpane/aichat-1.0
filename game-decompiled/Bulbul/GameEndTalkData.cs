using System;
using UnityEngine;

namespace Bulbul;

[Serializable]
public class GameEndTalkData
{
	[SerializeField]
	[Header("長時間")]
	private GameLongTimeEndTalkData gameLongTimeEndTalkData;

	[SerializeField]
	[Header("短時間")]
	private GameShortTimeEndTalkData gameShortTimeEndTalkData;

	public GameLongTimeEndTalkData GameLongTimeEndTalkData => gameLongTimeEndTalkData;

	public GameShortTimeEndTalkData GameShortTimeEndTalkData => gameShortTimeEndTalkData;
}
