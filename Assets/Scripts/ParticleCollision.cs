using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	public float fireflyTimeOut = 5f;

	private bool collision = false;

	private void OnParticleCollision(GameObject other)
	{
		if (other.tag == "Player")
		{
			if (!collision)
			{
				collision = true;
				StartCoroutine("endFirefliesOnTimeOut");
			}
			++WorldManager.instance.numFirefly;
			WorldManager.instance.GetComponent<AudioSource>().Play();
		}

	}
	
	IEnumerator endFirefliesOnTimeOut()
	{
		yield return new WaitForSeconds(fireflyTimeOut);
		GetComponent<ParticleSystem>().emissionRate = 0;
	}
}
