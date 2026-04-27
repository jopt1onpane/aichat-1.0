using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using SFB;

namespace NestopiSystem;

public static class FileBrowser
{
	private static readonly string[] musicFileTypes = new string[4] { "wav", "mp3", "flac", "ogg" };

	private static readonly ExtensionFilter[] extensionFilter = new ExtensionFilter[1]
	{
		new ExtensionFilter("Sound Files", musicFileTypes)
	};

	public static async UniTask<IEnumerable<string>> OpenMusicFiles(bool openFolder, CancellationToken ct)
	{
		if (openFolder)
		{
			string[] source = StandaloneFileBrowser.OpenFolderPanel("OpenFolder", "", multiselect: true);
			Predicate<string> filter = delegate(string file)
			{
				string text = Path.GetExtension(file).ToLower();
				string[] array = musicFileTypes;
				foreach (string text2 in array)
				{
					if (text == "." + text2.ToLower())
					{
						return true;
					}
				}
				return false;
			};
			return source.SelectMany((string p) => PathUtil.GetFilesRecursive(p, filter));
		}
		return StandaloneFileBrowser.OpenFilePanel("OpenFile", "", extensionFilter, multiselect: true);
	}
}
