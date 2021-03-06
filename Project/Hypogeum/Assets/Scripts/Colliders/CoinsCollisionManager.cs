﻿/*
MIT License
Copyright (c) 2019 Team Lama: Carrarini Andrea, Cerrato Loris, De Cosmo Andrea, Maione Michele
Author: Maione Michele
Contributors: Andrea Carrarini
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using UnityEngine;

public class CoinsCollisionManager : MonoBehaviour
{

    private InstinctReasonManager instinctReasonManager;


    void Start()
    {
        var obj = GameObject.Find( "InstinctReasonManager" );
        instinctReasonManager = obj.GetComponent<InstinctReasonManager>();
    }

    void Update()
    {
        gameObject.transform.RotateAroundLocal( Vector3.up, 2 * Time.deltaTime );
    }

    // Changed for the AI
    private void OnTriggerEnter( Collider otherObjectCollider )
    {
        GB.ECoin? tipo = null;

        // AI part
        var gobj = otherObjectCollider.attachedRigidbody.gameObject;
        if ( gobj.name == "AICar(Clone)" )
        {
            gobj.GetComponent<FSMBehaviour>().CoinTaken = true;
            gobj.GetComponent<FSMBehaviour>().CarOnRamp = false;

            foreach ( GameObject go in GameObject.FindGameObjectsWithTag( "ramp" ) )
            {
                // Re-enabling raycast detection
                go.layer = LayerMask.NameToLayer( "Default" );
            }

            Destroy( gameObject );
        }

        if ( gameObject.name == "CoinReason(Clone)" )
            tipo = GB.ECoin.Reason;
        else if ( gameObject.name == "CoinInstinct(Clone)" )
            tipo = GB.ECoin.Instinct;

        if ( tipo.HasValue )
        {
            GB.EAnimal? animale = null;
            var go = otherObjectCollider.attachedRigidbody.gameObject;

            if ( go.CompareTag( "car" ) )
            {
                var gc = go.GetComponent<GeneralCar>();
                animale = gc.AnimalType;
            }
            else if ( go.CompareTag( "Bullet" ) )
            {
                var b = go.GetComponent<Bullet>();
                animale = b.AnimaleCheHaSparatoQuestoColpo;
            }

            if ( animale.HasValue )
                instinctReasonManager.Cmd_server_OnCoinChosed( animale.Value, tipo.Value, gameObject );
        }

    }


}