using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
	public float Normal_Speed = 25f;

	public float Shift_Speed = 54f;

	public float Speed_Cap = 54f;

	public float Camera_Sensitivity = 0.6f;

	private Vector3 Mouse_Location = new Vector3(255f, 255f, 255f);

	private float Total_Speed = 1f;

	public bool freeze;

	public bool freezeMouse = true;

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			freeze = !freeze;
		}
		if (!freeze)
		{
			if (Input.GetKey(KeyCode.M))
			{
				freezeMouse = !freezeMouse;
			}
			if (!freezeMouse)
			{
				Mouse_Location = Input.mousePosition - Mouse_Location;
				Mouse_Location = new Vector3((0f - Mouse_Location.y) * Camera_Sensitivity, Mouse_Location.x * Camera_Sensitivity, 0f);
				Mouse_Location = new Vector3(base.transform.eulerAngles.x + Mouse_Location.x, base.transform.eulerAngles.y + Mouse_Location.y, 0f);
				base.transform.eulerAngles = Mouse_Location;
				Mouse_Location = Input.mousePosition;
			}
			Vector3 vector = GetBaseInput();
			if (Input.GetKey(KeyCode.LeftShift))
			{
				Total_Speed += Time.deltaTime;
				vector = vector * Total_Speed * Shift_Speed;
				vector.x = Mathf.Clamp(vector.x, 0f - Speed_Cap, Speed_Cap);
				vector.y = Mathf.Clamp(vector.y, 0f - Speed_Cap, Speed_Cap);
				vector.z = Mathf.Clamp(vector.z, 0f - Speed_Cap, Speed_Cap);
			}
			else
			{
				Total_Speed = Mathf.Clamp(Total_Speed * 0.5f, 1f, 1000f);
				vector *= Normal_Speed;
			}
			vector *= Time.deltaTime;
			Vector3 position = base.transform.position;
			if (Input.GetKey(KeyCode.Space))
			{
				base.transform.Translate(vector);
				position.x = base.transform.position.x;
				position.z = base.transform.position.z;
				base.transform.position = position;
			}
			else
			{
				base.transform.Translate(vector);
			}
		}
	}

	private Vector3 GetBaseInput()
	{
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		return default(Vector3) + new Vector3(axis, 0f, 0f) + new Vector3(0f, 0f, axis2);
	}
}
