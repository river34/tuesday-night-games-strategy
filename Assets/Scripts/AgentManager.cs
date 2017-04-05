using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class AgentMeta {
	public string tag;
	public string name;
	public float damage = 0;			// damage per time (base)
	public float damageTime = 0;		// 1 / damageFrequency (base)
	public int num = 0;					// number of enemies it can attack once (base)
	public float speed = 0;				// move speed (m/frame) (base)
	public float health = 0;			// initial health (base)
	public float defense = 0;			// defense power (base)
	public int level = 1;
	public int rate = 0;
	public int cost = 0;
	public GameObject gameObject;

	public AgentMeta ()
	{

	}

	public AgentMeta (string _tag, string _name, float _damage, float _damageTime, int _num, float _speed, float _health, float _defense, int _level, int _rate, int _cost, GameObject _gameObject)
	{
		tag = _tag;
		name = _name;
		damage = _damage;
		damageTime = _damageTime;
		num = _num;
		speed = _speed;
		health = _health;
		defense = _defense;
		level = _level;
		rate = _rate;
		cost = _cost;
		gameObject = _gameObject;
	}

	public AgentMeta (string _tag, string _name, string _damage, string _damageTime, string _num, string _speed, string _health, string _defense, string _level, string _rate, string _cost, GameObject _gameObject)
	{
		tag = _tag;
		name = _name;
		damage = float.Parse (_damage);
		damageTime = float.Parse (_damageTime);
		num = int.Parse (_num);
		speed = float.Parse (_speed);
		health = float.Parse (_health);
		defense = float.Parse (_defense);
		level = int.Parse (_level);
		rate = int.Parse (_rate);
		cost = int.Parse (_cost);
		gameObject = _gameObject;
	}

	public void ExportFromPrefab ()
	{
		if (gameObject != null)
		{
			Agent agent = gameObject.GetComponent <Agent> ();
			if (tag == "") tag = agent.tag;
			if (name == "") name = agent.name;
			if (damage == -1) damage = agent.damage;
			if (damageTime == -1) damageTime = agent.damageTime;
			if (num == -1) num = agent.num;
			if (speed == -1) speed = agent.speed;
			if (health == -1) health = agent.health;
			if (defense == -1) defense = agent.defense;
			if (level == -1) level = agent.level;
			if (rate == -1) rate = agent.rate;
			if (cost == -1) cost = agent.cost;
		}
	}

	public void ExportToPrefab ()
	{
		if (gameObject != null)
		{
			Agent agent = gameObject.GetComponent <Agent> ();
			if (tag != "") agent.tag = tag;
			if (name != "") agent.name = name;
			if (damage != -1) agent.damage = damage;
			if (damageTime != -1) agent.damageTime = damageTime;
			if (num != -1) agent.num = num;
			if (speed != -1) agent.speed = speed;
			if (health != -1) agent.health = health;
			if (defense != -1) agent.defense = defense;
			if (level != -1) agent.level = level;
			if (rate != -1) agent.rate = rate;
			if (cost != -1) agent.cost = cost;
		}
	}

	public void CopyToAgent (ref GameObject gameObject)
	{
		Agent agent = gameObject.GetComponent <Agent> ();

		if (agent == null || agent.tag != tag || agent.name != name) return;

		if (damage != -1) agent.damage = damage;

		if (damageTime != -1) agent.damageTime = damageTime;

		if (num != -1) agent.num = num;

		if (speed != -1) agent.speed = speed;

		if (health != -1) agent.health = health;

		if (defense != -1) agent.defense = defense;

		if (rate != -1) agent.rate = rate;

		if (cost != -1) agent.cost = cost;
	}

	public void LevelUp (string _tag, ref int exp)
	{
		if (_tag != "Spirit" || tag != _tag) return;

		if (cost < 0) return;

		if (exp < cost) return;

		exp -= cost;

		LevelUp ();
	}

	public void LevelUp (ref AgentMeta meta)
	{
		if (meta.tag == "Ghost")
		{
			if (meta.defense != -1) meta.damage *= 1.1f;

			if (meta.health != -1) meta.health *= 1.1f;

			if (meta.defense != -1) meta.defense *= 1.05f;

			if (meta.defense > 0.7f) meta.defense = 0.7f;
		}

		if (meta.tag == "Spirit")
		{
			if (meta.defense != -1) meta.damage *= 1.1f;

			if (meta.health != -1) meta.health *= 1.1f;

			if (meta.defense != -1) meta.defense *= 1.05f;

			if (meta.defense > 0.7f) meta.defense = 0.7f;

			if (meta.cost != -1) meta.cost *= 2;
		}

		if (meta.tag == "Tree")
		{
			if (meta.health != -1) meta.health += 80;
		}

		meta.level += 1;

		ExportToPrefab ();
	}

	public void LevelUp ()
	{
		if (tag == "Ghost")
		{
			if (defense != -1) damage *= 1.1f;

			if (health != -1) health *= 1.1f;

			if (defense != -1) defense *= 1.05f;

			if (defense > 0.7f) defense = 0.7f;
		}

		if (tag == "Spirit")
		{
			if (defense != -1) damage *= 1.1f;

			if (health != -1) health *= 1.1f;

			if (defense != -1) defense *= 1.05f;

			if (defense > 0.7f) defense = 0.7f;

			if (cost != -1) cost *= 2;
		}

		if (tag == "Tree")
		{
			if (health != -1) health += 80;
		}

		level += 1;

		ExportToPrefab ();
	}

	public void LevelUp (int _level)
	{
		if (tag == "Ghost")
		{
			if (damage != -1) damage *= Mathf.Pow (1.1f, _level - level);

			if (health != -1) health *= Mathf.Pow (1.1f, _level - level);

			if (defense != -1)	defense *= Mathf.Pow (1.05f, _level - level);

			if (defense > 0.7f) defense = 0.7f;
		}

		if (tag == "Tree")
		{
			if (health != -1) health += 80 * (_level - level);
		}

		level = _level;

		ExportToPrefab ();
	}

	public AgentMeta NextDay ()
	{
		AgentMeta nextAgentMeta = new AgentMeta ();
		nextAgentMeta.tag = tag;
		nextAgentMeta.name = name;
		nextAgentMeta.damage = damage;
		nextAgentMeta.damageTime = damageTime;
		nextAgentMeta.num = num;
		nextAgentMeta.speed = speed;
		nextAgentMeta.health = health;
		nextAgentMeta.defense = defense;
		nextAgentMeta.level = level;
		nextAgentMeta.rate = rate;
		nextAgentMeta.gameObject = gameObject;

		if (nextAgentMeta.tag == "Ghost" || nextAgentMeta.tag == "Tree")
		{
			nextAgentMeta.LevelUp (ref nextAgentMeta);
		}

		if (nextAgentMeta.tag == "Ghost")
		{
			nextAgentMeta.rate ++;
		}

		return nextAgentMeta;
	}
}

