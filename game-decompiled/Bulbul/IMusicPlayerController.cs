namespace Bulbul;

public interface IMusicPlayerController
{
	bool IsPaused { get; }

	void OnClickButtonPlayListPlayMusicButton(GameAudioInfo info);

	void UnPauseMusic();

	void PauseMusic();

	void Release();
}
