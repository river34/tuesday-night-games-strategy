using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour {

	public GameController game;
	public GameObject agentObject;

	public void SelectRoll ()
	{
		StartCoroutine (game.AttemptRemoveRoll (gameObject));
		agentObject.SetActive (true);
		agentObject.GetComponent <Agent> ().game = game;
		agentObject.GetComponent <Agent> ().Move ();
		enabled = false;

		// sound
		game.soundManager.RandomizeEffect (game.clickSounds);
	}

	void OnDisable ()
	{
		gameObject.SetActive (false);
	}
}
