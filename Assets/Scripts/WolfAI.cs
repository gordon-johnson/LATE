using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAI : MonoBehaviour
{
	public float movementSpeed = 1.0f;
	private GameObject player;
	private bool active = true;
	private bool colliding = false;
	private bool attacking = false;

	// Start is called before the first frame update
    void Start()
    {
		player = WorldManager.instance.player;
    }

    // Update is called once per frame
    void Update()
    {
		if (Vector3.Distance(player.transform.position, transform.localPosition) < 15.0f)
		{
			if (active && !colliding)
			{
				movementSpeed = WorldManager.instance.player.GetComponent<PlayerController>().movementSpeed;
				Vector3 temp = (player.transform.position - transform.localPosition);
				temp = ((movementSpeed * 0.15f) / (temp.magnitude + 1.0f)) * temp.normalized;
				transform.position += temp;

				transform.rotation = Quaternion.LookRotation(transform.position - player.transform.position);
			}
			else
			{
				transform.position += Vector3.zero;
			}
		}
		else
		{
			WorldManager.instance.enemies.Remove(gameObject);
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Net")
		{
			StartCoroutine("pauseOnTrigger");
			StartCoroutine("flashOnTrigger");
		}
		if (other.tag == "Player")
		{
			colliding = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.tag == "Player" && colliding && !attacking)
		{
			if (!WorldManager.instance.GODMODE)
			{
				StartCoroutine("attack");
				StartCoroutine("flashPlayerOnTrigger");
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			colliding = false;
		}
	}

	IEnumerator pauseOnTrigger()
	{
		active = false;
		yield return new WaitForSeconds(1.0f);
		active = true;
	}

	IEnumerator attack()
	{
		attacking = true;
		if (WorldManager.instance.numFirefly > 0)
		{
			WorldManager.instance.numFirefly -= 2;
		}
		yield return new WaitForSeconds(1.0f);
		attacking = false;
	}

	IEnumerator flashOnTrigger()
	{
		foreach(Transform child in transform)
		{
			child.GetComponent<Renderer>().material.color = Color.red;
		}
		yield return new WaitForSeconds(1.0f);
		foreach (Transform child in transform)
		{
			child.GetComponent<Renderer>().material.color = Color.black;
		}
	}

	IEnumerator flashPlayerOnTrigger()
	{
		foreach (Transform child in WorldManager.instance.player.transform)
		{
			if (child.GetComponent<Renderer>() != null)
			{
				if (child.name == "Player") child.GetComponent<Renderer>().material.color = Color.red;
			}
			if (child.childCount > 0)
			{
				foreach (Transform subchild in child)
				{
					if (subchild.name == "Player") subchild.GetComponent<Renderer>().material.color = Color.red;
				}
			}
		}
		yield return new WaitForSeconds(0.1f);
		foreach (Transform child in WorldManager.instance.player.transform)
		{
			if (child.GetComponent<Renderer>() != null)
			{
				if (child.name == "Player") child.GetComponent<Renderer>().material.color = Color.white;
			}
			if (child.childCount > 0)
			{
				foreach (Transform subchild in child)
				{
					if (subchild.name == "Player") subchild.GetComponent<Renderer>().material.color = Color.white;
				}
			}
		}
	}
}
