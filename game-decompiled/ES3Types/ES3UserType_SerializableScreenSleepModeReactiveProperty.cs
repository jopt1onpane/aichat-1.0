using Bulbul;
using R3;
using UnityEngine.Scripting;

namespace ES3Types;

[Preserve]
[ES3Properties(new string[] { "value" })]
public class ES3UserType_SerializableScreenSleepModeReactiveProperty : ES3ObjectType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableScreenSleepModeReactiveProperty()
		: base(typeof(SerializableReactiveProperty<ScreenSleepMode>))
	{
		Instance = this;
		priority = 1;
	}

	protected override void WriteObject(object obj, ES3Writer writer)
	{
		SerializableReactiveProperty<ScreenSleepMode> objectContainingField = (SerializableReactiveProperty<ScreenSleepMode>)obj;
		writer.WritePrivateField("value", objectContainingField);
	}

	protected override void ReadObject<T>(ES3Reader reader, object obj)
	{
		SerializableReactiveProperty<ScreenSleepMode> serializableReactiveProperty = (SerializableReactiveProperty<ScreenSleepMode>)obj;
		foreach (string property in reader.Properties)
		{
			if (property == "value")
			{
				serializableReactiveProperty.Value = reader.Read<ScreenSleepMode>();
			}
			else
			{
				reader.Skip();
			}
		}
	}

	protected override object ReadObject<T>(ES3Reader reader)
	{
		SerializableReactiveProperty<ScreenSleepMode> serializableReactiveProperty = new SerializableReactiveProperty<ScreenSleepMode>();
		ReadObject<T>(reader, serializableReactiveProperty);
		return serializableReactiveProperty;
	}
}
