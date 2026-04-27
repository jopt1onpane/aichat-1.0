using System;
using Bulbul;

public class MotionSoundCup : MotionSound
{
	public enum PutPlaceType
	{
		CoffeeMaker,
		DefaultPos
	}

	protected PutPlaceType _putPlaceType;

	public static PutPlaceType TryParsePlaceType(string place)
	{
		if (Enum.TryParse<PutPlaceType>(place, out var result))
		{
			return result;
		}
		return PutPlaceType.DefaultPos;
	}

	public void ChangePutPlace(PutPlaceType putPlaceType)
	{
		_putPlaceType = putPlaceType;
	}

	public override void Play()
	{
		if ((object)_decorationService == null)
		{
			_decorationService = RoomLifetimeScope.Resolve<DecorationService>();
		}
		switch (_decorationService.CurrentMugCupModel.Value)
		{
		case DecorationService.DecorationModelType.Cup_1:
		case DecorationService.DecorationModelType.Cup_2:
		case DecorationService.DecorationModelType.Cup_4_BearsRestaurant:
			PlayNormal();
			break;
		case DecorationService.DecorationModelType.Cup_3:
			PlaySaucer();
			break;
		}
		void PlayNormal()
		{
			Play(AmbientSeType.PlaceCup);
		}
		void PlaySaucer()
		{
			if (_putPlaceType == PutPlaceType.DefaultPos)
			{
				Play(AmbientSeType.PlaceCupOnSaucer);
			}
			else
			{
				Play(AmbientSeType.PlaceCup);
			}
		}
	}
}
