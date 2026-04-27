using Bulbul;
using R3;
using UnityEngine;

public class MotionSoundKeyboard : MotionSound
{
	public void Setup(MonoBehaviour owner)
	{
		if ((object)_decorationService == null)
		{
			_decorationService = RoomLifetimeScope.Resolve<DecorationService>();
		}
		ObservableSubscribeExtensions.Subscribe(_decorationService.CurrentKeyboardModel, delegate
		{
			Stop();
		}).AddTo(owner);
	}

	public override void Play()
	{
		switch (_decorationService.CurrentKeyboardModel.Value)
		{
		case DecorationService.DecorationModelType.Keyboard_2:
			PlayPantographKeyboard();
			break;
		case DecorationService.DecorationModelType.Keyboard_3:
			PlayMechanicalKeyboard();
			break;
		default:
			PlayDefaultKeyboard();
			break;
		}
		void PlayDefaultKeyboard()
		{
			Play(AmbientSeType.KeyboardTyping);
		}
		void PlayMechanicalKeyboard()
		{
			if (!IsPlaying(AmbientSeType.MechanicalKeyboard_01) && !IsPlaying(AmbientSeType.MechanicalKeyboard_02) && !IsPlaying(AmbientSeType.MechanicalKeyboard_03))
			{
				Play(Random.Range(1, 4) switch
				{
					1 => AmbientSeType.MechanicalKeyboard_01, 
					2 => AmbientSeType.MechanicalKeyboard_02, 
					3 => AmbientSeType.MechanicalKeyboard_03, 
					_ => AmbientSeType.MechanicalKeyboard_01, 
				});
			}
		}
		void PlayPantographKeyboard()
		{
			if (!IsPlaying(AmbientSeType.PantographKeyboard_01) && !IsPlaying(AmbientSeType.PantographKeyboard_02) && !IsPlaying(AmbientSeType.PantographKeyboard_03))
			{
				Play(Random.Range(1, 4) switch
				{
					1 => AmbientSeType.PantographKeyboard_01, 
					2 => AmbientSeType.PantographKeyboard_02, 
					3 => AmbientSeType.PantographKeyboard_03, 
					_ => AmbientSeType.PantographKeyboard_01, 
				});
			}
		}
	}

	public void Stop()
	{
		Stop(AmbientSeType.KeyboardTyping);
		Stop(AmbientSeType.MechanicalKeyboard_01);
		Stop(AmbientSeType.MechanicalKeyboard_02);
		Stop(AmbientSeType.MechanicalKeyboard_03);
		Stop(AmbientSeType.PantographKeyboard_01);
		Stop(AmbientSeType.PantographKeyboard_02);
		Stop(AmbientSeType.PantographKeyboard_03);
	}
}
