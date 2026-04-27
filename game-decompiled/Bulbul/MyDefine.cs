using System;
using DG.Tweening;
using UnityEngine;

namespace Bulbul;

public class MyDefine
{
	public const string EntryLog = "[Entry]⇒";

	public const float GameDemoInitTimeLimitSeconds = 9000f;

	public const string GameDemoLastEpisodeGroupID = "main_05";

	public const float GameStartDirectionMinSeconds = 2f;

	public static readonly Color CommonRed = new Color(13f / 15f, 24f / 85f, 37f / 85f);

	public const float TutorialFocusSeconds = 3.5f;

	public const float UIParentFadeSeconds = 0.5f;

	public const float BlackFadeOutSeconds = 2f;

	public const Ease BlackFadeOutEase = Ease.Unset;

	public const float BlackFadeInSeconds = 1.2f;

	public const Ease BlackFadeInEase = Ease.Unset;

	public const float BottomBackImageDefaultHeight = 180f;

	public const float BottomBackImageStoryHeight = 308f;

	public const float AllUIActivateSecond = 0.2f;

	public const float AllUIDeactivateSecond = 0.2f;

	public const float BottomUITweenDeactivateMovePos = -150f;

	public const float BottomUITweenActivateSecond = 0.2f;

	public const int GameStartEpisodeFirstCameraTouchNumberMax = 1;

	public const int GameStartEpisodeLessTowDaysCameraTouchNumberMax = 12;

	public const int GameStartEpisodeLessTowDaysNumberMax = 17;

	public const int GameStartEpisodeLessHarfMonthCameraTouchNumberMax = 8;

	public const int GameStartEpisodeGreaterMonthCameraTouchNumberMax = 2;

	public const int GameStartEpisodeLessTowDaysCameraTouchMorningNumberMax = 9;

	public const int GameStartEpisodeLessTowDaysCameraTouchNoonNumberMax = 4;

	public const int GameStartEpisodeLessTowDaysCameraTouchEveningNumberMax = 5;

	public const int GameStartEpisodeLessTowDaysCameraTouchNightNumberMax = 7;

	public const float GameStartTimeOfDayGreetingProbabilityPercent = 35f;

	public const int DefaultStartMorningHour = 5;

	public const int DefaultStartNoonHour = 11;

	public const int DefaultStartEveningHour = 17;

	public const int DefaultStartNightHour = 20;

	public const int GameNormalEndEpisodeNumberMin = 1;

	public const int GameNormalEndEpisodeNumberMax = 11;

	public const int GameShortTimeEndEpisodeNumberMin = 1;

	public const int GameShortTimeEndEpisodeNumberMax = 5;

	public const int GameLongTimeEndEpisodeNumberMin = 1;

	public const int GameLongTimeEndEpisodeNumberMax = 5;

	public const int PomodoroEndEpisodeNumberMin = 1;

	public const int PomodoroEndEpisodeNumberMax = 20;

	public const int NormalClickShortTalkEpisodeNumberMin = 1;

	public const int NormalClickShortTalkEpisodeNumberMax = 23;

	public const int WorkClickShortTalkEpisodeNumberMin = 1;

	public const int WorkClickShortTalkEpisodeNumberMax = 23;

	public const int WorkClickShortTalkMorningEpisodeNumberMax = 5;

	public const int WorkClickShortTalkNoonEpisodeNumberMax = 5;

	public const int WorkClickShortTalkEveningEpisodeNumberMax = 5;

	public const int WorkClickShortTalkNightEpisodeNumberMax = 5;

	public const float WorkClickShortTalkTimeOfDayProbabilityPercent = 50f;

	public const int BreakClickBreakShortTalkEpisodeNumberMin = 1;

	public const int BreakClickBreakShortTalkEpisodeNumberMax = 20;

	public const int BreakClickBreakShortTalkMorningEpisodeNumberMax = 10;

	public const int BreakClickBreakShortTalkNoonEpisodeNumberMax = 5;

	public const int BreakClickBreakShortTalkEveningEpisodeNumberMax = 5;

	public const int BreakClickBreakShortTalkNightEpisodeNumberMax = 11;

	public const float BreakClickBreakShortTalkTimeOfDayProbabilityPercent = 50f;

