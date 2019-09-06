using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBehaviourVolume : MovementBehaviour {

    [Range( 0f, 1000f )] public float sightRange;
	public float sightAngle;

	public float steer;
	public float backpedal;

	public override Vector3 GetAcceleration (MovementStatus status) {

		Collider collider = GetComponentInChildren<Collider>();

        //bool leftHit = Physics.BoxCast (transform.position, 
        //                                collider.bounds.extents, 
        //                                Quaternion.Euler (0f, - sightAngle, 0f) * status.movementDirection, 
        //                                transform.rotation, 
        //                                sightRange);

        //bool centerHit = Physics.BoxCast (transform.position, 
        //                                  collider.bounds.extents, 
        //                                  status.movementDirection, 
        //                                  transform.rotation, 
        //                                  sightRange);

        //bool rightHit = Physics.BoxCast (transform.position, 
        //                                 collider.bounds.extents, 
        //                                 Quaternion.Euler (0f, sightAngle, 0f) * status.movementDirection, 
        //                                 transform.rotation, 
        //                                 sightRange);

        bool leftHit = Physics.Raycast( transform.position,
                                        Quaternion.Euler( 0f, -sightAngle, 0f ) * status.movementDirection,
                                        sightRange );

        bool centerHit = Physics.Raycast( transform.position,
                                          status.movementDirection,
                                          sightRange );

        bool rightHit = Physics.Raycast( transform.position,
                                         Quaternion.Euler( 0f, sightAngle, 0f ) * status.movementDirection,
                                         sightRange );

        Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.movementDirection.normalized;

		if (leftHit && !centerHit && !rightHit) {
			return right * steer;
		} else if (leftHit && centerHit && !rightHit) {
			return right * steer * 2f;
		} else if (leftHit && centerHit && rightHit) {
			return -status.movementDirection.normalized * backpedal;
		} else if (!leftHit && centerHit && rightHit) {
			return -right * steer * 2f;
		} else if (!leftHit && !centerHit && rightHit) {
			return -right * steer;
		} else if (!leftHit && centerHit && !rightHit) {
			return right * steer;
		}

		return Vector3.zero;
	}
}
