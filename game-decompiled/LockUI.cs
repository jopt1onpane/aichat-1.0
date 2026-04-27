using DG.Tweening;
using NestopiSystem;
using UnityEngine;
using UnityEngine.UI;

public class LockUI : MonoBehaviour
{
	[SerializeField]
	private Image _lockImage;

	private Tween _lockTween;

	private bool _isActive;

	public bool IsActive => _isActive;

	public void Setup()
	{
		_lockImage.gameObject.SetActive(value: false);
		_lockImage.SetAlpha(0f);
	}

	public void Activate()
	{
		_isActive = true;
		_lockImage.gameObject.SetActive(value: true);
		_lockTween?.Kill();
		_lockTween = _lockImage.DOFade(1f, 0.18f);
	}

	public void Deactivate()
	{
		_isActive = false;
		_lockTween?.Kill();
		_lockTween = _lockImage.DOFade(0f, 0.18f).OnComplete(delegate
		{
			_lockImage.gameObject.SetActive(value: false);
		});
	}
}
