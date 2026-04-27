using R3;
using UnityEngine;
using VContainer;

namespace Bulbul;

public class FacilityCurrentDateAndTime : MonoBehaviour
{
	[Inject]
	private ICurrentDateAndTimeUI _currentDateAndTimeUI;

	public void Setup(ReadOnlyReactiveProperty<bool> storyLogIsActive, ReadOnlyReactiveProperty<bool> decorationIsActive, ReadOnlyReactiveProperty<bool> environmentIsActive, ReadOnlyReactiveProperty<bool> playlistIsActive, ReadOnlyReactiveProperty<bool> collaborationListIsActive)
	{
		Observable.CombineLatest<bool>(storyLogIsActive, decorationIsActive, environmentIsActive, playlistIsActive, collaborationListIsActive).Subscribe(delegate(bool[] values)
		{
			if (!values[0] && !values[1] && !values[2] && !values[3] && !values[4])
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}).AddTo(this);
		_currentDateAndTimeUI.Setup();
	}

	public void Activate()
	{
		_currentDateAndTimeUI.Activate();
	}

	public void Deactivate()
	{
		_currentDateAndTimeUI.Deactivate();
	}
}
