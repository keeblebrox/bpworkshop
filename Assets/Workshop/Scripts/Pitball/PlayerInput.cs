/**
@file PlayerInput.cs
*/

using UnityEngine;
using System.Collections;

/**
@brief Interpret player input and send it to the PlayerMotor
@author Brian Crockford
*/
[RequireComponent(typeof(PlayerMotor))]
public class PlayerInput : MonoBehaviour {
	[System.Serializable]
	public class PlayerInputConfig {
		public string NavigationVertical	= "PlayerNVertical";
		public string NavigationHorizontal	= "PlayerNHorizontal";
		public string JumpButton			= "PlayerNJump";
	}
	public PlayerInputConfig	InputConfig = new PlayerInputConfig();

	[System.NonSerialized] public PlayerMotor motor;

	void Awake() {
		motor = GetComponent<PlayerMotor>();
	}
	
	void Update () {
		Vector3 directionVector = new Vector3(
			Input.GetAxis(InputConfig.NavigationHorizontal), 
			0, 
			Input.GetAxis(InputConfig.NavigationVertical)
		);
		
		if (directionVector != Vector3.zero) {
			float directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;

			directionLength = Mathf.Min(directionLength, 1);
			directionLength = directionLength * directionLength;

			directionVector = directionVector * directionLength;
		}
		//Set the movement parameters
		motor.movement.inputDirection = directionVector;
		motor.jumping.input = Input.GetButton(InputConfig.JumpButton);
	}
}