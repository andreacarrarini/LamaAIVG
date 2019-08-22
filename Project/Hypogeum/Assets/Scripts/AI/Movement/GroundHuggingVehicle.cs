using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundHuggingVehicle : MonoBehaviour
{
    public Transform raycastPoint;
    public GameObject AICar;
    private RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        AICar = GameObject.Find( "AICar" );
        raycastPoint = GameObject.Find( "RaycastPoint" ).transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate to align with terrain
        Physics.Raycast( raycastPoint.position, Vector3.down, out hit );
        transform.up -= (transform.up - hit.normal) * 0.1f;
    }
}