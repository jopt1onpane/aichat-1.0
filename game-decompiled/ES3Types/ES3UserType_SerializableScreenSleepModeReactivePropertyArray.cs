using Bulbul;
using R3;

namespace ES3Types;

public class ES3UserType_SerializableScreenSleepModeReactivePropertyArray : ES3ArrayType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableScreenSleepModeReactivePropertyArray()
		: base(typeof(SerializableReactiveProperty<ScreenSleepMode>[]), ES3UserType_SerializableScreenSleepModeReactiveProperty.Instance)
	{
		Instance = this;
	}
}
