using System;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class LoadingScreen : MonoBehaviour
{
	public readonly struct LoadingScope : IDisposable
	{
		private readonly LoadingScreen loadingScreen;

		public LoadingScope(LoadingScreen loadingScreen)
		{
			this.loadingScreen = loadingScreen;
			if ((bool)loadingScreen)
			{
				loadingScreen.Show();
			}
		}

		public void Dispose()
		{
			if ((bool)loadingScreen)
			{
				loadingScreen.Hide();
			}
		}
	}

	[SerializeField]
	private RectTransform loadingImage;

	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private float rotateSpeed = 1f;

	[SerializeField]
	private float angleSnap = 45f;

	[SerializeField]
	private float defaultBgAlpha = 1f;

	private float angle;

	private float lastSnappedAngle;

	public void Show()
	{
		angle = 0f;
		SetBgAlpha(defaultBgAlpha);
		SetActiveBg(active: true);
		if ((bool)base.gameObject)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if ((bool)base.gameObject)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetActiveBg(bool active)
	{
		if ((bool)base.gameObject)
		{
			bgImage.enabled = active;
		}
	}

	public void SetBgAlpha(float alpha)
	{
		bgImage.SetAlpha(alpha);
	}

	private void Update()
	{
		angle += rotateSpeed * Time.deltaTime;
		angle = Mathf.Repeat(angle, 360f);
		if (angleSnap <= 0f)
		{
			loadingImage.rotation = Quaternion.Euler(0f, 0f, 0f - angle);
			return;
		}
		float num = Mathf.Round(angle / angleSnap) * angleSnap;
		if (!Mathf.Approximately(num, lastSnappedAngle))
		{
			lastSnappedAngle = num;
			loadingImage.rotation = Quaternion.Euler(0f, 0f, 0f - num);
		}
	}

	public LoadingScope CreateLoadingScope()
	{
		return new LoadingScope(this);
	}

	public LoadingScope CreateEmptyScope()
	{
		return default(LoadingScope);
	}

	public async UniTask<T> ShowLoadingAsync<T>(Func<UniTask<T>> task)
	{
		using (CreateLoadingScope())
		{
			return await task();
		}
	}

	public async UniTask ShowLoadingAsync(Func<UniTask> task)
	{
		using (CreateLoadingScope())
		{
			await task();
		}
	}
}
