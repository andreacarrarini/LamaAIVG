/*
MIT License
Copyright (c) 2019 Carrarini Andrea
Author: Carrarini Andrea
Contributors: 
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FSMBehaviour : MonoBehaviour
{
    [Range( 0f, 1000f )] public float enemyRange;
    [Range( 0f, 10000f )] public float coinRange;
    [Range( 0f, 50f )] public float jumpRange;
    [Range( 0f, 50f )] public float padDistance;

    public float reactionTime = .5f;

    private string coinTag = "coin";
    private string jumpTag = "jumpPad";
    private string baseTag = "basePad";

    private GameObject enemyCar = null;
    private GeneralCar generalCar;
    private SeekBehaviour seekBehaviour;
    private FleeBehaviour fleeBehaviour;
    public GameObject nearestBasePad, nearestMidPad, nearestJumpPad, nearestCoin, destination = null;

    private Coroutine attackCR, pickCoinCR, moveAroundMapCR, jumpForHypeCR = null;
    private bool resetAttackBT, resetPickCoinBT, resetMoveAroundMapBT, resetJumpForHypeBT = false;

    private float maxHypeValue = 1000f;
    private bool coinTaken = false;

    // Must be changed to true after the jump is done to exit the state
    // Must be change back to false when leaving the state
    private bool jumpTaken = false;

    // FSMStates Behaviour Trees
    private CRBT.BehaviorTree AttackBT, PickCoinBT, MoveAroundMapBT, JumpForHypeBT;

    // General FSM
    private FSM generalFSM;

    // Properties
    public bool CoinTaken { get => coinTaken; set => coinTaken = value; }
    public bool CarOnRamp { get => carOnRamp; set => carOnRamp = value; }

    // To activate the check on how many wheels are on the ground
    private bool carOnRamp = false;


    #region FSM Conditions

    public bool EnemiesInRange()
    {
        if ( (enemyCar.transform.position - transform.position).magnitude <= enemyRange )
            return true;
        return false;
    }

    public bool NoEnemiesInRange()
    {
        return !EnemiesInRange();
    }

    public bool CoinInRangeAndCoinNotTaken()
    {
        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( coinTag ) )
        {
            if ( ((go.transform.position - transform.position).magnitude <= coinRange) && !CoinTaken )
            {
                nearestCoin = go;
                return true;
            }
        }
        return false;
    }

    public bool CoinTakenCondition()
    {
        if ( CoinTaken )
        {
            return true;
        }
        return false;
    }

    public bool JumpInRangeAndHypeNotFull()
    {
        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( baseTag ) )
        {
            if ( ((go.transform.position - transform.position).magnitude <= jumpRange) && (generalCar.Hype < maxHypeValue) )
                return true;
        }
        return false;
    }

    public bool JumpTaken()
    {
        if ( jumpTaken )
        {
            return true;
        }
        return false;
    }
    #endregion

    #region BTs Tasks

    #region Conditions

    public bool MyResistanceGreaterThanHis()
    {
        if ( generalCar.Defense >= enemyCar.GetComponent<GeneralCar>().Defense )
            return true;
        return false;
    }

    public bool DistanceFromBasePad()
    {
        if ( (nearestBasePad.transform.position - gameObject.transform.position).magnitude > padDistance )
            return true;
        return false;
    }

    public bool DistanceFromMidPad()
    {
        if ( (nearestMidPad.transform.position - gameObject.transform.position).magnitude > padDistance )
            return true;
        return false;
    }

    public bool NearestPadFound()
    {
        if ( nearestJumpPad )
            return true;
        return false;
    }

    public bool DestinationFound()
    {
        if ( destination )
            return true;
        return false;
    }

    public bool DistanceFromDestination()
    {
        if ( (destination.transform.position - gameObject.transform.position).magnitude > 20 )
            return true;
        return false;
    }
    #endregion

    #region Actions

    public bool Chase()
    {
        // To stop fleeing and start chasing
        if ( MyResistanceGreaterThanHis() )
        {
            fleeBehaviour.destination = null;
            seekBehaviour.destination = enemyCar.transform;
            return true;
        }
        return false;
    }

    public bool KeepDistance()
    {
        // To stop chasing and start fleeing
        if ( !MyResistanceGreaterThanHis() )
        {
            seekBehaviour.destination = null;
            fleeBehaviour.destination = enemyCar.transform;
            return true;
        }
        return false;
    }

    // Action to make the car go to the base of the ramp
    public bool MoveToRamp()
    {
        seekBehaviour.destination = nearestBasePad.transform;

        return true;
    }

    public bool MoveToMidPad()
    {
        CarOnRamp = true;
        IgnoreRampRaycast();
        seekBehaviour.destination = nearestMidPad.transform;

        seekBehaviour.brake *= 0.1f;
        seekBehaviour.brakeAt *= 0.1f;

        // To ensure the car doesn't try to reach the mid pad from the ground
        StartCoroutine( WaitForMidPadReached() );

        return true;
    }

    public bool Jump()
    {
        seekBehaviour.destination = nearestJumpPad.transform;

        CarOnRamp = false;

        // To avoid to steer all to right nefore taking the coin
        gameObject.GetComponent<AvoidBehaviourVolume>().steer = 3;

        // To give the car more speed to take the jump
        gameObject.GetComponent<SeekBehaviour>().gas *= 2f;

        StartCoroutine( WaitForJump() );

        return true;
    }

    public bool MoveToCoin()
    {
        CarOnRamp = false;

        seekBehaviour.destination = nearestJumpPad.transform;

        // To avoid to steer all to right nefore taking the coin
        //gameObject.GetComponent<AvoidBehaviourVolume>().steer = 10;
        gameObject.GetComponent<AvoidBehaviourVolume>().steer = 3;

        gameObject.GetComponent<SeekBehaviour>().gas *= 2f;

        // To check if the coin has been taken, if not, do it again
        StartCoroutine( WaitForCoinTaken() );

        return true;
    }

    // To know from where the coin is accessible and to move to the base of the correct ramp
    public bool GetRampPads()
    {
        float minDistance = 100000f;

        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( baseTag ) )
        {
            if ( (go.transform.position - gameObject.transform.position).magnitude < minDistance )
            {
                minDistance = (go.transform.position - gameObject.transform.position).magnitude;
                nearestBasePad = go;
            }
        }

        switch ( nearestBasePad.name )
        {
            case "DragonBasePad":
                nearestMidPad = GameObject.Find( "DragonMidPad" );
                nearestJumpPad = GameObject.Find( "DragonJumpPad" );
                break;
            case "WoodBasePad":
                nearestMidPad = GameObject.Find( "WoodMidPad" );
                nearestJumpPad = GameObject.Find( "WoodJumpPad" );
                break;
            case "StoneBasePad":
                nearestMidPad = GameObject.Find( "StoneMidPad" );
                nearestJumpPad = GameObject.Find( "StoneJumpPad" );
                break;
            case "BigBasePad":
                nearestMidPad = GameObject.Find( "BigMidPad" );
                nearestJumpPad = GameObject.Find( "BigJumpPad" );
                break;
        }

        if ( nearestJumpPad && nearestMidPad )
            return true;
        return false;
    }

    // Can be improved
    public bool PickRandomDestination()
    {
        if ( !destination )
        {
            float randomX, randomZ;

            if ( Random.value < 0.5 )
            {
                randomX = Random.value * -250;
                randomZ = Random.value * -230;
            }
            randomX = Random.value * 250;
            randomZ = Random.value * 230;

            GameObject go = GB.LoadDestinationPlaceholder();
            destination = Instantiate( go, new Vector3( randomX, -6, randomZ ), new Quaternion() );
        }
        return true;
    }

    public bool MoveToDestination()
    {
        seekBehaviour.destination = destination.transform;

        return true;
    }

    public bool ResetDestination()
    {
        Destroy( destination );
        destination = null;
        seekBehaviour.destination = null;

        // To never satisfy the UF
        return true;
    }
    #endregion

    #endregion

    public bool DistanceFromPad( GameObject go )
    {
        if ( (go.transform.position - gameObject.transform.position).magnitude > padDistance )
            return true;
        return false;
    }

    public void IgnoreRampRaycast()
    {
        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( "ramp" ) )
        {
            // Ignore Raycast
            go.layer = LayerMask.NameToLayer( "Ignore Raycast" );
        }
    }

    public IEnumerator WaitForJump()
    {
        yield return new WaitForSeconds( 5f );

        // Putting back brake and brakeAt at default values
        seekBehaviour.brake *= 10f;
        seekBehaviour.brakeAt *= 10f;

        CarOnRamp = false;

        jumpTaken = true;

        // Putting back the steer value to its "default" value
        gameObject.GetComponent<AvoidBehaviourVolume>().steer = 50;

        // Putting back the gas value to its "default" value
        gameObject.GetComponent<SeekBehaviour>().gas *= 0.5f;
    }

    public IEnumerator WaitForCoinTaken()
    {
        yield return new WaitForSeconds( 5f );

        // Putting back brake and brakeAt at default values
        seekBehaviour.brake *= 10f;
        seekBehaviour.brakeAt *= 10f;

        // Putting back the steer value to its "default" value
        gameObject.GetComponent<AvoidBehaviourVolume>().steer = 50;

        // Putting back the gas value to its "default" value
        gameObject.GetComponent<SeekBehaviour>().gas *= 0.5f;

        CarOnRamp = false;

        if ( !coinTaken )
        {
            StopPickCoinBT();

            PickCoinStartCoroutine();
        }
    }

    public IEnumerator WaitForMidPadReached()
    {
        yield return new WaitForSeconds( 4f );

        if ( gameObject.transform.position.y < -5f )
        {
            // Putting back brake and brakeAt at default values
            seekBehaviour.brake *= 10f;
            seekBehaviour.brakeAt *= 10f;

            StopPickCoinBT();

            CarOnRamp = false;

            PickCoinStartCoroutine();
        }
    }

    public GameObject FindEnemy()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag( "car" );
        foreach ( GameObject go in cars )
        {
            if ( go.name != "AICar" )
                return go;
        }
        return null;
    }

    public CRBT.BehaviorTree MoveAroundMapBTBuilder()
    {
        CRBT.BTCondition c7 = new CRBT.BTCondition( DestinationFound );
        CRBT.BTCondition c8 = new CRBT.BTCondition( DistanceFromDestination );

        CRBT.BTAction a7 = new CRBT.BTAction( PickRandomDestination );
        CRBT.BTAction a8 = new CRBT.BTAction( MoveToDestination );
        CRBT.BTAction a9 = new CRBT.BTAction( ResetDestination );

        CRBT.BTSelector sel3 = new CRBT.BTSelector( new CRBT.IBTTask[] { c7, a7 } );

        CRBT.BTDecoratorUntilFail uf3 = new CRBT.BTDecoratorUntilFail( c8 );

        CRBT.BTSequence seq3 = new CRBT.BTSequence( new CRBT.IBTTask[] { sel3, a8, uf3, a9 } );

        CRBT.BTDecoratorUntilFail uf4 = new CRBT.BTDecoratorUntilFail( seq3 );

        return new CRBT.BehaviorTree( uf4 );
    }

    public CRBT.BehaviorTree PickCoinBTBuilder()
    {
        CRBT.BTAction a3 = new CRBT.BTAction( GetRampPads );
        CRBT.BTAction a4 = new CRBT.BTAction( MoveToRamp );
        CRBT.BTAction a5 = new CRBT.BTAction( MoveToMidPad );
        CRBT.BTAction a6 = new CRBT.BTAction( MoveToCoin );

        CRBT.BTCondition c4 = new CRBT.BTCondition( NearestPadFound );
        CRBT.BTCondition c2 = new CRBT.BTCondition( DistanceFromBasePad );
        CRBT.BTCondition c6 = new CRBT.BTCondition( DistanceFromMidPad );

        CRBT.BTSelector sel2 = new CRBT.BTSelector( new CRBT.IBTTask[] { c4, a3 } );

        CRBT.BTDecoratorUntilFail uf1 = new CRBT.BTDecoratorUntilFail( c2 );
        CRBT.BTDecoratorUntilFail uf2 = new CRBT.BTDecoratorUntilFail( c6 );

        CRBT.BTSequence seq2 = new CRBT.BTSequence( new CRBT.IBTTask[] { sel2, a4, uf1, a5, uf2, a6 } );

        return new CRBT.BehaviorTree( seq2 );
    }

    public CRBT.BehaviorTree JumpForHypeBTBuilder()
    {
        CRBT.BTAction a3 = new CRBT.BTAction( GetRampPads );
        CRBT.BTAction a4 = new CRBT.BTAction( MoveToRamp );
        CRBT.BTAction a5 = new CRBT.BTAction( MoveToMidPad );
        CRBT.BTAction a6 = new CRBT.BTAction( Jump );

        CRBT.BTCondition c4 = new CRBT.BTCondition( NearestPadFound );
        CRBT.BTCondition c2 = new CRBT.BTCondition( DistanceFromBasePad );
        CRBT.BTCondition c6 = new CRBT.BTCondition( DistanceFromMidPad );

        CRBT.BTSelector sel2 = new CRBT.BTSelector( new CRBT.IBTTask[] { c4, a3 } );

        CRBT.BTDecoratorUntilFail uf1 = new CRBT.BTDecoratorUntilFail( c2 );
        CRBT.BTDecoratorUntilFail uf2 = new CRBT.BTDecoratorUntilFail( c6 );

        CRBT.BTSequence seq2 = new CRBT.BTSequence( new CRBT.IBTTask[] { sel2, a4, uf1, a5, uf2, a6 } );

        return new CRBT.BehaviorTree( seq2 );
    }

    public CRBT.BehaviorTree AttackBTBuilder()
    {
        CRBT.BTAction a1 = new CRBT.BTAction( Chase );
        CRBT.BTAction a2 = new CRBT.BTAction( KeepDistance );

        CRBT.BTCondition c1 = new CRBT.BTCondition( MyResistanceGreaterThanHis );

        CRBT.BTSequence seq1 = new CRBT.BTSequence( new CRBT.IBTTask[] { c1, a1 } );

        CRBT.BTSelector sel1 = new CRBT.BTSelector( new CRBT.IBTTask[] { seq1, a2 } );

        return new CRBT.BehaviorTree( sel1 );
    }

    void Start()
    {
        enemyCar = FindEnemy();
        generalCar = gameObject.GetComponent<GeneralCar>();
        seekBehaviour = gameObject.GetComponent<SeekBehaviour>();
        fleeBehaviour = gameObject.GetComponent<FleeBehaviour>();

        AttackBT = AttackBTBuilder();

        PickCoinBT = PickCoinBTBuilder();

        MoveAroundMapBT = MoveAroundMapBTBuilder();

        JumpForHypeBT = JumpForHypeBTBuilder();

        #region General FSM

        #region FSM Transitions

        FSMTransition t1 = new FSMTransition( EnemiesInRange );
        FSMTransition t2 = new FSMTransition( NoEnemiesInRange );
        FSMTransition t3 = new FSMTransition( CoinInRangeAndCoinNotTaken );
        FSMTransition t4 = new FSMTransition( CoinTakenCondition );
        FSMTransition t5 = new FSMTransition( JumpInRangeAndHypeNotFull );
        FSMTransition t6 = new FSMTransition( JumpTaken );
        #endregion

        #region FSM States

        FSMState moveAroundMap = new FSMState();
        moveAroundMap.enterActions.Add( MoveAroundMapStartCoroutine );
        moveAroundMap.exitActions.Add( StopMoveAroundMapBT );

        FSMState jumpForHype = new FSMState();
        jumpForHype.enterActions.Add( JumpForHypeStartCoroutine );
        jumpForHype.exitActions.Add( StopJumpForHypeBT );

        FSMState pickCoin = new FSMState();
        pickCoin.enterActions.Add( PickCoinStartCoroutine );
        pickCoin.exitActions.Add( StopPickCoinBT );

        FSMState attack = new FSMState();
        attack.enterActions.Add( AttackStartCoroutine );
        attack.exitActions.Add( StopAttackBT );

        // Link states with transitions
        moveAroundMap.AddTransition( t1, attack );
        moveAroundMap.AddTransition( t3, pickCoin );
        moveAroundMap.AddTransition( t5, jumpForHype );

        pickCoin.AddTransition( t4, moveAroundMap );
        pickCoin.AddTransition( t1, attack );

        jumpForHype.AddTransition( t6, moveAroundMap );
        jumpForHype.AddTransition( t1, attack );

        attack.AddTransition( t2, moveAroundMap );
        #endregion

        #endregion

        generalFSM = new FSM( moveAroundMap );
        StartCoroutine( MoveThroughFSM() );

    }

    #region Stop BTs

    public void StopAttackBT()
    {
        StopCoroutine( attackCR );
        attackCR = null;
        seekBehaviour.destination = null;
        fleeBehaviour.destination = null;
        resetAttackBT = true;
    }

    public void StopPickCoinBT()
    {
        StopCoroutine( pickCoinCR );
        pickCoinCR = null;
        seekBehaviour.destination = null;
        resetPickCoinBT = true;

        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( "ramp" ) )
        {
            // Re-enabling raycast detection
            go.layer = LayerMask.NameToLayer( "Default" );
        }
    }

    public void StopMoveAroundMapBT()
    {
        StopCoroutine( moveAroundMapCR );
        moveAroundMapCR = null;
        Destroy( destination );
        destination = null;
        seekBehaviour.destination = null;
        resetMoveAroundMapBT = true;
    }

    public void StopJumpForHypeBT()
    {
        StopCoroutine( jumpForHypeCR );
        jumpForHypeCR = null;
        seekBehaviour.destination = null;
        jumpTaken = false;
        resetJumpForHypeBT = true;

        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( "ramp" ) )
        {
            // Re-enabling raycast detection
            go.layer = LayerMask.NameToLayer( "Default" );
        }
    }

    #endregion

    #region Coroutines Launchers

    public IEnumerator AttackLauncherCR()
    {
        while ( AttackBT.Step() )
            yield return new WaitForSeconds( reactionTime );
    }

    public IEnumerator PickCoinLauncherCR()
    {
        while ( PickCoinBT.Step() )
            yield return new WaitForSeconds( reactionTime );
    }

    public IEnumerator MoveAroundMapLauncherCR()
    {
        while ( MoveAroundMapBT.Step() )
            yield return new WaitForSeconds( reactionTime );
    }

    public IEnumerator JumpForHypeLauncherCR()
    {
        while ( JumpForHypeBT.Step() )
            yield return new WaitForSeconds( reactionTime );
    }

    public void MoveAroundMapStartCoroutine()
    {
        if ( resetMoveAroundMapBT )
        {
            MoveAroundMapBT = MoveAroundMapBTBuilder();
            resetMoveAroundMapBT = false;
        }

        moveAroundMapCR = StartCoroutine( MoveAroundMapLauncherCR() );
    }

    public void AttackStartCoroutine()
    {
        if ( resetAttackBT )
        {
            AttackBT = AttackBTBuilder();
            resetAttackBT = false;
        }

        attackCR = StartCoroutine( AttackLauncherCR() );
    }

    public void PickCoinStartCoroutine()
    {
        if ( resetPickCoinBT )
        {
            PickCoinBT = PickCoinBTBuilder();
            resetPickCoinBT = false;
        }

        pickCoinCR = StartCoroutine( PickCoinLauncherCR() );
    }

    public void JumpForHypeStartCoroutine()
    {
        if ( resetJumpForHypeBT )
        {
            JumpForHypeBT = JumpForHypeBTBuilder();
            resetJumpForHypeBT = false;
        }

        jumpForHypeCR = StartCoroutine( JumpForHypeLauncherCR() );
    }

    #endregion

    // The coroutine that cycles through the FSM
    public IEnumerator MoveThroughFSM()
    {
        while ( true )
        {
            generalFSM.Update();
            yield return new WaitForSeconds( reactionTime );
        }
    }

    void Update()
    {
        if ( !enemyCar )
            enemyCar = FindEnemy();
    }
}