	public const int HeroinClickAnswerChoiceEpisodeNumberMin = 1;

	public const int HeroinClickAnswerChoiceEpisodeNumberMax = 20;

	public const int BreakHeroinSelfShortTalkEpisodeNumberMin = 1;

	public const int BreakHeroinSelfShortTalkEpisodeNumberMax = 22;

	public const int BreakHeroinSelfShortTalkMorningEpisodeNumberMax = 7;

	public const int BreakHeroinSelfShortTalkNoonEpisodeNumberMax = 9;

	public const int BreakHeroinSelfShortTalkEveningEpisodeNumberMax = 7;

	public const int BreakHeroinSelfShortTalkNightEpisodeNumberMax = 7;

	public const float BreakHeroinSelfShortTalkTimeOfDayProbabilityPercent = 50f;

	public const float CurrentTimeSelfTalkOccurrenceProbabilityPercent = 20f;

	public const float WorkedTimeSelfTalkOccurrenceProbabilityPercent = 20f;

	public const int PomodoroNormalStartVoiceEpisodeNumberMin = 1;

	public const int PomodoroNormalStartVoiceEpisodeNumberMax = 15;

	public const int PomodoroContinuousWorkStartVoiceEpisodeNumberMin = 1;

	public const int PomodoroContinuousWorkStartVoiceEpisodeNumberMax = 10;

	public const int PomodoroLongWorkStartVoiceEpisodeNumberMin = 1;

	public const int PomodoroLongWorkStartVoiceEpisodeNumberMax = 10;

	public const int PomodoroBreakVoiceEpisodeNumberMin = 1;

	public const int PomodoroBreakVoiceEpisodeNumberMax = 15;

	public const int PomodoroShortWorkedBreakStartVoiceEpisodeNumberMin = 1;

	public const int PomodoroShortWorkedBreakStartVoiceEpisodeNumberMax = 10;

	public const int PomodoroLongWorkedBreakStartVoiceEpisodeNumberMin = 1;

	public const int PomodoroLongWorkedBreakStartVoiceEpisodeNumberMax = 6;

	public const int PomodoroFinishVoiceEpisodeNumberMin = 1;

	public const int PomodoroFinishVoiceEpisodeNumberMax = 20;

	public const int PomodoroLongWorkFinishVoiceEpisodeNumberMin = 1;

	public const int PomodoroLongWorkFinishVoiceEpisodeNumberMax = 7;

	public const int PomodoroShortWorkFinishVoiceEpisodeNumberMin = 1;

	public const int PomodoroShortWorkFinishVoiceEpisodeNumberMax = 10;

	public const int PomodoroMidwayFinishVoiceEpisodeNumberMin = 1;

	public const int PomodoroMidwayFinishVoiceEpisodeNumberMax = 10;

	public const int LeaveChairStartVoiceEpisodeNumberMin = 1;

	public const int LeaveChairStartVoiceEpisodeNumberMax = 4;

	public const int SnackEatStartVoiceEpisodeNumberMin = 1;

	public const int SnackEatStartVoiceEpisodeNumberMax = 2;

	public const float FacilityFadeSeconds = 0.2f;

	public const float FacilityMoveAmountY = -8f;

	public const float FacilityMoveSeconds = 0.2f;

	public const float LockFadeSeconds = 0.18f;

	public const float TodoInitializePositionX = -1213f;

	public const float TodoInitializeAnchorPositionY = 10f;

	public const float TodoListFocusScrollDuration = 0.5f;

	public const int TodoCompletedTaskDisplayMax = 50;

	public const float HabitInitializePositionX = -194f;

	public const float HabitInitializePositionY = -115f;

	public const float HabitFocusScrollDuration = 0.5f;

	public const float NoteInitializePositionX = -150f;

	public const float NoteInitializePositionY = -62f;

	public const float NoteFocusScrollDuration = 0.5f;

	public const int PomodoroWorkTimeMin = 1;

	public const int PomodoroWorkTimeMax = 999;

	public const int PomodoroRestTimeMin = 1;

	public const int PomodoroBreakTimeMax = 999;

	public const int PomodoroLoopCountMin = 1;

	public const int PomodoroLoopCountMax = 99;

