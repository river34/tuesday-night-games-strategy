using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour {

	public Text levelText;
	public Text costText;
	public Text expText;
	public GameController game;
	public AgentMeta agentMeta;

	void Start ()
	{
		levelText.text = "LEVEL " + agentMeta.level;
		costText.text = "- " + agentMeta.cost;
		expText = game.UpgradeUI.transform.Find ("Exp").GetComponent <Text> ();
		expText.text = "EXP " + game.exp;
	}

	public void SelectUpgrade ()
	{
		agentMeta.LevelUp ("Spirit", ref game.exp);
		levelText.text = "LEVEL " + agentMeta.level;
		costText.text = "- " + agentMeta.cost;
		expText.text = "EXP " + game.exp;
	}
}
