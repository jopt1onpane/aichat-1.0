using System;
using DG.Tweening;

namespace Bulbul.MasterData;

[Serializable]
public class NovelData
{
	public string ID;

	public string ScenarioGroupID;

	public CommandType Command;

	public string Arg1;

	public float Arg2;

	public string Arg3;

	public string Arg4;

	public TalkerType Talker;

	public string LocalizationID;

	public int BodyMotion;

	public int FacialMotion;

	public float LookScale;

	public float LookSpeedSeconds;

	public Ease LookEaseType;
}