	public const int PomodoroTalkForceFinishTimeSeconds = 15;

	public const int CalendarMinYear = 2025;

	public const int CalendarMaxYear = 2999;

	public const int LayoutPresetCount = 5;

	public const float AmbientSoundDefaultVolume = 0.5f;

	public const string LockedEnvironmentItemLocalizeID = "ui_lock_title";

	public const int MaxImportMusicCount = 100;

	public const int InitLevel = 1;

	public const float NewIconFadeTime = 0.7f;

	public const string FirstResolutionText = "1280x720";

	public const int FirstResolutionWidth = 1280;

	public const int FirstResolutionHeight = 720;

	public const string SecondResolutionText = "1600x900";

	public const int SecondResolutionWidth = 1600;

	public const int SecondResolutionHeight = 900;

	public const string ThirdResolutionText = "1920x1080";

	public const int ThirdResolutionWidth = 1920;

	public const int ThirdResolutionHeight = 1080;

	public const string MasterAudioMixerName = "MasterVolume";

	public const string MusicAudioMixerName = "MusicVolume";

	public const string VoiceAudioMixerName = "VoiceVolume";

	public const string SystemSEAudioMixerName = "SystemSEVolume";

	public const string AmbientBGMAudioMixerName = "AmbientBGMVolume";

	public const string AmbientSEAudioMixerName = "AmbientSEVolume";

	public const float ScenarioMusicFadeSeconds = 1.5f;

	public const float ScenarioProgressImpossibleAvoidSeconds = 30f;

	public const float ScenarioAmbientVolume = 0.5f;

	public const float AutoDelaySec = 0f;

	public const float ShortTalkEndAutoDelaySec = 0f;

	public const string ScenarioID_MainStory1 = "main_01";

	public const string ScenarioID_MainStoryLast = "main_32";

	public const float LastStoryUnlockWorkTimeMinutes = 50f;

	public const float WildMotionProgressImpossibleAvoidSeconds = 10f;

	public const float WildMotionProgressImpossibleLongAvoidSeconds = 20f;

	public const float HeroineActionProgressImpossibleAvoidSeconds = 10f;

	public const float CanPlayPomodoroVoiceSeconds = 5f;

	public const int InitDrinkRemainAmount = 5;

	public const float CanPlayMotionVoiceDelaySeconds = 10f;

	public const float InitLookDurationSeconds = 1.5f;

	public const float InitFacialDelaySecondsAfterTalk = 0.77f;

	public const float AnnounceFadeSeconds = 0.6f;

	public const float AnnounceFadeOutDelaySeconds = 1.8f;

	public const float AnnounceFadeOutLongDelaySeconds = 3f;

	public const float AnnounceMoveAmountY = 7f;

	public const string AnnounceNewStoryLocalizeID = "ui_popup_add_new_story";

	public const string AnnounceNewEnviromentLocalizeID = "announce_new_enviroment";

	public const string AnnounceNewDecorationLocalizeID = "announce_new_decoration";

	public const string AnnounceNewMusicLocalizeID = "announce_new_music";

	public const float AnnounceEndDemoFadeSeconds = 0.6f;

	public const float AnnounceEndDemoFadeOutDelaySeconds = 5f;

	public const float AnnounceEndDemoMoveAmountY = 7f;

	public const string AnnounceEndDemoLocalizeID = "announce_demo_end_story";

	public const string AnnounceSaveDataMigrationLocalizeID = "ui_announce_wait_savedata_migration";

	public const string AnnounceSaveDataMigrationInsufficientLocalizeID = "ui_announce_wait_savedata_migration_insufficient";

	public const string AnnounceSaveDataBrokenBackupLocalizeID = "ui_announce_wait_savedata_broken_backup";

	public const string AnnouncePomodoroNormalModeLocalizeID = "ui_announce_pomodoro_normal_mode";

	public const string AnnouncePomodoroAlterEgoModeLocalizeID = "ui_announce_pomodoro_alterego_mode";

	public const string AnnouncePomodoroBearsRestaurantModeLocalizeID = "ui_announce_pomodoro_bearsrestaurant_mode";

	public const string AnnouncePomodoroValentine2026ModeLocalizeID = "ui_announce_pomodoro_valentine2026_mode";

