using System;
using System.Collections.Generic;
using System.IO;

public static class PathUtil
{
	public static List<string> GetFilesRecursive(string root, Predicate<string> fileFilter)
	{
		List<string> list = new List<string>();
		Stack<string> stack = new Stack<string>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			string text = stack.Pop();
			try
			{
				if ((new DirectoryInfo(text).Attributes & FileAttributes.ReparsePoint) != 0)
				{
					continue;
				}
				string[] files = Directory.GetFiles(text);
				foreach (string text2 in files)
				{
					if (fileFilter(text2))
					{
						list.Add(text2);
					}
				}
				files = Directory.GetDirectories(text);
				foreach (string item in files)
				{
					stack.Push(item);
				}
			}
			catch (Exception arg)
			{
				Debug.LogWarning($"DirectoryInfo エラー: {text}\n{arg}");
			}
		}
		return list;
	}
}
