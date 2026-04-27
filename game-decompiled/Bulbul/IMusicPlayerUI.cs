using R3;

namespace Bulbul;

public interface IMusicPlayerUI : IMusicUIBase
{
	Observable<Unit> OnClickSwitchMuteButton { get; }

	Observable<float> OnChangeVolume { get; }

	Observable<Unit> OnClickPlayOrPauseButton { get; }

	Observable<Unit> OnClickShuffleButton { get; }

	Observable<Unit> OnClickNextButton { get; }

	Observable<Unit> OnClickBackButton { get; }

	Observable<Unit> OnClickLoopButton { get; }

	Observable<float> OnChangeProgress { get; }

	void Setup(bool isPlay, bool isLoop);

	void SetMute(bool isMute);

	void OnPauseMusic();

	void OnPlayMusic();

	void OnChangeShuffle(bool isShuffle);

	void OnChangeLoop(bool isLoop);

	void OnChangeMusic(string musicName, string artistName, MusicChangeKind kind);

	void UpdateProgressBar(float amount);
}
