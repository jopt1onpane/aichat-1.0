using System;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Bulbul;

[Obsolete]
public class RoomMusicController
{
	[Inject]
	private MyMusicManager _myMusicManager;

	public async UniTaskVoid OnInput(MusicParam parameter)
	{
		_myMusicManager.ChangeVolume(parameter.Volume);
		bool isCallbackSoon = true;
		switch (parameter.MusicButtonType)
		{
		case MusicButtonType.Play:
			if (_myMusicManager.PlayingMusic != null)
			{
				_myMusicManager.UnPause();
			}
			else
			{
				await _myMusicManager.PlayNextMusic(0);
			}
			break;
		case MusicButtonType.Pause:
			_myMusicManager.Pause();
			break;
		case MusicButtonType.Previous:
			await _myMusicManager.PlayNextMusic(-1);
			break;
		case MusicButtonType.Next:
			await _myMusicManager.PlayNextMusic(1);
			break;
		case MusicButtonType.Repeat:
			_myMusicManager.SetRepeat(!_myMusicManager.IsRepeatOneMusic);
			break;
		case MusicButtonType.Random:
			_myMusicManager.EnableRandomPlayback(!_myMusicManager.IsRandom);
			break;
		default:
			Debug.LogWarning($"MusicButtonType:{parameter.MusicButtonType}");
			return;
		case MusicButtonType.Files:
		case MusicButtonType.Directory:
		case MusicButtonType.ChangedVolume:
			break;
		}
		if (isCallbackSoon)
		{
			parameter.Callback?.Invoke();
		}
	}
}
