/**
@file Player.cs
*/

using UnityEngine;
using System.Collections;


/**
@brief Retains player state and handles player input.

This type of player is going to scate around
@author Brian Crockford
*/
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {
	public PlayerInputConfig	InputConfig;
	public float 				PlayerSpeed;

	

	[System.NonSerialized] 
	public CharacterController controller;

	void Start () {
		controller = GetComponent<CharacterController>();
		if (null == controller) {
			Debug.LogError("Couldn't find CharacterController!");
		}
	}
	
	void Update () {
		Vector3 vertical 	= Vector3.forward * Input.GetAxis(InputConfig.NavigationVertical);
		Vector3 horizontal 	= Vector3.right * Input.GetAxis(InputConfig.NavigationHorizontal);
		Vector3 direction	= Vector3.Normalize(vertical + horizontal);

		controller.Move(direction * PlayerSpeed * Time.deltaTime);
	}
}

/**
@brief Input configuration information for player components.
@author Brian Crockford
*/
[System.Serializable]
public class PlayerInputConfig {
	public string NavigationVertical	= "PlayerNVertical";
	public string NavigationHorizontal	= "PlayerNHorizontal";
}