	public const string AnnouncePomodoroLunaNewYear2026ModeLocalizeID = "ui_announce_pomodoro_lunaNewYear2026_mode";

	public const string AnnouncePomodoroNearSpring2026ModeLocalizeID = "ui_announce_pomodoro_nearSpring2026_mode";

	public const string AnnounceAlterEgoModeLocalizeID = "ui_announce_unlock_alterego_mode";

	public const string AnnounceAlterEgoFinishScenarioLocalizeID = "ui_announce_finish_alterego_scenario";

	public const string AnnounceBearsRestaurantModeLocalizeID = "ui_announce_unlock_bearsrestaurant_mode";

	public const string AnnounceBearsRestaurantFinishScenarioLocalizeID = "ui_announce_finish_bearsrestaurant_scenario";

	public const string AnnounceValentine2026ModeLocalizeID = "ui_announce_unlock_valentine2026_mode";

	public const string AnnounceValentine2026FinishScenarioLocalizeID = "ui_announce_finish_valentine2026_scenario";

	public const string AnnounceLunaNewYear2026ModeLocalizeID = "ui_announce_unlock_lunaNewYear2026_mode";

	public const string AnnounceLunaNewYear2026FinishScenarioLocalizeID = "ui_announce_finish_lunaNewYear2026_scenario";

	public const string AnnounceNearSpring2026ModeLocalizeID = "ui_announce_unlock_nearSpring2026_mode";

	public const string AnnounceNearSpring2026FinishScenarioLocalizeID = "ui_announce_finish_nearSpring2026_scenario";

	public const string AnnounceUnlockExtraScenarioLocalizeID = "ui_popup_add_new_story";

	public const string AnnounceImportImpossibleLimitLocalizeID = "ui_announce_import_impossible_limit";

	public const string AnnounceImportFailedLimitLocalizeID = "ui_announce_import_failed_limit";

	public const string AnnounceImportFailedImportedFileLocalizeID = "ui_announce_import_failed_imported_for_file";

	public const string AnnounceImportFailedImportedFolderLocalizeID = "ui_announce_import_failed_imported_for_folder";

	public const string AnnounceImportFailedInvalidFileLocalizeID = "ui_announce_import_failed_not_supported_single";

	public const string AnnounceImportFailedInvalidFolderLocalizeID = "ui_announce_import_failed__supported_multiple";

	public const float DoubleClickTime = 0.5f;

	public const float UIProgressImpossibleAvoidSeconds = 10f;

	public const float PointCountTweenSeconds = 0.5f;

	public const int AlwaysNoiseStartEpisode = 30;

	public const int ConnectionLostStartEpisode = 31;

	public const int LastStoryEpisodeNo = 32;

	public const int UseAlwaysNoiseNextEpisode = 31;

	public const int UseConnectionLostNextEpisode = 32;

	public const int LastDirectionUpperLevel = 32;

	public const int IsPossibleReconnectLevel = 33;

	public const int ConnectionLostNextEpisodeNeedWorkSeconds = 3000;

	public const TimeFormatType InitTimeFormatType = TimeFormatType.AMPM;

	public const bool InitIsAlwaysOnTop = false;

	public const WindowModeType InitWindowModeType = WindowModeType.Window;

	public const WindowModeType InitWindowModeTypeMobile = WindowModeType.BorderlessFullScreen;

	public const WindowResolutionType InitWindowResolutionType = WindowResolutionType.Third;

	public const bool InitIsUseVerticalSync = false;

	public const int InitActiveFramerate = 60;

	public const int InitActiveFramerateMobile = 30;

	public const int InitDeactiveFramerate = 24;

	public const GraphicQualityLevel InitGraphicQuality = GraphicQualityLevel.High;

	public const GraphicQualityLevel InitGraphicQualityMobile = GraphicQualityLevel.Medium;

	public const int InitRenderScale = 100;

	public const int InitRenderScaleMobile = 70;

	public const int MinRenderScale = 35;

	public const int MaxRenderScale = 100;

	public const float InitMasterVolume = 0.5f;

	public const float InitMasterVolumeMobile = 0.5f;

	public const bool InitMasterIsMute = false;

	public const float InitMusicVolume = 0.5f;