public class DayMeta {
	public int day;
	public AgentMeta treeMeta;
	public List<AgentMeta> spiritMeta = new List<AgentMeta> ();
	public List<AgentMeta> ghostMeta = new List<AgentMeta> ();
	public int numOfSpirits;
	public int numOfGhosts;

	public DayMeta ()
	{

	}

	public DayMeta (int _day, AgentMeta _treeMeta, List<AgentMeta> _spiritMeta, List<AgentMeta> _ghostMeta, int _numOfSpirits, int _numOfGhosts)
	{
		day = _day;
		treeMeta = _treeMeta;
		spiritMeta = _spiritMeta;
		ghostMeta = _ghostMeta;
		numOfSpirits = _numOfSpirits;
		numOfGhosts = _numOfGhosts;
	}

	public DayMeta (int _day, List<AgentMeta> _spiritMeta, List<AgentMeta> _ghostMeta, int _numOfSpirits, int _numOfGhosts)
	{
		day = _day;
		spiritMeta = _spiritMeta;
		foreach (AgentMeta agentMeta in spiritMeta)
		{
			if (agentMeta.name == "Tree Spirit")
			{
				treeMeta = agentMeta;
				spiritMeta.Remove (treeMeta);
				break;
			}
		}
		ghostMeta = _ghostMeta;
		numOfSpirits = _numOfSpirits;
		numOfGhosts = _numOfGhosts;
	}

	public DayMeta NextDay ()
	{
		DayMeta nextDayMeta = new DayMeta ();
		nextDayMeta.day = day + 1;
		nextDayMeta.treeMeta = treeMeta;
		nextDayMeta.spiritMeta = spiritMeta;
		for (int i = 0; i < nextDayMeta.spiritMeta.Count; i++)
		{
			nextDayMeta.spiritMeta [i] = nextDayMeta.spiritMeta [i].NextDay ();
		}
		nextDayMeta.treeMeta = treeMeta.NextDay ();
		nextDayMeta.ghostMeta = ghostMeta;
		for (int i = 0; i < nextDayMeta.ghostMeta.Count; i++)
		{
			nextDayMeta.ghostMeta [i] = nextDayMeta.ghostMeta [i].NextDay ();
		}
		nextDayMeta.numOfSpirits = numOfSpirits + 6;
		nextDayMeta.numOfGhosts = numOfGhosts + 8;
		return nextDayMeta;
	}
}

