/**
@file Ball.cs
*/

using UnityEngine;
using System.Collections;


/**
@brief The ball the players use to score points
@author Brian Crockford
*/
[RequireComponent(typeof(SphereCollider))]
public class Ball : MonoBehaviour {
	[System.Serializable]
	public class BallMovement {
		public float maxSpeed = 30.0f;
		public float fallSpeed = 5.0f;	//speed at which the ball starts falling		
		public float gravity = 5.0f;
		public float damping = 0.95f;
		public float maxFallSpeed = 20.0f;
		[System.NonSerialized] public Vector3 velocity = Vector3.zero;
		[System.NonSerialized] public bool grounded = false;
	}
	public BallMovement movement = new BallMovement();

	[System.Serializable]
	public class BallLocking {
		public bool locked = false;
		[System.NonSerialized] public Transform target;
	}
	public BallLocking locking = new BallLocking();


	void Start() {
		//Reset();
	}
	

	void FixedUpdate() {
		if (!locking.locked) {
			Vector3 velocity = movement.velocity;
			
			//damp the velocity
			float dampingDelta = (1.0f - movement.damping) * velocity.magnitude;
			velocity = velocity.normalized * (velocity.magnitude - dampingDelta);

			if (!movement.grounded && 0.0 != movement.fallSpeed && velocity.magnitude < movement.fallSpeed) {
				float scale = (movement.fallSpeed - velocity.magnitude) / movement.fallSpeed;
				velocity.y -= scale * scale * movement.gravity;
				velocity.y = Mathf.Min(velocity.y, movement.maxFallSpeed);
			}

			transform.position += velocity * Time.fixedDeltaTime;
			movement.velocity = velocity;
		}
	}


	void Reset() {
		movement.velocity = Vector3.zero;
		movement.grounded = false;
		locking.locked = false;
		locking.target = null;
	}


	void Shoot(Vector3 velocity) {
		Unlock();
		movement.velocity = velocity;
	}


	void Lock(Transform target) {
		if (null != target) {
			movement.grounded = false;
			locking.locked = true;
			locking.target = target;
		}
	}


	void Unlock() {
		locking.locked = false;
		locking.target = null;
	}


	void OnCollisionEnter(Collision collision) {
		if (null != collision.contacts && collision.contacts.Length > 0) {
			ContactPoint contact = collision.contacts[0];

			if (contact.normal.y > 0.3) {
				movement.grounded = true;
				movement.velocity.y = 0;
			}
			else {
				movement.velocity = Vector3.Reflect(movement.velocity, collision.contacts[0].normal);
			}
		}
	}
}
