using System.Threading;
using Cysharp.Threading.Tasks;

namespace NestopiSystem;

public static class NativeFilePickerTask
{
	public static UniTask<string> PickFileAsync(string[] allowedFileTypes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		UniTaskCompletionSource<string> completionSource = new UniTaskCompletionSource<string>();
		ct.RegisterCompletionSource(completionSource);
		NativeFilePicker.PickFile(delegate(string path)
		{
			completionSource.TrySetResult(path);
		}, allowedFileTypes);
		return completionSource.Task;
	}

	public static UniTask<string[]> PickMultipleFilesAsync(string[] allowedFileTypes, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		UniTaskCompletionSource<string[]> completionSource = new UniTaskCompletionSource<string[]>();
		ct.RegisterCompletionSource(completionSource);
		NativeFilePicker.PickMultipleFiles(delegate(string[] paths)
		{
			completionSource.TrySetResult(paths);
		}, allowedFileTypes);
		return completionSource.Task;
	}
}