	public const float InitMusicVolumeMobile = 0.5f;

	public const bool InitMusicIsMute = false;

	public const float InitSystemSEVolume = 0.5f;

	public const float InitSystemSEVolumeMobile = 0.5f;

	public const bool InitSystemSEIsMute = false;

	public const float InitVoiceVolume = 0.5f;

	public const float InitVoiceVolumeMobile = 0.5f;

	public const bool InitVoiceIsMute = false;

	public const float InitAmbientBGMVolume = 0.5f;

	public const float InitAmbientBGMVolumeMobile = 0.5f;

	public const bool InitAmbientBGMIsMute = false;

	public const float InitAmbientSEVolume = 0.5f;

	public const float InitAmbientSEVolumeMobile = 0.5f;

	public const bool InitAmbientSEIsMute = false;

	public const bool InitIsPlayPomodoroSE = true;

	public const bool InitIsPlaySelfTalk = true;

	public const bool InitIsNotificationPomodoro = true;

	public const bool InitIsNotificationReminder = true;

	public const float InitWallpaperAutoTransitionSec = 15f;

	public const float WallpaperAutoTransitionDisableSec = -100f;

	public const ScreenSleepMode InitScreenSleepMode = ScreenSleepMode.Disable;

	public const SaveDataSyncInterval InitSaveDataSyncInterval = SaveDataSyncInterval.Sec60;

	public const int AchievementGrassesEpisodeNumber = 33;

	public const int AchievementPublishSummertimeOverdriveEpisodeNumber = 34;

	public const int AchievementLastMainEpisodeNumber = 36;

	public const int AchievementSmallTalkHeatstrokeEpisodeNumber = 1;

	public const int AchievementSmallTalkGalaxyExpressEpisodeNumber = 2;

	public const int AchievementSmallTalkCatHeadphoneEpisodeNumber = 3;

	public const int AchievementSmallTalkCicadaEpisodeNumber = 4;

	public const int AchievementKouClickMaxCount = 5;

	public const int AchievementPomodoroMaxWorkHours = 100;

	public const int SpecialAlterEgoUsePossibleMainEpisodeNumber = 10;

	public const int SpecialAlterEgoLastEpisodeNumber = 2;

	public const int SpecialAlterEgoMaxLevel = 2;

	public const int SpecialBearsRestaurantUsePossibleMainEpisodeNumber = 10;

	public const int SpecialBearsRestaurantLastEpisodeNumber = 2;

	public const int SpecialBearsRestaurantMaxLevel = 2;

	public const int SpecialValentine2026UsePossibleMainEpisodeNumber = 10;

	public static readonly DateTime SpecialValentine2026UnlockStartDate = new DateTime(2026, 2, 14);

	public static readonly DateTime SpecialValentine2026UnlockEndDate = new DateTime(2026, 2, 28, 23, 59, 59);

	public const int SpecialValentine2026LastEpisodeNumber = 2;

	public const int SpecialValentine2026MaxLevel = 2;

	public const int SpecialLunaNewYear2026UsePossibleMainEpisodeNumber = 10;

	public static readonly DateTime SpecialLunaNewYear2026UnlockStartDate = new DateTime(2026, 2, 16);

	public static readonly DateTime SpecialLunaNewYear2026UnlockEndDate = new DateTime(2026, 2, 28, 23, 59, 59);

	public const int SpecialLunaNewYear2026LastEpisodeNumber = 1;

	public const int SpecialLunaNewYear2026MaxLevel = 2;

	public const int SpecialNearSpring2026UsePossibleMainEpisodeNumber = 10;

	public static readonly DateTime SpecialNearSpring2026UnlockStartDate = new DateTime(2026, 3, 1);

	public static readonly DateTime SpecialNearSpring2026UnlockEndDate = new DateTime(2026, 4, 30, 23, 59, 59);

	public const int SpecialNearSpring2026LastEpisodeNumber = 1;

	public const int SpecialNearSpring2026MaxLevel = 2;

	public const int EventNewYearCountdownPreparationSeconds = 10;

	public const int EventNewYearCountdownUseSeconds = 10;

	public const int EventAprilUnlockEpisodeNumber = 3;

	public const int LevelMaxNextNecessaryExp = -1;

