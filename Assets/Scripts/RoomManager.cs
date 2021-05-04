using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		int index = tag[0] - '0';

		if (other.tag == "Player" && index != 5)
		{
			WorldManager.instance.UpdateWorld(index);
		}
	}
}
