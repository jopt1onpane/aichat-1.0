using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Bulbul.Web;

[JsonSerializable(typeof(ResetReasonResponse))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, IncludeFields = true)]
[GeneratedCode("System.Text.Json.SourceGeneration", "10.0.13.7005")]
internal class ResetReasonResponseGenerationContext : JsonSerializerContext, IJsonTypeInfoResolver
{
	private JsonTypeInfo<ResetReason>? _ResetReason;

	private JsonTypeInfo<ResetReasonResponse>? _ResetReasonResponse;

	private static readonly JsonSerializerOptions s_defaultOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
	{
		IncludeFields = true
	};

	private const BindingFlags InstanceMemberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly JsonEncodedText PropName_reset_reason = JsonEncodedText.Encode("reset_reason");

	public JsonTypeInfo<ResetReason> ResetReason => _ResetReason ?? (_ResetReason = (JsonTypeInfo<ResetReason>)base.Options.GetTypeInfo(typeof(ResetReason)));

	public JsonTypeInfo<ResetReasonResponse> ResetReasonResponse => _ResetReasonResponse ?? (_ResetReasonResponse = (JsonTypeInfo<ResetReasonResponse>)base.Options.GetTypeInfo(typeof(ResetReasonResponse)));

	public static ResetReasonResponseGenerationContext Default { get; } = new ResetReasonResponseGenerationContext(new JsonSerializerOptions(s_defaultOptions));

	protected override JsonSerializerOptions? GeneratedSerializerOptions { get; } = s_defaultOptions;

	private JsonTypeInfo<ResetReason> Create_ResetReason(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ResetReason> jsonTypeInfo))
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ResetReason>(options, JsonMetadataServices.GetEnumConverter<ResetReason>(options));
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private JsonTypeInfo<ResetReasonResponse> Create_ResetReasonResponse(JsonSerializerOptions options)
	{
		if (!TryGetTypeInfoForRuntimeCustomConverter(options, out JsonTypeInfo<ResetReasonResponse> jsonTypeInfo))
		{
			JsonObjectInfoValues<ResetReasonResponse> objectInfo = new JsonObjectInfoValues<ResetReasonResponse>
			{
				ObjectCreator = null,
				ObjectWithParameterizedConstructorCreator = (object[] args) => new ResetReasonResponse((ResetReason)args[0]),
				PropertyMetadataInitializer = (JsonSerializerContext _) => ResetReasonResponsePropInit(options),
				ConstructorParameterMetadataInitializer = ResetReasonResponseCtorParamInit,
				ConstructorAttributeProviderFactory = () => typeof(ResetReasonResponse).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ResetReason) }, null),
				SerializeHandler = ResetReasonResponseSerializeHandler
			};
			jsonTypeInfo = JsonMetadataServices.CreateObjectInfo(options, objectInfo);
			jsonTypeInfo.NumberHandling = null;
		}
		jsonTypeInfo.OriginatingResolver = this;
		return jsonTypeInfo;
	}

	private static JsonPropertyInfo[] ResetReasonResponsePropInit(JsonSerializerOptions options)
	{
		JsonPropertyInfo[] array = new JsonPropertyInfo[1];
		JsonPropertyInfoValues<ResetReason> propertyInfo = new JsonPropertyInfoValues<ResetReason>
		{
			IsProperty = false,
			IsPublic = true,
			IsVirtual = false,
			DeclaringType = typeof(ResetReasonResponse),
			Converter = null,
			Getter = (object obj) => ((ResetReasonResponse)obj).ResetReason,
			Setter = null,
			IgnoreCondition = null,
			HasJsonInclude = false,
			IsExtensionData = false,
			NumberHandling = null,
			PropertyName = "ResetReason",
			JsonPropertyName = "reset_reason",
			AttributeProviderFactory = () => typeof(ResetReasonResponse).GetField("ResetReason", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
		};
		array[0] = JsonMetadataServices.CreatePropertyInfo(options, propertyInfo);
		return array;
	}

	private void ResetReasonResponseSerializeHandler(Utf8JsonWriter writer, ResetReasonResponse value)
	{
		writer.WriteStartObject();
		writer.WritePropertyName(PropName_reset_reason);
		JsonSerializer.Serialize(writer, value.ResetReason, ResetReason);
		writer.WriteEndObject();
	}

	private static JsonParameterInfoValues[] ResetReasonResponseCtorParamInit()
	{
		return new JsonParameterInfoValues[1]
		{
			new JsonParameterInfoValues
			{
				Name = "resetReason",
				ParameterType = typeof(ResetReason),
				Position = 0,
				HasDefaultValue = false,
				DefaultValue = null,
				IsNullable = false
			}
		};
	}

	public ResetReasonResponseGenerationContext()
		: base(null)
	{
	}

	public ResetReasonResponseGenerationContext(JsonSerializerOptions options)
		: base(options)
	{
	}

	private static bool TryGetTypeInfoForRuntimeCustomConverter<TJsonMetadataType>(JsonSerializerOptions options, out JsonTypeInfo<TJsonMetadataType> jsonTypeInfo)
	{
		JsonConverter runtimeConverterForType = GetRuntimeConverterForType(typeof(TJsonMetadataType), options);
		if (runtimeConverterForType != null)
		{
			jsonTypeInfo = JsonMetadataServices.CreateValueInfo<TJsonMetadataType>(options, runtimeConverterForType);
			return true;
		}
		jsonTypeInfo = null;
		return false;
	}

	private static JsonConverter? GetRuntimeConverterForType(Type type, JsonSerializerOptions options)
	{
		for (int i = 0; i < options.Converters.Count; i++)
		{
			JsonConverter jsonConverter = options.Converters[i];
			if (jsonConverter != null && jsonConverter.CanConvert(type))
			{
				return ExpandConverter(type, jsonConverter, options, validateCanConvert: false);
			}
		}
		return null;
	}

	private static JsonConverter ExpandConverter(Type type, JsonConverter converter, JsonSerializerOptions options, bool validateCanConvert = true)
	{
		if (validateCanConvert && !converter.CanConvert(type))
		{
			throw new InvalidOperationException($"The converter '{converter.GetType()}' is not compatible with the type '{type}'.");
		}
		if (converter is JsonConverterFactory jsonConverterFactory)
		{
			converter = jsonConverterFactory.CreateConverter(type, options);
			if (converter == null || converter is JsonConverterFactory)
			{
				throw new InvalidOperationException($"The converter '{jsonConverterFactory.GetType()}' cannot return null or a JsonConverterFactory instance.");
			}
		}
		return converter;
	}

	public override JsonTypeInfo? GetTypeInfo(Type type)
	{
		base.Options.TryGetTypeInfo(type, out JsonTypeInfo typeInfo);
		return typeInfo;
	}

	JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		if (type == typeof(ResetReason))
		{
			return Create_ResetReason(options);
		}
		if (type == typeof(ResetReasonResponse))
		{
			return Create_ResetReasonResponse(options);
		}
		return null;
	}
}
