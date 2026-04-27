using Bulbul;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IMusicPlayListButton
{
	GameAudioInfo AudioInfo { get; }

	RectTransform RectTransform { get; }

	Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnStartReorder { get; }

	Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnReorderDrag { get; }

	Observable<(IMusicPlayListButton button, PointerEventData eventData)> OnEndReorder { get; }

	void Setup(GameAudioInfo audioInfo, FacilityMusic facilityMusic);

	void ActivateDragAnimation();

	void DeactivateDragAnimation();

	void Hide();

	void Show();
}
