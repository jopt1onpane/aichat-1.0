using System;
using System.Threading;
using Bulbul;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class ScreenSharingDirection : MonoBehaviour
{
	[Inject]
	private IDirectionServiceUIProvider _directionServiceUIProvider;

	private const string StartAnimName = "anim_ui_screensharing_In";

	private const string EndAnimName = "anim_ui_screensharing_Out";

	private const string AnimActiveBoolName = "Active";

	private CancellationTokenSource _cts;

	private Animator _animator;

	public void Setup()
	{
		_animator = _directionServiceUIProvider.ScreenSharing;
		_animator.gameObject.SetActive(value: false);
	}

	public void Play()
	{
		_animator.gameObject.SetActive(value: true);
		_animator.Play("anim_ui_screensharing_In", 0, 0f);
		_animator.SetBool("Active", value: true);
	}

	public void End()
	{
		_animator.SetBool("Active", value: false);
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = new CancellationTokenSource();
		WaitForAnimationEndAsync(_cts.Token, "anim_ui_screensharing_Out", delegate
		{
			_animator.gameObject.SetActive(value: false);
		}).Forget();
	}

	public void EndTidying()
	{
		if (_animator.gameObject.activeInHierarchy)
		{
			_animator.SetBool("Active", value: false);
			_animator.Play("anim_ui_screensharing_Out", 0, 1f);
			_animator.gameObject.SetActive(value: false);
		}
		_cts?.Cancel();
		_cts?.Dispose();
	}

	private async UniTaskVoid WaitForAnimationEndAsync(CancellationToken cancellationToken, string animName, Action onEndAction)
	{
		bool _isWaitEndAnim = true;
		try
		{
			while (_isWaitEndAnim)
			{
				if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && _animator.GetCurrentAnimatorStateInfo(0).IsName(animName))
				{
					onEndAction();
					break;
				}
				await UniTask.Yield(cancellationToken);
			}
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			_cts?.Dispose();
			_cts = null;
		}
	}

	private void OnDestroy()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
	}
}
