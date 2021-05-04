using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldManager : MonoBehaviour
{
	public static WorldManager instance;

	public GameObject player;
	public GameObject net;
	public GameObject startingRoom;
	public GameObject numFireflyText;
	public GameObject startText;
	public GameObject endText;
	public GameObject Text;
	public int numFirefly = 0;
	public int fireflySpawnChance;
	public int batSpawnChance;
	public Light jar;
	public bool enableDeath = false;

	private bool gameover = false;
	private bool setFogActive = false;
	private bool fogIsActive = false;
	private bool displayText = false;
	private Dictionary<int, GameObject> grid = new Dictionary<int, GameObject>();
	private float targetDensity = 0.15f;
	private float playerBaseSpeed;
	private GameObject roomPrefab;
	private GameObject treePrefab;
	private GameObject grassPrefab;
	private GameObject fireflyPrefab;
	private GameObject batPrefab;
	private GameObject wolfPrefab;

	private RectTransform rt;
	private Vector2 targetPos;
	private Vector2 originalPos;
	private float messageTimer = 4.0f;
	public Queue<string> messages = new Queue<string>();
	public bool displayedFogMessage = false;
	private bool displayedWarningMessage = false;
	public List<GameObject> enemies = new List<GameObject>();
	public bool GODMODE = false;
	private bool gamestart = false;
	public AudioClip pickup;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}

		// Initialize audio
		GetComponent<AudioSource>().playOnAwake = false;
		GetComponent<AudioSource>().clip = pickup;
	}

	private void Start()
    {
		// Initialize UI
		startText.SetActive(true);
		numFireflyText.SetActive(false);
		endText.SetActive(false);
		rt = Text.GetComponent<RectTransform>();
		targetPos = new Vector2(0.0f, -60.0f);
		originalPos = rt.anchoredPosition;

		// Load prefabs
		roomPrefab = Resources.Load("Room") as GameObject;
		treePrefab = Resources.Load("Tree") as GameObject;
		grassPrefab = Resources.Load("GrassPatch") as GameObject;
		fireflyPrefab = Resources.Load("Fireflies") as GameObject;
		batPrefab = Resources.Load("Bat") as GameObject;
		wolfPrefab = Resources.Load("Wolf") as GameObject;

		// Initialize fog
		RenderSettings.fog = false;
		RenderSettings.fogDensity = 0.0f;

		// Initilize other
		playerBaseSpeed = player.GetComponent<PlayerController>().movementSpeed / 2f;
		jar.intensity = 1.0f / 25.0f;
		jar.range = 1.0f / 10.0f;

		// Initialize world
		grid[1] = NewRoom(new Vector3(-10, 0, 10), "1");
		grid[2] = NewRoom(new Vector3(0, 0, 10), "2");
		grid[3] = NewRoom(new Vector3(10, 0, 10), "3");
		grid[4] = NewRoom(new Vector3(-10, 0, 0), "4");
		grid[5] = startingRoom;
		grid[6] = NewRoom(new Vector3(10, 0, 0), "6");
		grid[7] = NewRoom(new Vector3(-10, 0, -10), "7");
		grid[8] = NewRoom(new Vector3(0, 0, -10), "8");
		grid[9] = NewRoom(new Vector3(10, 0, -10), "9");

		player.GetComponent<PlayerController>().enabled = false;
		net.GetComponent<BoxCollider>().enabled = false;
		net.GetComponent<Net>().enabled = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (!GODMODE) GODMODE = true;
			else if (GODMODE) GODMODE = false;
		}

		if (!gamestart && Input.GetKeyDown(KeyCode.Return))
		{
			gamestart = true;
			player.GetComponent<PlayerController>().enabled = true;
			net.GetComponent<BoxCollider>().enabled = true;
			net.GetComponent<Net>().enabled = true;

			// UI input
			messages.Enqueue("use WASD to MOVE");
			messages.Enqueue("use the MOUSE to ROTATE");
			messages.Enqueue("and CLICK to swing your NET");
		}

		if (gamestart)
		{
			startText.SetActive(false);
		}

		UI();

		if (setFogActive)
		{
			ActivateFog();
		}
		if (fogIsActive)
		{
			// Decrease fog density with number of fireflies caught
			RenderSettings.fogDensity
				= targetDensity - numFirefly * (targetDensity / 100f);

			// Increase player movement speed with number of fireflies caught
			player.GetComponent<PlayerController>().movementSpeed
				= playerBaseSpeed + (numFirefly / 37.5f);

			jar.intensity = (float)numFirefly / 25f;
			jar.range = (float)numFirefly / 10f;

			numFireflyText.GetComponent<Text>().text = "Collect Fireflies: "
				 + numFirefly.ToString() + " / 100";

			if (numFirefly > 0)
			{
				enableDeath = true;
			}
			if (enableDeath && numFirefly <= 0)
			{
				gameover = true;
			}
			if (gameover)
			{
				player.GetComponent<PlayerController>().enabled = false;
				numFireflyText.SetActive(false);
			}
		}
		else
		{
			numFirefly = 0;
		}
	}

	private void ActivateFog()
	{
		// UI input
		if (!displayedFogMessage)
		{
			messageTimer = 3.0f;
			messages.Enqueue("Oh no...");
			messages.Enqueue("...the fog is rising...");
			messages.Enqueue("...and with it comes the wolves.");
			messages.Enqueue("Can you gather enough light to dispell the fog...");
			messages.Enqueue("...and find your way back to camp?");
			displayedFogMessage = true;
		}
		
		// Activate fog
		RenderSettings.fog = true;
		RenderSettings.fogDensity += (targetDensity - RenderSettings.fogDensity) * 0.01f;

		// Slow player movement
		player.GetComponent<PlayerController>().movementSpeed
			+= (playerBaseSpeed - player.GetComponent<PlayerController>().movementSpeed) * 0.005f;
	}

	private GameObject NewRoom(Vector3 mPos, string mTag)
	{
		// Initialize room
		GameObject newRoom = Instantiate(roomPrefab, mPos, Quaternion.identity, gameObject.transform);
		newRoom.tag = mTag;

		// Spawn trees
		for (int i = 0; i < Random.Range(2, 5); ++i)
		{
			GameObject tree = Instantiate(treePrefab, Vector3.zero, Quaternion.identity, newRoom.transform);
			tree.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
		}

		// Spawn fireflies
		GameObject firefly = null;
		if (Random.Range(0, 100) <= fireflySpawnChance)
		{
			firefly = Instantiate(fireflyPrefab, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f), newRoom.transform);
			firefly.transform.localPosition = new Vector3(0f, 0.5f, 0f);
			firefly.GetComponent<ParticleSystem>().emissionRate = 2f * Mathf.Ceil((numFirefly + 1) / 50f);
		}

		// Spawn bats, if fireflies
		if (firefly)
		{
			for (int i = 0; i < Random.Range(2, 5) * Mathf.Ceil((numFirefly + 1) / 30f); ++i)
			{
				if (Random.Range(0, 100) <= batSpawnChance)
				{
					GameObject bat = Instantiate(batPrefab, Vector3.zero, Quaternion.identity, newRoom.transform);
					bat.GetComponent<BatAI>().ps = firefly.GetComponent<ParticleSystem>();
					bat.transform.localPosition = new Vector3(Random.Range(-4f, 4f), 5f, Random.Range(-4f, 4f));
				}
			}
		}	

		return newRoom;
	}

	private void CycleRooms(int i, int j, int k)
	{
		Vector3 tempPos = grid[i].transform.position;
		Vector3 victPos = grid[k].transform.position;
		string tempTag = grid[i].tag;
		string victTag = grid[k].tag;

		// Initialize fog when starting room is destroyed
		if (grid[k] == startingRoom)
		{
			setFogActive = true;
		}

		Destroy(grid[k]);

		// Cylce rooms
		grid[i].transform.position = grid[j].transform.position;
		grid[i].tag = grid[j].tag;

		grid[j].transform.position = victPos;
		grid[j].tag = victTag;

		GameObject newRoom = NewRoom(tempPos, tempTag);

		// Reindex rooms
		grid[k] = grid[j];
		grid[j] = grid[i];
		grid[i] = newRoom;
	}

	public void UpdateWorld(int index)
	{
		// Spawn wolves
		if (RenderSettings.fog == true)
		{
			if (Random.Range(11, 20) <= numFirefly)
			{
				enemies.Add(Instantiate(wolfPrefab, new Vector3(player.transform.position.x + 5f * (Random.Range(0, 2) * 2 - 1), -0.5f, player.transform.position.z + 5f * (Random.Range(0, 2) * 2 - 1)), Quaternion.identity));

				if (!displayedWarningMessage)
				{
					messages.Enqueue("Don't let your light be extinguished...");
					displayedWarningMessage = true;
				}

				if (numFirefly >= 30)
				{
					enemies.Add(Instantiate(wolfPrefab, new Vector3(player.transform.position.x + 5f * (Random.Range(0, 2) * 2 - 1), -0.5f, player.transform.position.z + 5f * (Random.Range(0, 2) * 2 - 1)), Quaternion.identity));
				}
				if (numFirefly >= 50)
				{
					enemies.Add(Instantiate(wolfPrefab, new Vector3(player.transform.position.x + 5f * (Random.Range(0, 2) * 2 - 1), -0.5f, player.transform.position.z + 5f * (Random.Range(0, 2) * 2 - 1)), Quaternion.identity));
				}
				if (numFirefly >= 70)
				{
					enemies.Add(Instantiate(wolfPrefab, new Vector3(player.transform.position.x + 5f * (Random.Range(0, 2) * 2 - 1), -0.5f, player.transform.position.z + 5f * (Random.Range(0, 2) * 2 - 1)), Quaternion.identity));
				}
				if (numFirefly >= 90)
				{
					enemies.Add(Instantiate(wolfPrefab, new Vector3(player.transform.position.x + 5f * (Random.Range(0, 2) * 2 - 1), -0.5f, player.transform.position.z + 5f * (Random.Range(0, 2) * 2 - 1)), Quaternion.identity));
				}
			}
		}

		switch (index)
		{
			case 1:
				UpdateWorld(2);
				UpdateWorld(4);
				break;

			case 2:
				player.transform.position += Vector3.back * 10;
				foreach (GameObject e in enemies) e.transform.position += Vector3.back * 10;
				CycleRooms(1, 4, 7);
				CycleRooms(2, 5, 8);
				CycleRooms(3, 6, 9);
				break;

			case 3:
				UpdateWorld(2);
				UpdateWorld(6);
				break;

			case 4:
				player.transform.position += Vector3.right * 10;
				foreach (GameObject e in enemies) e.transform.position += Vector3.right * 10;
				CycleRooms(1, 2, 3);
				CycleRooms(4, 5, 6);
				CycleRooms(7, 8, 9);
				break;

			case 5:
				break;

			case 6:
				player.transform.position += Vector3.left * 10;
				foreach (GameObject e in enemies) e.transform.position += Vector3.left * 10;
				CycleRooms(3, 2, 1);
				CycleRooms(6, 5, 4);
				CycleRooms(9, 8, 7);
				break;

			case 7:
				UpdateWorld(8);
				UpdateWorld(4);
				break;

			case 8:
				player.transform.position += Vector3.forward * 10;
				foreach (GameObject e in enemies) e.transform.position += Vector3.forward * 10;
				CycleRooms(7, 4, 1);
				CycleRooms(8, 5, 2);
				CycleRooms(9, 6, 3);
				break;

			case 9:
				UpdateWorld(8);
				UpdateWorld(6);
				break;

			default:
				break;
		}
	}

	private void UI()
	{
		if (messages.Count > 0 && !displayText)
		{
			displayText = true;
			StartCoroutine("DisplayText");
		}
		if (displayText)
		{
			rt.anchoredPosition += (targetPos - rt.anchoredPosition) * 0.1f;
		}
		else
		{
			rt.anchoredPosition += (originalPos - rt.anchoredPosition) * 0.1f;
		}

		if (!gameover && numFirefly >= 100)
		{
			RenderSettings.fog = false;
			endText.GetComponent<Text>().text = "YOU WIN";
			endText.SetActive(true);
			gameover = true;
			GODMODE = true;
			foreach (GameObject e in enemies)
			{
				e.SetActive(false);
			}
		}
		if (gameover && numFirefly <= 0)
		{
			endText.GetComponent<Text>().text = "YOU LOSE";
			endText.SetActive(true);
		}
		if ((gameover && Input.GetKeyDown(KeyCode.Return)) || Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	private IEnumerator DisplayText()
	{
		Text.GetComponent<Text>().text = messages.Dequeue();
		yield return new WaitForSeconds(messageTimer);
		displayText = false;

		if (Text.GetComponent<Text>().text == "...and find your way back to camp?")
		{
			numFireflyText.SetActive(true);
			setFogActive = false;
			numFirefly = 0;
			fogIsActive = true;
		}
	}
}
