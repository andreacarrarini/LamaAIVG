using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehaviour : MovementBehaviour
{

    public Transform destination;

    public float gas;
    public float steer;
    public float brake;

    public float brakeAt;
    public float stopAt;

    public override Vector3 GetAcceleration( MovementStatus status )
    {
        if ( destination != null )
        {
            Vector3 verticalAdj = new Vector3( destination.position.x, transform.position.y, destination.position.z );
            Vector3 toDestination = (verticalAdj - transform.position);

            if ( toDestination.magnitude > stopAt )
            {
                Vector3 tangentComponent = Vector3.Project( toDestination.normalized, status.movementDirection );
                Vector3 normalComponent = (toDestination.normalized - tangentComponent);
                return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
            }
            else
            {
                return Vector3.zero;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }
}
