using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
	public Transform target;

	public Vector3 transformv;

	public float xRot;

	public float RestartRotation;

	public float yRot;

	public float distance = 5f;

	public float sensitivity = 10f;

	public Slider rotationSlider;

	public Slider distanceSlider;

	private bool m_Locked;

	private void Start()
	{
		rotationSlider.onValueChanged.AddListener(delegate
		{
			RotationValueChange();
		});
		distanceSlider.onValueChanged.AddListener(delegate
		{
			DistanceValueChange();
		});
	}

	public void RotationValueChange()
	{
		if (m_Locked)
		{
			yRot = rotationSlider.value;
			base.transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
			base.transform.LookAt(target.position + transformv, Vector3.up);
		}
		else
		{
			yRot = rotationSlider.value;
		}
	}

	public void DistanceValueChange()
	{
		if (m_Locked)
		{
			distance = distanceSlider.value;
			base.transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
			base.transform.LookAt(target.position + transformv, Vector3.up);
		}
		else
		{
			distance = distanceSlider.value;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			LockRotation();
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			yRot = RestartRotation;
		}
		if (!m_Locked)
		{
			yRot += sensitivity * Time.deltaTime;
			base.transform.position = target.position + transformv + Quaternion.Euler(xRot, yRot, 0f) * (distance * -Vector3.back);
			base.transform.LookAt(target.position + transformv, Vector3.up);
			if (yRot >= 360f)
			{
				yRot = 0f;
			}
			UiManager();
		}
	}

	private void UiManager()
	{
		rotationSlider.value = yRot;
	}

	public void LockRotation()
	{
		m_Locked = !m_Locked;
	}
}
