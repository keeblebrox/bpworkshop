/**
@file PlayerMotor.cs
*/

using UnityEngine;
using System.Collections;


/**
@brief Responsible for moving the player.
@author Brian Crockford
*/
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour {
	// Movement variables in one class, for organization
	[System.Serializable]
	public class PlayerMovement {
		public float speed = 10.0f;
		public float gravity = 10.0f;	//gravity to apply in y direction
		public float maxFallSpeed = 20.0f;	//terminal velocity
		public float maxGroundAcceleration = 30.0f;
		public float maxAirAcceleration = 20.0f;

		[System.NonSerialized] public Vector3 velocity = Vector3.zero;
		[System.NonSerialized] public Vector3 frameVelocity = Vector3.zero;
		[System.NonSerialized] public Vector3 inputDirection = Vector3.zero;
		[System.NonSerialized] public Vector3 hitPoint = Vector3.zero;
		[System.NonSerialized] public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
		[System.NonSerialized] public CollisionFlags collisionFlags;
	}
	public PlayerMovement movement = new PlayerMovement();

	// Jumping variables in one class, for organization
	[System.Serializable]
	public class PlayerJumping {
		public float baseHeight = 1.0f;		//minimum jumping distance
		public float extraHeight = 2.0f;	//maximum extra distance for holding jump key

		[System.NonSerialized] public bool jumping = false;	//are we jumping now
		[System.NonSerialized] public bool input = false;	//is jump currently pressed
		[System.NonSerialized] public bool inputHeld = false;	//is jump being held
		[System.NonSerialized] public float lastStartTime = 0.0f;
		[System.NonSerialized] public float lastButtonTime = 0.0f;
	}
	public PlayerJumping jumping = new PlayerJumping();

	[System.Serializable]
	public class PlayerShooting {
		public float shootSpeed = 20.0f;
		public float shootChargeTime = 2.0f;
		[System.NonSerialized] public Ball ball = null;
		[System.NonSerialized] public bool input = false;
		[System.NonSerialized] public bool inputHeld = false;
		[System.NonSerialized] public float lastStartTime = 0.0f;
	}
	public PlayerShooting shooting = new PlayerShooting();

	public class PlayerGround {
		public bool grounded = true;		//are we on the ground
		public Vector3 normal = Vector3.zero;	//ground angle
		public Vector3 lastNormal = Vector3.zero;	//ground angle last frame
	}
	public PlayerGround ground = new PlayerGround();

	// Retain access to the character controller
	[System.NonSerialized] public CharacterController controller;



	void Start() {
		controller = GetComponent<CharacterController>();
	}


	void Update() {
		if (!shooting.input && shooting.inputHeld) {
			if (null != shooting.ball) {
				Vector3 direction = transform.forward;
				float charge = Mathf.Max(Time.time - shooting.lastStartTime, shooting.shootChargeTime) / shooting.shootChargeTime;
				shooting.ball.transform.parent = null;
				shooting.ball.SendMessage("Shoot", direction * shooting.shootSpeed * charge);
			}
			shooting.inputHeld = false;
		}

		if (shooting.input && null != shooting.ball) {
			shooting.lastStartTime = Time.time;
		}
	}

	
	void FixedUpdate() {
		// We copy the actual velocity into a temporary variable that we can manipulate.
		Vector3 velocity = movement.velocity;
		
		// Update velocity based on input
		velocity = ApplyInputVelocityChange(velocity);
		
		// Apply gravity and jumping force
		velocity = ApplyGravityAndJumping (velocity);
		
		// Save lastPosition for velocity calculation.
		Vector3 lastPosition = transform.position;
		
		// We always want the movement to be framerate independent.  Multiplying by Time.deltaTime does this.
		Vector3 currentMovementOffset = velocity * Time.deltaTime;
		
		// Find out how much we need to push towards the ground to avoid loosing grouning
		// when walking down a step or over a sharp change in slope.
		float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
		if (ground.grounded) {
			currentMovementOffset -= pushDownOffset * Vector3.up;
		}
		
		// Reset variables that will be set by collision function
		ground.normal = Vector3.zero;
		
	   	// Move our character!
		movement.collisionFlags = controller.Move(currentMovementOffset);
		movement.lastHitPoint = movement.hitPoint;
		ground.lastNormal = ground.normal;
		
		// Calculate the velocity based on the current and previous position.  
		// This means our velocity will only be the amount the character actually moved as a result of collisions.
		Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
		movement.velocity = (transform.position - lastPosition) / Time.deltaTime;
		Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);
		
		// The CharacterController can be moved in unwanted directions when colliding with things.
		// We want to prevent this from influencing the recorded velocity.
		if (oldHVelocity == Vector3.zero) {
			movement.velocity = new Vector3(0, movement.velocity.y, 0);
		}
		else {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
		}
		
		if (movement.velocity.y < velocity.y - 0.001) {
			if (movement.velocity.y < 0) {
				// Something is forcing the CharacterController down faster than it should.
				// Ignore this
				movement.velocity.y = velocity.y;
			}
			else {
				// The upwards movement of the CharacterController has been blocked.
				// This is treated like a ceiling collision - stop further jumping here.
				jumping.inputHeld = false;
			}
		}
		
		// We were grounded but just lost grounding
		if (ground.grounded && !IsGroundedTest()) {
			ground.grounded = false;
			
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			// We pushed the character down to ensure it would stay on the ground; cancel that.
			transform.position += pushDownOffset * Vector3.up;
		}
		// We were not grounded but just landed on something
		else if (!ground.grounded && IsGroundedTest()) {
			ground.grounded = true;
			jumping.jumping = false;
			
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
		
	}



	Vector3 ApplyInputVelocityChange(Vector3 velocity) {	
		Vector3 desiredVelocity = GetDesiredHorizontalVelocity();
		
		if (ground.grounded) {
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, ground.normal);
		} 
		else {
			velocity.y = 0;
		}
		
		// Enforce max velocity change
		float maxVelocityChange = GetMaxAcceleration(ground.grounded) * Time.deltaTime;
		Vector3 velocityChangeVector = (desiredVelocity - velocity);
		if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}

		// If we're in the air and don't have control
		if (ground.grounded) {
			velocity += velocityChangeVector;
		}
		
		return velocity;
	}



	Vector3 ApplyGravityAndJumping(Vector3 velocity) {
		if (!jumping.input) {
			jumping.inputHeld = false;
			jumping.lastButtonTime = -100;
		}
		
		if (jumping.input && jumping.lastButtonTime < 0) {
			jumping.lastButtonTime = Time.time;
		}
		
		if (ground.grounded) {
			velocity.y = Mathf.Min(0, velocity.y) - movement.gravity * Time.deltaTime;
		}
		else {
			velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;
			
			// When jumping, don't apply gravity while holding the jump button.
			if (jumping.jumping && jumping.inputHeld) {
				// Calculate the duration that the extra jump force should have effect.
				// If we're still less than that duration after the jumping time, apply the force.
				if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)) {
					// Negate the gravity we just applied
					velocity += Vector3.up * movement.gravity * Time.deltaTime;
				}
			}
			
			// Make sure we don't fall any faster than maxFallSpeed. This gives our character a terminal velocity.
			velocity.y = Mathf.Max (velocity.y, -movement.maxFallSpeed);
		}
			
		if (ground.grounded) {
			// Jump only if the jump button was pressed down in the last 0.2 seconds.
			// This makes chain jumps feel natural.
			if (Time.time - jumping.lastButtonTime < 0.2f) {
				ground.grounded = false;
				jumping.jumping = true;
				jumping.lastStartTime = Time.time;
				jumping.lastButtonTime = -100;
				jumping.inputHeld = true;
				
				// Apply the jumping force to the velocity. Cancel any vertical velocity first.
				velocity.y = 0;
				velocity += Vector3.up * CalculateJumpVerticalSpeed(jumping.baseHeight);
				
				SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
			}
			else {
				jumping.inputHeld = false;
			}
		}
		
		return velocity;
	}



	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.normal.y > 0 && hit.normal.y > ground.normal.y && hit.moveDirection.y < 0) {
			if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || ground.lastNormal == Vector3.zero)
				ground.normal = hit.normal;
			else
				ground.normal = ground.lastNormal;
			
			movement.hitPoint = hit.point;
			movement.frameVelocity = Vector3.zero;
		}
	}



	Vector3 GetDesiredHorizontalVelocity() {
		// Find desired velocity
		Vector3 desiredLocalDirection = transform.InverseTransformDirection(movement.inputDirection);
		return transform.TransformDirection(desiredLocalDirection * movement.speed);
	}



	Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal) {
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(sideways, ground.normal).normalized * hVelocity.magnitude;
	}



	bool IsGroundedTest() {
		return (ground.normal.y > 0.01f);
	}



	float GetMaxAcceleration(bool grounded) {
		// Maximum acceleration on ground and in air
		if (ground.grounded) {
			return movement.maxGroundAcceleration;
		}
		return movement.maxAirAcceleration;
	}



	float CalculateJumpVerticalSpeed(float targetJumpHeight) {
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt (2 * targetJumpHeight * movement.gravity);
	}

}

