using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

	// info
	public int id;					// unique id
	public string tag;				// Spirit, Ghost, SpiritTower, GhostTower
	public string cat;				// category
	public string name;				// name

	// stats
	public float damage;			// damage per time
	public float damageTime;		// 1 / damageFrequency
	public int num;					// number of enemies it can attack once
	// public float area;				// area of effect (m)
	public float speed;				// move speed (m/frame)
	public float health;			// initial health
	public float defense;			// defense power
	public float refresh;			// production speed (seconds)
	public int rate;
	public int cost;
	public int level;

	// controls
	public bool move;
	public bool right;
	public List<Agent> targets = new List<Agent>();
	public List<Agent> predators = new List<Agent>();
	public bool targetsLocked;
	public bool predatorsLocked;
	private float maxHealth;
	private float pace;
	private int n;

	public AgentManager agentManager;
	private Animator animator;
	public GameController game;

	/*
	public Agent (string _name, float _dps, float _aoe, float _speed, float _health, float _defense, float _refresh, int _cost, int _level)
	{
		name = _name;
		dps = _dps;
		aoe = _aoe;
		speed = _speed;
		health = _health;
		defense = _defense;
		refresh = _refresh;
		cost = _cost;
		level = _level;
	}
	*/

	void Awake ()
	{
		animator = GetComponent <Animator> ();

		gameObject.tag = tag;
		gameObject.name = name;
	}

	void Start ()
	{
		maxHealth = health;
		if (tag == "Tree" && transform.Find ("Leaves") != null)
		{
			pace = maxHealth / transform.Find ("Leaves").childCount;
			n = 0;
		}
	}

	void OnEnable ()
	{
		maxHealth = Mathf.Max (maxHealth, health);
		if (tag == "Tree" && transform.Find ("Leaves") != null)
		{
			pace = maxHealth / transform.Find ("Leaves").childCount;
		}
	}

	void Update ()
	{
		if (move)
		{
			if (right)
			{
				transform.position += Vector3.right * Time.deltaTime * speed;
			}
			else
			{
				transform.position += Vector3.left * Time.deltaTime * speed;
			}
		}
	}

	public void Move ()
	{
		// Debug.Log (name + " : Move");
		if (tag != "Tree")
		{
			animator.SetBool ("Move", true);
			move = true;
		}
	}

	public void Stop ()
	{
		// Debug.Log (name + " : Stop");
		animator.SetBool ("Move", false);
		move = false;
	}

	public void Attack ()
	{
		// Debug.Log (name + " : Attack");
		targetsLocked = true;
		if (!CheckIfDead ())
		{
			if (targets.Count > 0)
			{
				Stop ();
				animator.SetTrigger ("Attack");
				foreach (Agent target in targets)
				{
					if (target != null && target.gameObject.activeSelf)
					{
						target.Hurt (this, damage);
					}
					else
					{
						StartCoroutine (AttemptRemoveTarget (target));
					}
				}
				Invoke ("Attack", damageTime);
			}
			else
			{
				Move ();
			}
		}
		targetsLocked = false;
	}

	public void Hurt (Agent predator, float damage)
	{
		predatorsLocked = true;
		float healthLoss = damage * (1 - defense);
		// Debug.Log (name + " : Hurt -" + healthLoss);
		if (!predators.Contains (predator))
		{
			predators.Add (predator);
		}
		animator.SetTrigger ("Hurt");
		health -= healthLoss;
		if (tag == "Ghost")
		{
			StartCoroutine (game.AttemptAddExp ((int) Mathf.Floor (healthLoss/2)));
		}
		if (tag == "Tree")
		{
			if (maxHealth - health > pace * n)
			{
				foreach (Transform child in transform)
				{
					if (child.gameObject.name == "Leaves")
					{
						foreach (Transform grandchild in child)
						{
							if (grandchild.gameObject.activeSelf)
							{
								grandchild.gameObject.SetActive (false);
								n ++;
								break;
							}
						}
					}
				}
			}
		}
		CheckIfDead ();
		predatorsLocked = false;
	}

	public void Die ()
	{
		// Debug.Log (name + " : Die");
		CheckIfEnd ();
		Stop ();
		animator.SetTrigger ("Die");
		if (tag == "Tree")
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive (false);
			}
			NotATarget ();
			Destroy (GetComponent <BoxCollider2D> ());
			agentManager.Recycle (gameObject);
			enabled = false;
		}
		else
		{
			gameObject.tag = "Dead";
			NotATarget ();
			NotAPredator ();
			gameObject.GetComponent <Renderer> ().enabled = false;
			gameObject.GetComponent <Collider2D> ().enabled = false;
			agentManager.Recycle (gameObject);
			enabled = false;
		}
	}

	void NotATarget ()
	{
		foreach (Agent predator in predators)
		{
			if (predator != null && predator.gameObject.activeSelf)
			{
				StartCoroutine (predator.AttemptRemoveTarget (this));
			}
		}
	}

	void NotAPredator ()
	{
		foreach (Agent target in targets)
		{
			if (target != null && target.gameObject.activeSelf)
			{
				StartCoroutine (target.AttemptRemovePredator (this));
			}
		}
	}

	public bool CheckIfDead ()
	{
		if (health <= Mathf.Epsilon)
		{
			Die ();
			return true;
		}
		return false;
	}

	public void CheckIfEnd ()
	{
		if (tag == "Tree")
		{
			game.EndDay ("Ghost");
		}
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag (tag);
		if (gameObjects.Length == 0)
		{
			if (GameObject.Find ("Exit") != null)
				return;
			GameObject exit = new GameObject ("Exit");
			if (tag == "Ghost") exit.tag = "SpiritExit";
			if (tag == "Spirit" || tag == "Tree") exit.tag = "GhostExit";
			exit.transform.SetParent (transform.parent);
			if (tag == "Ghost") exit.transform.localPosition = transform.position + Vector3.left * 2;
			if (tag == "Spirit" || tag == "Tree") exit.transform.localPosition = transform.position + Vector3.right * 2;
			exit.transform.localScale = new Vector3 (1, 5, 1);
			exit.AddComponent <BoxCollider2D> ();
			exit.GetComponent <BoxCollider2D> ().isTrigger = true;
		}
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		// Debug.Log (name + " : Meet " + other.gameObject.tag);
		if (tag == "Wall")
		{
			enabled = false;
		}
		else if (tag == "Spirit" && other.gameObject.CompareTag ("Ghost")
		|| tag == "Ghost" && other.gameObject.CompareTag ("Spirit")
		|| tag == "Ghost" && other.gameObject.CompareTag ("Tree"))
		{
			if (targets.Count < num)
			{
				Agent target = other.gameObject.GetComponent <Agent> ();
				if (target != null && enabled)
				{
					StartCoroutine (AttemptAddTarget (target));
					Attack ();
				}
			}
		}
		else if (tag == "Spirit" && other.gameObject.CompareTag ("SpiritExit")
		|| tag == "Ghost" && other.gameObject.CompareTag ("GhostExit"))
		{
			game.EndDay (tag);
			enabled = false;
		}
	}

	void OnTriggerExit2D (Collider2D other)
	{
		if (tag == "Spirit" && other.gameObject.CompareTag ("Ghost")
		|| tag == "Ghost" && other.gameObject.CompareTag ("Spirit")
		|| tag == "Ghost" && other.gameObject.CompareTag ("Tree"))
		{
			Agent target = other.gameObject.GetComponent <Agent> ();
			if (target)
			{
				StartCoroutine (AttemptRemoveTarget (target));
			}
		}
	}

	public IEnumerator AttemptAddTarget (Agent target)
	{
		if (!enabled || !gameObject.activeSelf)
		{
			yield return null;
		}
		if (!target.enabled || !target.gameObject.activeSelf)
		{
			yield return null;
		}
		if (targetsLocked)
		{
			yield return new WaitForSeconds (0.1f);
		}
		// Debug.Log (name + " : Add to attack list");
		if (!targets.Contains (target))
		{
			targets.Add (target);
		}
	}

	public IEnumerator AttemptRemoveTarget (Agent target)
	{
		if (!enabled || !gameObject.activeSelf)
		{
			yield return null;
		}
		if (targetsLocked)
		{
			yield return new WaitForSeconds (0.1f);
		}
		// Debug.Log (name + " : Remove from attack list");
		targets.Remove (target);
		if (targets.Count <= 0)
		{
			Move ();
		}
	}

	public IEnumerator AttemptRemovePredator (Agent predator)
	{
		if (!enabled || !gameObject.activeSelf)
		{
			yield return null;
		}
		if (predatorsLocked)
		{
			yield return new WaitForSeconds (0.1f);
		}
		// Debug.Log (name + " : Remove from attack list");
		predators.Remove (predator);
		if (predators.Count <= 0)
		{
			Move ();
		}
	}

	public void ExportToMeta (ref List<AgentMeta> agentMeta, int i)
	{
		AgentMeta meta = agentMeta[i];
		meta.tag = tag;
		meta.name = name;
		meta.damage = damage;
		meta.damageTime = damageTime;
		meta.num = num;
		meta.speed = speed;
		meta.health = health;
		meta.defense = defense;
		meta.level = level;
		meta.cost = cost;
		meta.gameObject = gameObject;
	}
}
