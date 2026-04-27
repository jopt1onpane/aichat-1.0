using R3;
using UnityEngine.Scripting;

namespace ES3Types;

[Preserve]
[ES3Properties(new string[] { "value" })]
public class ES3UserType_SerializableBoolReactiveProperty : ES3ObjectType
{
	public static ES3Type Instance;

	public ES3UserType_SerializableBoolReactiveProperty()
		: base(typeof(SerializableReactiveProperty<bool>))
	{
		Instance = this;
		priority = 1;
	}

	protected override void WriteObject(object obj, ES3Writer writer)
	{
		SerializableReactiveProperty<bool> objectContainingField = (SerializableReactiveProperty<bool>)obj;
		writer.WritePrivateField("value", objectContainingField);
	}

	protected override void ReadObject<T>(ES3Reader reader, object obj)
	{
		SerializableReactiveProperty<bool> serializableReactiveProperty = (SerializableReactiveProperty<bool>)obj;
		foreach (string property in reader.Properties)
		{
			if (property == "value")
			{
				serializableReactiveProperty.Value = reader.Read<bool>();
			}
			else
			{
				reader.Skip();
			}
		}
	}

	protected override object ReadObject<T>(ES3Reader reader)
	{
		SerializableReactiveProperty<bool> serializableReactiveProperty = new SerializableReactiveProperty<bool>();
		ReadObject<T>(reader, serializableReactiveProperty);
		return serializableReactiveProperty;
	}
}
