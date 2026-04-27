namespace Bulbul.MasterData;

public enum ShortTalkType
{
	GameStart_First_CameraTouch = 11,
	GameStart_LessTowDays_CameraTouch = 12,
	GameStart_LessTowDays = 13,
	GameStart_LessHarfMonth_CameraTouch = 14,
	GameStart_GreaterHarfMonth_CameraTouch = 15,
	GameStart_GreaterMonth_CameraTouch = 16,
	GameStart_GreaterYear_AND_PlayOneHundred_CameraTouch = 17,
	HeroineClickNormal = 8,
	HeroineClickWork = 9,
	HeroineClickWork_Morning = 500,
	HeroineClickWork_Noon = 501,
	HeroineClickWork_Evening = 502,
	HeroineClickWork_Night = 503,
	HeroineClickBreak = 10,
	HeroineClickBreak_Morning = 550,
	HeroineClickBreak_Noon = 551,
	HeroineClickBreak_Evening = 552,
	HeroineClickBreak_Night = 553,
	HeroineSelfShortTalkBreak = 21,
	SelfTalk_Morning = 2000,
	SelfTalk_Noon = 2001,
	SelfTalk_Evening = 2002,
	SelfTalk_Night = 2003,
	SelfTalk_CurrentTime = 2050,
	SelfTalk_WorkedTime = 2100,
	SmallTalk = 23,
	GameEnd = 19,
	GameDemo_Talk = 40
}