public class AgentManager : MonoBehaviour {

	// tree
	public List<GameObject> allTrees;

	// spirit
	public List<GameObject> allSpirits;

	// ghost
	public List<GameObject> allGhosts;

	// contols
	public List<Agent> trees = new List<Agent> ();
	public List<Agent> spirits = new List<Agent> ();
	public List<Agent> ghosts = new List<Agent> ();
	public List<GameObject> recycle = new List<GameObject> ();
	public Transform agentHolder;

	// dictionary
	public Dictionary<string, GameObject> treeBook = new Dictionary<string, GameObject> ();
	public Dictionary<string, GameObject> spiritBook = new Dictionary<string, GameObject> ();
	public Dictionary<string, GameObject> ghostBook = new Dictionary<string, GameObject> ();

	public List<DayMeta> dayMeta = new List<DayMeta> ();

	public GameController game;

	void Awake ()
	{
		if (agentHolder == null)
		{
			GameObject agentObject = new GameObject ("Agents");
			agentHolder = agentObject.transform;
		}

		game = GetComponent <GameController> ();
	}

	void Update ()
	{

	}

	public void Init ()
	{
		trees.Clear ();
		treeBook.Clear ();
		spirits.Clear ();
		spiritBook.Clear ();
		ghosts.Clear ();
		ghostBook.Clear ();

		int index;

		// trees
		index = 1;
		foreach (GameObject tree in allTrees)
		{
			Agent agent = tree.GetComponent <Agent>();
			if (!treeBook.ContainsKey (agent.name))
			{
				treeBook.Add (agent.name, agent.gameObject);
				agent.id = index;
				// agent.level = 1;
				agent.agentManager = this;
				trees.Add (agent);
				index ++;
			}
		}

		// spirits
		index = 1;
		foreach (GameObject spirit in allSpirits)
		{
			Agent agent = spirit.GetComponent <Agent>();
			if (!spiritBook.ContainsKey (agent.name))
			{
				spiritBook.Add (agent.name, agent.gameObject);
				agent.id = index;
				// agent.level = 1;
				agent.agentManager = this;
				spirits.Add (agent);
				index ++;
			}
		}

		// ghosts
		index = 1;
		foreach (GameObject ghost in allGhosts)
		{
			Agent agent = ghost.GetComponent <Agent>();
			if (!ghostBook.ContainsKey (agent.name))
			{
				ghostBook.Add (agent.name, agent.gameObject);
				agent.id = index;
				// agent.level = 1;
				agent.agentManager = this;
				ghosts.Add (agent);
				index ++;
			}
		}

		// dictionary
		dayMeta.Clear ();

		AgentMeta todaysTree = null;
		List<AgentMeta> todaysSpirits = new List<AgentMeta> ();
		List<AgentMeta> todaysGhosts = new List<AgentMeta> ();
		DayMeta todaysMeta;
		AgentMeta agentMeta;
		int day = 0;
		int spiritNum = 0;
		int ghostNum = 0;
		int lineNum = 0;
		int lineIndex = 0;
		// string line;

		//-------start loading data
		TextAsset txt = (TextAsset) Resources.Load ("DayMeta", typeof(TextAsset));
		string content = txt.text;
  		string[] lines = Regex.Split (content, "\n|\r|\r\n");
		// StreamReader theReader = new StreamReader ("Assets/Data/DayMeta.txt", Encoding.Default);
		// using (theReader)
        // {
			// do
			// {
				// line = theReader.ReadLine();
				// if (line != null)
				// {
				foreach (string line in lines)
				{
					string[] entries = line.Split (',');
					if (entries.Length == 4)
					{
						day = int.Parse (entries [0]);
						spiritNum = int.Parse (entries [1]);
						ghostNum = int.Parse (entries [2]);
						lineNum = int.Parse (entries [3]);
						lineIndex = 0;
						todaysTree = null;
						todaysSpirits = new List<AgentMeta> ();
						todaysGhosts = new List<AgentMeta> ();
					}

					if (lineIndex < lineNum)
					{
						if (entries.Length == 12)
						{
							lineIndex ++;
							if (day != int.Parse (entries [0])) continue;
							if (treeBook.ContainsKey (entries [2]))
							{
								agentMeta = new AgentMeta (entries [1], entries [2], entries [3], entries [4], entries [5], entries [6], entries [7], entries [8], entries [9], entries [10], entries [11], treeBook [entries [2]]);
								agentMeta.ExportToPrefab ();
								agentMeta.ExportFromPrefab ();
								todaysTree = agentMeta;
							}
							if (spiritBook.ContainsKey (entries [2]))
							{
								agentMeta = new AgentMeta (entries [1], entries [2], entries [3], entries [4], entries [5], entries [6], entries [7], entries [8], entries [9], entries [10], entries [11], spiritBook [entries [2]]);
								agentMeta.ExportToPrefab ();
								agentMeta.ExportFromPrefab ();
								todaysSpirits.Add (agentMeta);
							}
						}
						if (entries.Length == 13)
						{
							lineIndex ++;
							if (day != int.Parse (entries [0])) continue;
							if (ghostBook.ContainsKey (entries [2]))
							{
								agentMeta = new AgentMeta (entries [1], entries [2], entries [3], entries [4], entries [5], entries [6], entries [7], entries [8], entries [9], entries [10], entries [11], ghostBook [entries [2]]);
								agentMeta.ExportToPrefab ();
								agentMeta.ExportFromPrefab ();
								agentMeta.LevelUp (int.Parse (entries [12]));
								todaysGhosts.Add (agentMeta);
							}
						}
					}

					if (lineIndex == lineNum)
					{
						todaysMeta = new DayMeta (day, todaysTree, todaysSpirits, todaysGhosts, spiritNum, ghostNum);
						dayMeta.Add (todaysMeta);
					}
				}
				// }
			// }
			// while (line != null);
			// theReader.Close();
        //  }
		 //-------finish loading data
	}

