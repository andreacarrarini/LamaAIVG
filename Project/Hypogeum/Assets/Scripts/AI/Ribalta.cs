using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ribalta : MonoBehaviour
{

	private Transform AICarTransform;

	// a modulo b
	static int MathMod( int a, int b )
	{
		return (Mathf.Abs( a * b ) + a) % b;
	}

	// Start is called before the first frame update
	void Start()
    {
		AICarTransform = gameObject.transform;
        StartCoroutine( Riposiziona() );
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator Riposiziona()
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
        }
    }
}
