using UnityEngine;

namespace Bulbul;

public class AddTodoUI : MonoBehaviour
{
	public void Setup()
	{
	}

	public TodoData GetTodoData()
	{
		return new TodoData
		{
			TodoText = string.Empty,
			CurrentState = TodoState.Working
		};
	}
}
