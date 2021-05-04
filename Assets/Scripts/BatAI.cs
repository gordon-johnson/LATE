using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatAI : MonoBehaviour
{
	public float movementSpeed = 0.5f;
	public GameObject wings;
	public ParticleSystem ps;

	private bool flyaway = false;
	private bool flapped = false;
	private int up = 1;
	private int hit = 0;
	private ParticleSystem.Particle[] particles;
	private Vector3 closetParticle = Vector3.zero;

	void Update()
    {
		if (!flyaway)
		{
			findClosestParicle();
			Vector3 temp = (closetParticle - transform.localPosition);
			temp = ((movementSpeed * 0.1f) / temp.magnitude) * temp;
			transform.localPosition += temp;
		}
		else
		{
			transform.localPosition += new Vector3(1f, 1f, 0f) * 0.1f;
		}
		
		if (!flapped)
		{
			flapped = true;
			if (up == 1)
			{
				wings.transform.localScale = new Vector3(1f, 0.5f, 1f);
			}
			else
			{
				wings.transform.localScale = new Vector3(0.75f, 1f, 1f);
			}
			StartCoroutine("flap");
		}
	}

	private void findClosestParicle()
	{
		if (particles == null || particles.Length < ps.main.maxParticles)
		{
			particles = new ParticleSystem.Particle[ps.main.maxParticles];
		}

		int numParticlesAlive = ps.GetParticles(particles);
		float minDist = Mathf.Infinity;

		for (int i = 0; i < numParticlesAlive; ++i)
		{
			float dist = Vector3.Distance(transform.localPosition, particles[i].position);
			if (dist < minDist)
			{
				closetParticle = particles[i].position;
				closetParticle = new Vector3(closetParticle.x, 0.5f, -closetParticle.y);
				minDist = dist;
			}
		}

		ps.SetParticles(particles, numParticlesAlive);

		if (minDist == Mathf.Infinity && !flyaway)
		{
			flyaway = true;
		}
	}

	IEnumerator flap()
	{
		yield return new WaitForSeconds(0.1f);
		flapped = false;
		up *= -1;

		if (transform.localPosition.y >= 5)
		{
			gameObject.SetActive(false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Net")
		{
			++hit;
			StartCoroutine("flashOnTrigger");
			if (hit >= 5)
			{
				flyaway = true;
			}
		}
	}

	IEnumerator flashOnTrigger()
	{
		GameObject parent = gameObject;
		foreach (Transform child in transform)
		{
			if (child.GetComponent<Renderer>() != null)
			{
				child.GetComponent<Renderer>().material.color = Color.red;
			}
			if (child.childCount > 0)
			{
				foreach (Transform subchild in child)
				{
					subchild.GetComponent<Renderer>().material.color = Color.red;
				}
			}
		}
		yield return new WaitForSeconds(0.1f);
		foreach (Transform child in transform)
		{
			if (child.GetComponent<Renderer>() != null)
			{
				child.GetComponent<Renderer>().material.color = Color.black;
			}
			if (child.childCount > 0)
			{
				foreach (Transform subchild in child)
				{
					subchild.GetComponent<Renderer>().material.color = Color.black;
				}
			}
		}
	}
}
