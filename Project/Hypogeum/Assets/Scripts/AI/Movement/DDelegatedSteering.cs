using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]

//TODO Ensure that the car doesn't start flying, especially on ramps
public class DDelegatedSteering : MonoBehaviour {

	public float minLinearSpeed = 0.5f;
    [Range( 0f, 1000f )] public float maxLinearSpeed = 180f;
	public float maxAngularSpeed = 5f;

    private MovementStatus status;

    private WheelCollider[] wheels = new WheelCollider[ 4 ];

    private void Start () {
		status = new MovementStatus ();
		status.movementDirection = transform.forward;

        var i = 0u;
        var wc = GetComponentsInChildren<WheelCollider>();
        var obj_figli = GetComponentsInChildren<MeshRenderer>();

        foreach ( var w in wc )
        {
            foreach ( var o in obj_figli )
                if ( o.gameObject.name.Equals( $"Wheel_{w.name}" ) )
                {
                    wheels[ i ] = w;
                    break;
                }

            i++;
        }

    }

	void FixedUpdate () {

		Vector3 totalAcceleration = Vector3.zero;
		foreach (MovementBehaviour mb in GetComponents<MovementBehaviour> ()) {
			totalAcceleration += mb.GetAcceleration (status);
		}

		if (totalAcceleration.magnitude != 0f) {

			Vector3 tangentComponent = Vector3.Project (totalAcceleration, status.movementDirection);
			Vector3 normalComponent = totalAcceleration - tangentComponent;

			float tangentAcc = tangentComponent.magnitude * Vector3.Dot (tangentComponent.normalized, status.movementDirection);
			Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.movementDirection.normalized;
			float rotationAcc = normalComponent.magnitude * Vector3.Dot (normalComponent.normalized, right) * 360f;

			float t = Time.deltaTime;

			float tangentDelta = status.linearSpeed * t + 0.5f * tangentAcc * t * t;
			float rotationDelta = status.angularSpeed * t + 0.5f * rotationAcc * t * t;

			status.linearSpeed += tangentAcc * t;
			status.angularSpeed += rotationAcc * t;

			status.linearSpeed = Mathf.Clamp (status.linearSpeed, minLinearSpeed, maxLinearSpeed);
			status.angularSpeed = Mathf.Clamp (status.angularSpeed, -maxAngularSpeed, maxAngularSpeed);

			Rigidbody rb = GetComponent<Rigidbody> ();

            // Apply movement only if at least 3 of 4 wheels are on the ground
            int wheelsOnTheGround = 0;

            foreach ( WheelCollider wheelCollider in wheels )
            {
                if ( wheelCollider.isGrounded )
                    wheelsOnTheGround += 1;
            }

            if ( wheelsOnTheGround >= 1 )
            {
                rb.MovePosition( rb.position + transform.forward * tangentDelta );
                rb.MoveRotation( rb.rotation * Quaternion.Euler( 0f, rotationDelta, 0f ) );

                //status.movementDirection = transform.forward;
            }

            //rb.MovePosition( rb.position + transform.forward * tangentDelta );
            //rb.MoveRotation( rb.rotation * Quaternion.Euler( 0f, rotationDelta, 0f ) );

            status.movementDirection = transform.forward;
		}
	}
}
