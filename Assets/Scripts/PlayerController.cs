using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float movementSpeed = 5.0f;
	public Rigidbody rb;
	public GameObject net;
	public GameObject netting;

	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		float moveVertical = Input.GetAxisRaw("Vertical");

		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

		if (moveHorizontal > 0 && moveVertical > 0)
		{
			movement /= moveHorizontal;
		}

		rb.position += movement * movementSpeed * Time.deltaTime;

		Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);
		Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
		float angle = Mathf.Atan2(mouseOnScreen.y - positionOnScreen.y, mouseOnScreen.x - positionOnScreen.x) * Mathf.Rad2Deg;
		rb.transform.rotation = Quaternion.Euler(new Vector3(0f, -angle - 90, 0f));
	}
}
