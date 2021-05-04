using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour
{
	private Transform net;
	public GameObject netCollider;

    private void Start()
    {
		net = GetComponent<Transform>();
		netCollider.SetActive(false);
    }

	private void Update()
    {
		SwingNet();
    }

	private void SwingNet()
	{
		if (Input.GetMouseButton(0))
		{
			float yAxis = net.transform.localRotation.eulerAngles.y;
			yAxis += (90.0f - yAxis) * 0.75f;

			transform.localRotation = Quaternion.Euler(0.0f, yAxis, 0.0f);
			netCollider.SetActive(true);
		}
		if (Input.GetMouseButtonUp(0))
		{
			net.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			netCollider.SetActive(false);
		}
	}
}
