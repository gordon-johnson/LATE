using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	public Transform player;
	public float xDist = 0f;
	public float yDist = 3f;
	public float zDist = -1f;


    void Update()
    {
		transform.position = new Vector3(player.position.x + xDist, player.position.y + yDist, player.position.z + zDist);
    }
}
