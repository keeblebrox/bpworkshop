/**
@file Net.cs
*/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Net : MonoBehaviour {
	public enum PlayerNumber {
		PLAYER1 = 1,
		PLAYER2 = 2
	}
	public PlayerNumber player;

	void OnTriggerEnter(Collider collider) {
		Ball ball = collider.GetComponent<Ball>();
		if (null != ball) {
			GameState.Scoreboard.SendMessage("OnPlayerScored", player);
			ball.SendMessage("Lock", transform);
			ball.transform.parent = transform;
		}
	}
}
