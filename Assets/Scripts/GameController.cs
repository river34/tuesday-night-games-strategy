using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class States {
	public static int INIT = 0;
	public static int TITLE = 10;
	public static int INIT_DAY = 20;
	public static int SELECT = 30;
	public static int UPGRADE = 40;
	public static int SETUP = 50;
	public static int DAY = 60;
	public static int DAY_TUT0 = 70;
	public static int DAY_TUT1 = 71;
	public static int DAY_TUT2 = 72;
	public static int DAY_TUT3 = 73;
	public static int DAY_TUT4 = 74;
	public static int DAY_TUT5 = 75;
	public static int DAY_TUT6 = 76;
	public static int DAY_TUT7 = 77;
	public static int FINISH_DAY = 90;
	public static int RESTART_DAY = 100;
}

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	public int exp;
	public int day;

	// controls
	public AgentManager agentManager;
	public List<GameObject> spirits;
	public List<GameObject> ghosts;
	public List<GameObject> trees;
	public List<GameObject> roll = new List<GameObject> ();
	public DayMeta dayMeta;
	public List<AgentMeta> todaysSpirits;
	public List<AgentMeta> todaysGhosts;
	public List<AgentMeta> todaysTrees;
	public int state;
	public int last_state;
	public int next_state;
	public float time;
	private float starttime;
	private float spiritSpawnInterval = 1;
	private float ghostSpawnInterval = 1;
	private float lastSpiritSpawnTime;
	private float lastGhostSpawnTime;
	private GameObject agentObject;

	// UI
	public GameObject TitleUI;
	public GameObject UpgradeUI;
	public GameObject SetupUI;
	public GameObject RollUI;
	public GameObject ExpUI;
	private int rollNum;
	private int rollTotal;

	// numbers
	private int numOfSpirits;
	private int numOfGhosts;
	private int minCost = 0;
	public int numOfGhostsLeft;

	// tutorials
	public GameObject Tutorial0;
	public GameObject Tutorial1;
	public GameObject Tutorial2;
	public GameObject Tutorial3;
	public GameObject Tutorial4;
	public GameObject Tutorial5;
	public GameObject Tutorial6;
	public GameObject Tutorial7;
	private bool check1 = false;
	private bool check2 = false;
	private bool check3 = false;
	private bool check4 = false;
	private bool check5 = false;
	private bool check6 = false;
	private bool continueGame = false;

	// moon
	public GameObject moon;

	// roll
	public List<GameObject> allRolls = new List<GameObject> ();
	public Dictionary<string, GameObject> rollBook = new Dictionary<string, GameObject> ();

	// upgrade
	public List<GameObject> allUpgrades = new List<GameObject> ();
	public Dictionary<string, GameObject> upgradeBook = new Dictionary<string, GameObject> ();

	// lock
	public bool rollLocked;
	public bool expLocked;

	void Awake ()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);

		agentManager = GetComponent <AgentManager> ();

		foreach (GameObject roll in allRolls)
		{
			if (!rollBook.ContainsKey (roll.name))
			{
				rollBook.Add (roll.name, roll);
			}
		}

		foreach (GameObject upgrade in allUpgrades)
		{
			if (!upgradeBook.ContainsKey (upgrade.name))
			{
				upgradeBook.Add (upgrade.name, upgrade);
			}
		}

		Init ();
	}

	void PlaceMoon ()
	{
		float angle = (day-1) % 31 * Mathf.PI / 180 * 6;
		moon.transform.position = new Vector3 (Mathf.Sin (-Mathf.PI/2 + angle) * 3, Mathf.Sin (angle) - 0.3f, 0);
		// day ++;
	}

	void PlaceMoon (int day)
	{
		float angle = (day-1) % 31 * Mathf.PI / 180 * 6;
		moon.transform.position = new Vector3 (Mathf.Sin (-Mathf.PI/2 + angle) * 3, Mathf.Sin (angle) - 0.3f, 0);
	}

	public void Init ()
	{
		exp = 0;
		day = 1;

		// test only
		// day = 18;
		// InvokeRepeating ("PlaceMoon", 0.1f, 0.2f);

		rollNum = day + 2;
		if (rollNum > 8)
		{
			rollNum = 8;
		}

		agentManager.Init ();

		next_state = States.INIT;
	}

	public void FinishDay ()
	{
		day ++;
		if (rollNum < 8)
		{
			rollNum ++;
		}

		next_state = States.FINISH_DAY;
	}

	public void RestartDay ()
	{
		next_state = States.RESTART_DAY;
	}

	public void InitDay (int day)
	{
		PlaceMoon (day);

		dayMeta = agentManager.GetDayMeta (day);

		if (dayMeta != null)
		{
			rollTotal = 0;
			SetupUI.SetActive (true);
			SetupUI.transform.Find ("Text").GetComponent <Text> ().text = "DAY " + day;
			todaysSpirits = dayMeta.spiritMeta;
			todaysGhosts = dayMeta.ghostMeta;
			todaysTrees = dayMeta.treeMeta;
			numOfSpirits = dayMeta.numOfSpirits;
			numOfGhosts = dayMeta.numOfGhosts;
			numOfGhostsLeft = numOfGhosts;

			if (todaysSpirits != null && todaysSpirits.Count > 0)
			{
				for (int i = 0; i < todaysSpirits.Count; i++)
				{
					if (agentManager.spiritBook.ContainsKey (todaysSpirits[i].name))
					{
						agentManager.spiritBook [todaysSpirits[i].name].GetComponent <Agent> ().ExportToMeta (ref todaysSpirits, i);
						if (i == 0)
						{
							minCost = todaysSpirits[0].cost;
						}
						else
						{
							minCost = Mathf.Min (minCost, todaysSpirits[i].cost);
						}
					}
				}
			}

			if (exp >= minCost)
			{
				StartCoroutine (StartState (States.UPGRADE, 3f));
			}
			else
			{
				next_state = States.SETUP;
			}
		}
		else
		{
			// no more day to setup
		}
	}

	public void SetupDay ()
	{
		if (todaysSpirits.Count > 0)
		{
			if (rollTotal < numOfSpirits - 10)
			{
				spirits = agentManager.CreateRoll (10, todaysSpirits);
				rollTotal += 10;
			}
			else
			{
				spirits = agentManager.CreateRoll (numOfSpirits - rollTotal, todaysSpirits);
				rollTotal = numOfSpirits;
			}
		}

		if (todaysGhosts != null && todaysGhosts.Count > 0)
		{
			ghosts = agentManager.CreateRoll (numOfGhosts, todaysGhosts);
		}

		if (todaysTrees != null)
		{
			trees = agentManager.CreateTrees (todaysTrees);
		}

		roll.Clear ();

		ExpUI.GetComponent <Text> ().text = "EXP " + exp;

		StartCoroutine (StartDay (day, 3f));
	}

	IEnumerator StartState (int state, float delay)
	{
		yield return new WaitForSeconds (delay);

		next_state = States.UPGRADE;
	}

	IEnumerator StartDay (int day, float delay)
	{
		yield return new WaitForSeconds (delay);

		SetupUI.SetActive (false);

		time = Time.time;
		lastSpiritSpawnTime = -1;
		lastGhostSpawnTime = -1;

		next_state = States.DAY;
	}

	public IEnumerator AttemptRemoveRoll (GameObject rollObject)
	{
		if (rollLocked)
		{
			yield return new WaitForSeconds (0.1f);
		}
		roll.Remove (rollObject);
	}

	public IEnumerator AttemptAddExp (int gain)
	{
		if (expLocked)
		{
			yield return new WaitForSeconds (0.1f);
		}
		expLocked = true;
		exp += gain;
		ExpUI.GetComponent <Text> ().text = "EXP " + exp;
		expLocked = false;
	}

	public void EndDay (string tag)
	{
		// delete all agents
		Destroy (GameObject.Find ("Agents"));

		// delete all roll
		foreach (Transform child in RollUI.transform) {
			Destroy (child.gameObject);
		}

		if (tag == "Spirit")
		{
			// Spirit won, move to the next day
			FinishDay ();
		}
		else if (tag == "Ghost")
		{
			// Ghost won, restart the day
			RestartDay ();
		}
	}

	public void EndDayLoss (string tag)
	{
		// delete all agents
		Destroy (GameObject.Find ("Agents"));

		// delete all roll
		foreach (Transform child in RollUI.transform) {
			Destroy (child.gameObject);
		}

		if (tag == "Spirit")
		{
			// Ghost won, restart the day
			RestartDay ();
		}
		else if (tag == "Ghost")
		{
			// Spirit won, move to the next day
			FinishDay ();
		}
	}

	void Day ()
	{
		if (ghosts != null && ghosts.Count > 0)
		{
			if (lastGhostSpawnTime == -1 || Time.time - lastGhostSpawnTime >= ghostSpawnInterval)
			{
				lastGhostSpawnTime = Time.time;
				GameObject ghost = ghosts[0];
				ghosts.RemoveAt (0);
				ghost.SetActive (true);
				ghost.GetComponent <Agent> ().game = this;
				ghost.GetComponent <Agent> ().Move ();
				ghostSpawnInterval = Random.Range (0.3f, 3f);
			}
		}

		if (trees != null && trees.Count > 0)
		{
			foreach (GameObject tree in trees)
			{
				if (tree != null)
				{
					tree.GetComponent <Agent> ().game = this;
				}
			}
		}

		if (spirits != null && spirits.Count > 0)
		{
			if (lastSpiritSpawnTime == -1 || Time.time - lastSpiritSpawnTime >= spiritSpawnInterval)
			{
				if (roll.Count < rollNum)
				{
					lastSpiritSpawnTime = Time.time;
					GameObject spirit = spirits[0];
					spirits.RemoveAt (0);
					GameObject rollObject = null;
					if (spirit == null)
					{
						return;
					}
					if (rollBook.ContainsKey (spirit.name + " Roll"))
					{
						rollObject = Instantiate (rollBook [spirit.name + " Roll"], RollUI.transform, false);
					}
					if (rollObject != null)
					{
						rollObject.GetComponent <Roll> ().game = this;
						rollObject.GetComponent <Roll> ().agentObject = spirit;
						roll.Add (rollObject);
					}
				}
			}
		}
		else
		{
			if (rollTotal < numOfSpirits - 10)
			{
				spirits = agentManager.CreateRoll (10, todaysSpirits);
				rollTotal += 10;
			}
			else
			{
				spirits = agentManager.CreateRoll (numOfSpirits - rollTotal, todaysSpirits);
				rollTotal = numOfSpirits;
			}
		}

		if (roll.Count > 0)
		{
			int index = 0;
			rollLocked = true;
			foreach (GameObject rollObject in roll)
			{
				if (rollObject == null)
				{
					return;
				}
				Vector3 position = rollObject.GetComponent <RectTransform> ().localPosition;
				if (position.x > - 80 * rollNum/2 + 40 + 80 * index)
				{
					rollObject.GetComponent <RectTransform> ().localPosition += 100 * Vector3.left * Time.deltaTime;
				}
				else
				{
					rollObject.GetComponent <RectTransform> ().localPosition = new Vector3 (- 80 * rollNum/2 + 40 + 80 * index, position.y, position.z);
				}
				index ++;
			}
			rollLocked = false;
		}

		// tutorials - check points
		// when the first ghost comes, display tutorial 0
		if (!check1)
		{
			GameObject firstGhost = GameObject.FindWithTag ("Ghost");
			if (firstGhost != null)
			{
				if (firstGhost.transform.position.x > -4)
				{
					check1 = true;
					next_state = States.DAY_TUT0;
				}
			}
		}

		if (!check2)
		{
			if (exp > 0)
			{
				check2 = true;
				next_state = States.DAY_TUT3;
			}
		}

		if (!check3)
		{
			GameObject firstSpirit = GameObject.Find ("Stone Spirit");
			if (firstSpirit != null)
			{
				if (firstSpirit.transform.position.x < 4)
				{
					check3 = true;
					next_state = States.DAY_TUT4;
				}
			}
		}

		if (!check4)
		{
			GameObject firstSpirit = GameObject.Find ("Durio Spirit");
			if (firstSpirit != null)
			{
				if (firstSpirit.transform.position.x < 4)
				{
					check4 = true;
					next_state = States.DAY_TUT5;
				}
			}
		}

		if (!check5)
		{
			GameObject firstSpirit = GameObject.Find ("Long Grass Spirit");
			if (firstSpirit != null)
			{
				if (firstSpirit.transform.position.x < 4)
				{
					check5 = true;
					next_state = States.DAY_TUT6;
				}
			}
		}

		if (!check6)
		{
			GameObject firstSpirit = GameObject.Find ("Butterfly Spirit");
			if (firstSpirit != null)
			{
				if (firstSpirit.transform.position.x < 4)
				{
					check6 = true;
					next_state = States.DAY_TUT7;
				}
			}
		}
	}

	void Select ()
	{

	}

	void Upgrade ()
	{
		if (!UpgradeUI.activeSelf)
		{
			UpgradeUI.SetActive (true);
		}

		if (todaysSpirits != null && todaysSpirits.Count > 0)
		{
			GameObject upgradeObject;
			List<GameObject> upgradeObjects = new List<GameObject> ();
			for (int i = 0; i < todaysSpirits.Count; i++)
			{
				if (agentManager.spiritBook.ContainsKey (todaysSpirits[i].name) && upgradeBook.ContainsKey (todaysSpirits[i].name + " Upgrade"))
				{
					upgradeObject = Instantiate (upgradeBook [todaysSpirits[i].name + " Upgrade"], UpgradeUI.transform.Find ("Holder"), false);
					upgradeObject.GetComponent <Upgrade> ().game = this;
					upgradeObject.GetComponent <Upgrade> ().agentMeta = todaysSpirits[i];
					upgradeObjects.Add (upgradeObject);
				}
			}

			if (upgradeObjects.Count > 0)
			{
				int whole = (int) Mathf.Floor (upgradeObjects.Count / 5) * 5;
				int rest = upgradeObjects.Count - whole;
				for (int i = 0; i < upgradeObjects.Count; i++)
				{
					float x = 0;
					float y = -120 * (int) Mathf.Floor (i / 5);
					if (x < whole)
					{
						x = -240 + 120 * (i % 5);
					}
					else
					{
						if (rest == 1)
						{
							x = 0;
						}
						else if (rest == 2)
						{
							x = -60 + 120 * i;
						}
						else if (rest == 3)
						{
							x = -120 + 120 * i;
						}
						else if (rest == 4)
						{
							x = -180 + 120 * i;
						}
					}
					upgradeObjects[i].GetComponent <RectTransform> ().localPosition = new Vector3 (x, y, 0);
				}
			}
		}
	}

	public void Submit (string type)
	{
		if (type == "Upgrade")
		{
			if (UpgradeUI.activeSelf)
			{
				foreach (Transform child in UpgradeUI.transform.Find ("Holder"))
				{
					Destroy (child.gameObject);
				}
				UpgradeUI.SetActive (false);
			}
			next_state = States.SETUP;
		}
	}

	void Update ()
	{
		// keys
		if (Input.GetKey ("1"))
		{
			check1 = true;
			check2 = true;
			check3 = true;
			check4 = true;
			check5 = true;
			check6 = true;
		}

		if (state == States.INIT)
		{
			starttime = Time.time;
			next_state = States.TITLE;
		}

		else if (state == States.TITLE)
		{
			if (last_state != state)
			{
				TitleUI.SetActive (true);
			}

			if (Time.time - starttime > 2 && Input.anyKey)
			{
				next_state = States.INIT_DAY;
			}
		}

		else if (state == States.INIT_DAY)
		{
			if (last_state != state)
			{
				TitleUI.SetActive (false);
				InitDay (day);
			}
		}

		else if (state == States.SELECT)
		{
			if (last_state != state)
			{
				Select ();
			}
		}

		else if (state == States.UPGRADE)
		{
			if (last_state != state)
			{
				Upgrade ();
			}
		}

		else if (state == States.SETUP)
		{
			if (last_state != state)
			{
				SetupDay ();
			}
		}

		else if (state == States.DAY)
		{
			Day ();
		}

		else if (state == States.DAY_TUT0)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial0.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial0.SetActive (false);
				StartCoroutine (StartTutorial (1, 1));
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT1)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial1.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial1.SetActive (false);
				StartCoroutine (StartTutorial (2, 1));
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT2)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial2.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial2.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT3)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial3.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial3.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT4)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial4.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial4.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT5)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial5.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial5.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT6)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial6.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial6.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.DAY_TUT7)
		{
			if (last_state != state)
			{
				PauseGame ();
				Tutorial7.SetActive (true);
			}

			if (continueGame)
			{
				ContinueGame ();
				Tutorial7.SetActive (false);
				next_state = States.DAY;
			}
		}

		else if (state == States.FINISH_DAY)
		{
			next_state = States.INIT_DAY;
		}

		else if (state == States.RESTART_DAY)
		{
			next_state = States.INIT_DAY;
		}
	}

	void LateUpdate ()
	{
		last_state = state;
		state = next_state;
	}

	private void PauseGame ()
    {
        Time.timeScale = 0;
		continueGame = false;
		StartCoroutine (StartContinueGame (2));
	}

	private void ContinueGame ()
    {
        Time.timeScale = 1;
    }

	IEnumerator StartContinueGame (float delay)
	{
		float pauseEndTime = Time.realtimeSinceStartup + delay;
	    while (Time.realtimeSinceStartup < pauseEndTime)
	    {
	        yield return 0;
	    }
		continueGame = true;
	}

	IEnumerator StartTutorial (int id, float delay)
	{
		yield return new WaitForSeconds (delay);
		if (id == 1)
		{
			next_state = States.DAY_TUT1;
		}
		else if (id == 2)
		{
			next_state = States.DAY_TUT2;
		}
	}
}
