using UnityEngine;

namespace Bulbul;

public class HeroineEyeLookAt
{
	public const int BlendShapeIndexEyeLeft = 36;

	public const int BlendShapeIndexEyeUp = 37;

	public const int BlendShapeIndexEyeDown = 38;

	public const float LeftMaxAngle = 22f;

	public const float UpMaxAngle = 3f;

	public const float DownMaxAngle = 11f;

	private readonly SkinnedMeshRenderer _headSkin;

	private readonly Transform _headTransform;

	public HeroineEyeLookAt(SkinnedMeshRenderer headSkin, Transform headTransform)
	{
		_headSkin = headSkin;
		_headTransform = headTransform;
	}

	public void LookAtDirection(Vector3 worldDirection)
	{
		Vector3 vector = _headTransform.InverseTransformDirection(worldDirection);
		float value = (0f - Mathf.Atan2(vector.x, vector.z)) * 57.29578f;
		float value2 = Mathf.InverseLerp(0f, 22f, value) * 100f;
		float num = Mathf.Atan2(vector.y, new Vector2(vector.x, vector.z).magnitude) * 57.29578f;
		float value3 = Mathf.InverseLerp(0f, 3f, num) * 100f;
		float value4 = Mathf.InverseLerp(0f, 11f, 0f - num) * 100f;
		_headSkin.SetBlendShapeWeight(36, value2);
		_headSkin.SetBlendShapeWeight(37, value3);
		_headSkin.SetBlendShapeWeight(38, value4);
	}
}
