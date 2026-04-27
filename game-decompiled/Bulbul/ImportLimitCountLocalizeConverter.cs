using System.Linq;
using Cysharp.Text;
using TMPro;

namespace Bulbul;

public class ImportLimitCountLocalizeConverter : ILocalizeConverter
{
	private readonly MusicService _musicService;

	public ImportLimitCountLocalizeConverter(MusicService musicService)
	{
		_musicService = musicService;
	}

	public string Convert(string originalText)
	{
		int num = _musicService.AllMusicList.Count((GameAudioInfo m) => m.PathType == AudioMode.LocalPc);
		if (num < 100)
		{
			return ZString.Format(originalText, num, 100);
		}
		string arg = ZString.Concat("<color=#FF0000>", num, "</color>");
		return ZString.Format(originalText, arg, 100);
	}

	public void Bind(TMP_Text text, string originalText)
	{
		int num = _musicService.AllMusicList.Count((GameAudioInfo m) => m.PathType == AudioMode.LocalPc);
		if (num < 100)
		{
			text.SetTextFormat(originalText, num, 100);
			return;
		}
		string arg = ZString.Concat("<color=#FF0000>", num, "</color>");
		text.SetTextFormat(originalText, arg, 100);
	}
}
