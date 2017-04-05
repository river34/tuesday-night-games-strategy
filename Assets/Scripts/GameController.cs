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
	public static int FINISH_DAY = 70;
	public static int RESTART_DAY = 80;
}

public class GameController : MonoBehaviour {

	public static GameController instance = null;
	public int exp;
	public int day;

	// controls
	public AgentManager agentManager;
	public List<GameObject> spirits;
	public List<GameObject> ghosts;
	public GameObject tree;
	public List<GameObject> roll = new List<GameObject> ();
	public DayMeta dayMeta;
	public List<AgentMeta> todaysSpirits;
	public List<AgentMeta> todaysGhosts;
	public AgentMeta todaysTree;
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
	private int numOfSpirits;
	private int numOfGhosts;
	private int minCost = 0;

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

	public void Init ()
	{
		exp = 0;
		day = 1;

		// test only
		// day = 6;

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
		dayMeta = agentManager.GetDayMeta (day);

		if (dayMeta != null)
		{
			rollTotal = 0;
			SetupUI.SetActive (true);
			SetupUI.transform.Find ("Text").GetComponent <Text> ().text = "DAY " + day;
			todaysSpirits = dayMeta.spiritMeta;
			todaysGhosts = dayMeta.ghostMeta;
			todaysTree = dayMeta.treeMeta;
			numOfSpirits = dayMeta.numOfSpirits;
			numOfGhosts = dayMeta.numOfGhosts;

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

		if (todaysTree != null)
		{
			tree = agentManager.CreateTree (todaysTree);
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
				ghostSpawnInterval = Random.Range (0.5f, 1f);
			}
		}

		if (tree != null)
		{
			tree.GetComponent <Agent> ().game = this;
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
}
