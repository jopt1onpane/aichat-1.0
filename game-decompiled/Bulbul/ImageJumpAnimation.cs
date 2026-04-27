using DG.Tweening;
using NestopiSystem;
using UnityEngine;

namespace Bulbul;

public class ImageJumpAnimation : MonoBehaviour
{
	[SerializeField]
	private float jumpHeight = 7f;

	[SerializeField]
	private float jumpTime = 0.6f;

	[SerializeField]
	private float interval = 0.5f;

	[SerializeField]
	private string syncKey;

	private Tween tween;

	private void Start()
	{
		float y = base.transform.localPosition.y;
		DOTween.Sequence().AppendInterval(interval).Append(base.transform.DOLocalMoveY(y + jumpHeight, jumpTime).SetEase(Ease.InElastic))
			.Join(base.transform.DOScaleY(1.1f, jumpTime).SetEase(Ease.InElastic))
			.SetLoops(-1, LoopType.Yoyo)
			.Sync(syncKey)
			.RegisterForSync(syncKey)
			.Play()
			.SetLink(base.gameObject);
	}
}
