/**
@file GameState.cs
*/

using UnityEngine;
using System.Collections;

/**
@brief keep track of the game progress
@author Brian Crockford
*/
public class GameState : MonoBehaviour {
	public static GameState Scoreboard;

	[System.Serializable]
	public class Scores {
		public int maxScore = 10;
		public int pointsPerHit = 1;
		public int pointsPerGoal = 2;
		[System.NonSerialized] public int player1Score = 0;
		[System.NonSerialized] public int player2Score = 0;
	}
	public Scores scores = new Scores();


	[System.Serializable]
	public class GameTime {
		public float roundDuration = 60;
		[System.NonSerialized] public float roundStart = 0;
		[System.NonSerialized] public float roundEnd = 0;
	}
	public GameTime time;


	public enum StateName {
		START,
		PLAYING,
		OVER
	}
	[System.NonSerialized] public StateName state = StateName.START;


	[System.Serializable]
	public class Players {
		public Transform player1;
		public Transform player2;
		[System.NonSerialized] public Vector3 player1Position;
		[System.NonSerialized] public Vector3 player2Position;
	}
	public Players players = new Players();


	[System.Serializable]
	public class BallInfo {
		public Transform ball;
		[System.NonSerialized] public Ball component;
		[System.NonSerialized] public Vector3 ballDropPosition;
	}
	public BallInfo ball = new BallInfo();


	void Awake() {
		// We're the global scoreboard
		if (null == Scoreboard) {
			Scoreboard = this;
		}
		
		// set player 1 position
		if (null != players.player1) {
			players.player1Position = players.player1.position;
		}
		else {
			Debug.LogError("Player 1 hasn't been set!");
		}
		// set player 2 position
		if (null != players.player2) {
			players.player2Position = players.player2.position;
		}
		else {
			Debug.LogError("Player 2 hasn't been set!");
		}

		// set ball position
		if (null != ball.ball) {
			ball.ballDropPosition = ball.ball.position;
			ball.component = ball.ball.GetComponent<Ball>();
			if (null != ball.component) {
				ball.component.enabled = false;
			}
		}
		else {
			Debug.LogError("Ball hasn't been set!");
		}
	}


	
	void Update() {
		if (state == StateName.PLAYING) {
			if (Time.time > time.roundEnd) {
				GameOver();
			}
			else if (scores.player1Score > scores.maxScore || scores.player2Score > scores.maxScore) {
				GameOver();
			}
		}
	}



	void OnGUI() {
		switch (state) {
		case StateName.START:
			DrawStartWindow();
			break;
		case StateName.PLAYING:
			DrawGUI();
			break;
		case StateName.OVER:
			DrawGameOverWindow();
			break;
		}
	}



	void DrawStartWindow() {
		GUILayout.BeginArea(new Rect(Screen.width/3,Screen.height/3,Screen.width/3,Screen.height/3));
		GUILayout.BeginVertical();

		GUILayout.Label("Press the button to start the game!");
		if (GUILayout.Button("START")) {
			NewGame();
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}



	void DrawGUI() {
		//Scores
		GUILayout.BeginArea(new Rect(0,0,Screen.width/3,Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Player 1 Score");
		GUILayout.Label(""+ scores.player1Score);
		GUILayout.EndVertical();
		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(Screen.width * 2/3,0,Screen.width/3,Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Player 2 Score");
		GUILayout.Label(""+ scores.player2Score);
		GUILayout.EndVertical();
		GUILayout.EndArea();

		//Timer
		GUILayout.BeginArea(new Rect(Screen.width/3,0,Screen.width/3,Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Time remaining");
		GUILayout.Label(""+ (time.roundEnd - Time.time) + " seconds");
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}



	void DrawGameOverWindow() {
		GUILayout.BeginArea(new Rect(Screen.width/3,Screen.height/3,Screen.width/3,Screen.height/3));
		GUILayout.BeginVertical();

		GUILayout.Label("Game Over!");
		if (scores.player1Score == scores.player2Score) {
			GUILayout.Label("TIE!");
		}
		else if (scores.player1Score > scores.player2Score) {
			GUILayout.Label("PLAYER 1 WINS!");
		}
		else {
			GUILayout.Label("PLAYER 2 WINS!");
		}

		if (GUILayout.Button("PLAY AGAIN")) {
			NewGame();
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}



	void NewGame() {
		time.roundStart = Time.time;
		time.roundEnd = Time.time + time.roundDuration;
		scores.player1Score = 0;
		scores.player2Score = 0;

		//set player positions
		players.player1.position = players.player1Position;
		players.player2.position = players.player2Position;

		//enable controls
		players.player1.SendMessage("OnEnableControls", SendMessageOptions.DontRequireReceiver);
		players.player2.SendMessage("OnEnableControls", SendMessageOptions.DontRequireReceiver);

		//set the ball position
		ball.ball.position = ball.ballDropPosition;
		ball.component.enabled = true;

		state = StateName.PLAYING;
	}



	void GameOver() {
		players.player1.SendMessage("OnDisableControls", SendMessageOptions.DontRequireReceiver);
		players.player2.SendMessage("OnDisableControls", SendMessageOptions.DontRequireReceiver);

		//set the ball position
		ball.component.enabled = false;
		ball.component.SendMessage("Reset");

		state = StateName.OVER;
	}



	void OnPlayerScored(int player) {
		switch (player) {
		case 1:
			scores.player1Score += scores.pointsPerGoal;
			break;
		case 2:
			scores.player2Score += scores.pointsPerGoal;
			break;
		}

		ball.ball.position = ball.ballDropPosition;
		ball.component.enabled = true;
	}



	void OnPlayerHit(int player) {
		//this is reversed, since the player being hit isn't getting points
		switch (player) {
		case 1:
			scores.player2Score += scores.pointsPerGoal;
			break;
		case 2:
			scores.player1Score += scores.pointsPerGoal;
			break;
		}
	}
}
