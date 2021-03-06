﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reposition : MonoBehaviour
{

    private Transform AICarTransform;

    // a modulo b
    static int MathMod( int a, int b )
    {
        return (Mathf.Abs( a * b ) + a) % b;
    }

    void Start()
    {
        AICarTransform = gameObject.transform;
        StartCoroutine( Respawn() );
    }

    public IEnumerator Respawn()
    {
        while ( true )
        {
            yield return new WaitForSeconds( 4 );

            int zRotation = ( int ) Mathf.Ceil( AICarTransform.rotation.eulerAngles.z );

            if ( MathMod( zRotation, 360 ) < 190 && (MathMod( zRotation, 360 ) > 155) )
            {
                Vector3 respawnPosition = AICarTransform.position;
                AICarTransform.SetPositionAndRotation( new Vector3( respawnPosition.x, 0, respawnPosition.z ), new Quaternion( 0, 0, 0, 0 ) );
            }
            else if ( MathMod( zRotation, 360 ) < 280 && (MathMod( zRotation, 360 ) > 80) )
            {
                Vector3 respawnPosition = AICarTransform.position;
                AICarTransform.SetPositionAndRotation( new Vector3( respawnPosition.x, 0, respawnPosition.z ), new Quaternion( 0, 0, 0, 0 ) );
            }

            // If car falls beneath the map
            else if ( AICarTransform.position.y <= -15 )
            {
                Vector3 respawnPosition = AICarTransform.position;
                AICarTransform.SetPositionAndRotation( new Vector3( respawnPosition.x, 0, respawnPosition.z ), new Quaternion( 0, 0, 0, 0 ) );
            }
        }
    }
}