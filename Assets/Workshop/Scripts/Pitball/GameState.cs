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
	[System.Serializable]
	public class Scores {
		public int MaxScore = 10;
		public int PointsPerHit = 1;
		public int PointsPerGoal = 2;
		[System.NonSerialized] public int Player1Score = 0;
		[System.NonSerialized] public int Player2Score = 0;
	}
	public Scores scores = new Scores();


	[System.Serializable]
	public class GameTime {
		public float RoundDuration = 60;
		[System.NonSerialized] public float RoundStart = 0;
		[System.NonSerialized] public float RoundEnd = 0;
	}
	public GameTime time;


	public enum StateName {
		START,
		PLAYING,
		OVER
	}
	[System.NonSerialized] public StateName state = StateName.START;

	public Transform Player1;
	public Transform Player2;
	private Vector3 Player1Position;
	private Vector3 Player2Position;



	void Awake() {
		if (null != Player1) {
			Player1Position = Player1.position;
		}
		else {
			Debug.LogError("Player 1 hasn't been set!");
		}
		if (null != Player2) {
			Player2Position = Player2.position;
		}
		else {
			Debug.LogError("Player 2 hasn't been set!");
		}
	}


	
	void Update() {
		if (state == StateName.PLAYING) {
			if (Time.time > time.RoundEnd) {
				GameOver();
			}
			else if (scores.Player1Score > scores.MaxScore || scores.Player2Score > scores.MaxScore) {
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
		GUILayout.Label(""+ scores.Player1Score);
		GUILayout.EndVertical();
		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(Screen.width * 2/3,0,Screen.width/3,Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Player 2 Score");
		GUILayout.Label(""+ scores.Player2Score);
		GUILayout.EndVertical();
		GUILayout.EndArea();

		//Timer
		GUILayout.BeginArea(new Rect(Screen.width/3,0,Screen.width/3,Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Time remaining");
		GUILayout.Label(""+ (time.RoundEnd - Time.time) + " seconds");
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}



	void DrawGameOverWindow() {
		GUILayout.BeginArea(new Rect(Screen.width/3,Screen.height/3,Screen.width/3,Screen.height/3));
		GUILayout.BeginVertical();

		GUILayout.Label("Game Over!");
		if (scores.Player1Score == scores.Player2Score) {
			GUILayout.Label("TIE!");
		}
		else if (scores.Player1Score > scores.Player2Score) {
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
		time.RoundStart = Time.time;
		time.RoundEnd = Time.time + time.RoundDuration;
		scores.Player1Score = 0;
		scores.Player2Score = 0;

		//set player positions
		Player1.position = Player1Position;
		Player2.position = Player2Position;

		//enable controls
		Player1.SendMessage("OnEnableControls", SendMessageOptions.DontRequireReceiver);
		Player2.SendMessage("OnEnableControls", SendMessageOptions.DontRequireReceiver);

		state = StateName.PLAYING;
	}



	void GameOver() {
		Player1.SendMessage("OnDisableControls", SendMessageOptions.DontRequireReceiver);
		Player2.SendMessage("OnDisableControls", SendMessageOptions.DontRequireReceiver);

		state = StateName.OVER;
	}
}