	public DayMeta GetDayMeta (int day)
	{
		if (day < 0) return null;

		if (day > dayMeta.Count)
		{
			while (dayMeta.Count < day)
			{
				DayMeta lastdayMeta = dayMeta [dayMeta.Count-1];
				DayMeta todaysMeta = lastdayMeta.NextDay ();
				dayMeta.Add (todaysMeta);
			}
		}

		return dayMeta [day-1];
	}

	public List<AgentMeta> GetSpirits (int day)
	{
		if (day > dayMeta.Count || day < 0) return null;

		return dayMeta [day-1].spiritMeta;
	}

	public List<AgentMeta> GetGhosts (int day)
	{
		if (day > dayMeta.Count || day < 0)
		{
			return null;
		}

		return dayMeta [day-1].ghostMeta;
	}

	public GameObject CreateTree (AgentMeta agentMeta)
	{
		GameObject gameObject = Instantiate (agentMeta.gameObject, agentHolder);
		agentMeta.CopyToAgent (ref gameObject);
		return gameObject;
	}

	public List<GameObject> CreateRoll (int num, List<AgentMeta> agentMeta)
	{
		if (agentMeta.Count < 0)
		{
			return null;
		}

		if (num <= 0)
		{
			return null;
		}

		List<GameObject> rolling = new List<GameObject> ();

		int sum = 0;
		int index = 0;
		int[] rate = new int[agentMeta.Count];
		foreach (AgentMeta meta in agentMeta)
		{
			if (meta.rate < 0)
			{
				continue;
			}
			sum += meta.rate;
			rate[index] = meta.rate;
			index ++;
		}

		index = 0;
		int index2 = 0;
		int[] perc = new int[sum];
		foreach (AgentMeta meta in agentMeta)
		{
			if (meta.rate == -1)
			{
				continue;
			}
			while (rate[index] > 0)
			{
				rate[index] --;
				perc[index2] = index;
				index2 ++;
			}
			index ++;
		}

		if (agentHolder == null)
		{
			GameObject agentObject = new GameObject ("Agents");
			agentHolder = agentObject.transform;
		}

		while (rolling.Count < num)
		{
			int randomNum = Random.Range (0, sum);
			int index3 = perc[randomNum];
			GameObject gameObject = Instantiate (agentMeta[index3].gameObject, agentHolder);
			agentMeta[index3].CopyToAgent (ref gameObject);
			gameObject.SetActive (false);
			rolling.Add (gameObject);
		}

		return rolling;
	}

	public void Recycle (GameObject gameObject)
	{
		recycle.Add (gameObject);
	}
}
