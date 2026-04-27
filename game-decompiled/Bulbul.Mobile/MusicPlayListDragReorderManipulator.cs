namespace Bulbul.Mobile;

public class MusicPlayListDragReorderManipulator : DragReorderManipulator<MusicPlayListParam, MusicPlayListItemViewsHolder, MusicPlayListItemModel>
{
	public void UpdatePlayState(GameAudioInfo music, bool isPaused, bool isRemovingMode)
	{
		if (base.DraggedHolder != null)
		{
			MusicPlayListItemViewsHolder draggedHolder = base.DraggedHolder;
			MusicPlayListItemModel draggedModel = base.DraggedModel;
			draggedModel.isPlaying = draggedModel.audioInfo == music;
			draggedModel.isPausing = isPaused && draggedModel.isPlaying;
			draggedHolder.View.DeactivateDraggingImages();
			draggedHolder.UpdateModel(draggedModel, isPlaceHolder: false, isRemovingMode);
			draggedHolder.View.ActivateDraggingImages();
		}
	}

	public void UpdateState(bool isRemovingMode)
	{
		if (base.DraggedHolder != null)
		{
			MusicPlayListItemViewsHolder draggedHolder = base.DraggedHolder;
			MusicPlayListItemModel draggedModel = base.DraggedModel;
			draggedHolder.View.DeactivateDraggingImages();
			draggedHolder.UpdateModel(draggedModel, isPlaceHolder: false, isRemovingMode);
			draggedHolder.View.ActivateDraggingImages();
		}
	}
}