	public const int OneTimeGrantFirstPoint = 10000;

	public const float MobileFacilityMoveY = -30f;

	public const float MobileFacilityMoveSeconds = 0.2f;

	public const float MobileFacilityFadeSeconds = 0.2f;

	public const Ease MobileFaciltyEase = Ease.OutQuart;

	public const float MobileFacilityBurgerMoveY = 270f;

	public const float MobileFacilityBurgerActivateSeconds = 0.3f;

	public const float MobileMainUIFadeSeconds = 0.3f;

	public const float MobilePullDownSeconds = 0.2f;

	public const float MobileListItemSlideAnimMoveX = 800f;

	public const float MobileListItemSlideAnimSeconds = 0.2f;

	public const float MobileListItemSizeAnimSeconds = 0.1f;

	public const float MobilePresetAnimSeconds = 0.2f;

	public const float MobileWallpaperFade = 0.2f;

	public const float MobileScrollInputFieldActivateThreshold = 10f;

	public const string AnnounceMobileWallpaperOperationPortraitLocalizeID = "ui_announce_wallpaper_operation_portrait";

	public const string DisabledLocalizeID = "ui_setting_disabled";

	public const string CommonSecWithArgumentLocalizeID = "ui_common_sec";

	public const int MobileReviewRequestTargetLv = 5;

	public const string MobileShopUpgradeLocalizeID = "ui_shop_product_purchase_upgrade";

	public const string MobileShopUpgradePassNameLocalizeID = "ui_shop_product_upgrade_pass_name";

	public const string MobileShopUpgradePassContentText1 = "ui_shop_product_upgrade_pass_content_text_1";

	public const string MobileShopUpgradePassContentText2 = "ui_shop_product_upgrade_pass_content_text_2";

	public const string MobileShopUpgradePassContentText3 = "ui_shop_product_upgrade_pass_content_text_3";

	public const string MobileShopPurchased = "ui_shop_product_already_purchased";

	public const string MobileShopSteamBannerGuide = "ui_shop_banner_steam_guide";

	public const int StandAloneDefaultCameraFOV = 35;

	public const int MobileDefaultCameraFOV = 45;

	public const int StandAloneDefaultCameraFarClip = 1000;

	public const int MobileDefaultCameraFarClip = 1500;

	public const string AnnounceDemoEditionScenarioLimitLocalizeID = "ui_announce_demo_edition_scenario_limit";

	public const string AnnounceDemoEditionLockedLocalizeID = "ui_announce_demo_edition_locked";

	public const int MobileDemoEditionPlayerLimitLv = 11;

	public const int MobileDemoEditionInterstitialAdShowLv = 11;

	public const int MobileDemoEditionLimitStoryNo = 10;

	public const string FAQWebURL = "https://www.nestopi.co.jp/chill-with-you-faq/";

	public const string FAQWebURLEN = "https://www.nestopi.co.jp/en/chill-with-you-faq/";

	public const string SafeSaveKey = "SafeSaveKey";

	public const string SaveWarningNeverShowKey = "SaveWarningNeverShowKey";

	public const int PoorConnectionLoopNum = 7;

	public const float PoorConnectionAnimOneSec = 0.7f;

	public const float PoorConnectionSlideInAnimSec = 0.25f;

	public const float PoorConnectionSlideOutAnimSec = 0.3f;

	public const float PoorConnectionSlideOutFadeAnimSec = 0.2f;

	public const float PoorConnectionAnimationWait = 4f;

	public static string GetDayOfWeekLocalizeID(DayOfWeek dayOfWeek)
	{
		return dayOfWeek switch
		{
			DayOfWeek.Sunday => "ui_calendar_sunday", 
			DayOfWeek.Monday => "ui_calendar_monday", 
			DayOfWeek.Tuesday => "ui_calendar_tuesday", 
			DayOfWeek.Wednesday => "ui_calendar_wednesday", 
			DayOfWeek.Thursday => "ui_calendar_thursday", 
			DayOfWeek.Friday => "ui_calendar_friday", 
			DayOfWeek.Saturday => "ui_calendar_saturday", 
			_ => throw new ArgumentOutOfRangeException("dayOfWeek", dayOfWeek, null), 
		};
	}
}
