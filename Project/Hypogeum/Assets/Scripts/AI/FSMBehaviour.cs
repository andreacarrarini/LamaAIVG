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
	[Range(0f, 1000f)] public float enemyRange;
	[Range(0f, 10000f)] public float coinRange;
	[Range(0f, 50f)] public float jumpRange;
	[Range(0f, 50f)] public float padDistance;
	public float reactionTime = .5f;
	private string coinTag = "coin";
	private string jumpTag = "jumpPad";
    private string baseTag = "basePad";

	private GameObject enemyCar = null;
	private GeneralCar generalCar;
	private SeekBehaviour seekBehaviour;
	private FleeBehaviour fleeBehaviour; 
    public GameObject nearestBasePad, nearestMidPad, nearestJumpPad, nearestCoin, destination = null;

	private float maxHypeValue = 1000f;
	private bool coinTaken = false;

	// Must be changed to true after the jump is done to exit the state
	// Must be change back to false when leaving the state
	private bool jumpTaken = false;

	// To stop chasing a player in the BT if the FSM changes state
	//private bool mustChase = false;

	// Same
	//private bool mustKeepDistance = false;

	// FSMStates Behaviour Trees
	private CRBT.BehaviorTree AttackBT, PickCoinBT, MoveAroundMapBT;

	// General FSM
	private FSM generalFSM;

	// Properties
	public bool CoinTaken { get => coinTaken; set => coinTaken = value; }
    public bool CarOnRamp { get => carOnRamp; set => carOnRamp = value; }

    // List of ramps in the arena
    private GameObject[] ramps;

    // To try to avoid the condition checking distance in the UF of PicoCoinBT
    ////public bool basePadReached, midPadReached = false;

    // To activate the check on how many wheels are on the ground
    private bool carOnRamp = false;

    // To move randomly around the map
    //float randomX, randomZ;


    #region FSM COndition
    public bool EnemiesInRange()
	{
		if ((enemyCar.transform.position - transform.position).magnitude <= enemyRange)
			return true;
		return false;
	}

	public bool NoEnemiesInRange()
	{
		return !EnemiesInRange();
	}

	public bool CoinInRangeAndCoinNotTaken()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(coinTag))
		{
			if (((go.transform.position - transform.position).magnitude <= coinRange) && !CoinTaken)
			{
				nearestCoin = go;
				return true;
			}
		}
		return false;
	}

	public bool CoinTakenCondition()
	{
		if (CoinTaken)
		{
			return true;
		}
		return false;
	}

	public bool JumpInRangeAndHypeNotFull()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(baseTag))
		{
			if (((go.transform.position - transform.position).magnitude <= jumpRange) && (generalCar.Hype < maxHypeValue))
				return true;
		}
		return false;
	}

	public bool JumpTaken()
	{
		if (jumpTaken)
		{
			return true;
		}
		return false;
	}
	#endregion

	// BT CONDITIONS
	public bool MyResistanceGreaterThanHis()
	{
		if (generalCar.Defense >= enemyCar.GetComponent<GeneralCar>().Defense)
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

    // BT ACTIONS

    public bool Chase()
	{
		// To stop fleeing and start chasing
		if (MyResistanceGreaterThanHis())
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
		if (!MyResistanceGreaterThanHis())
		{
			seekBehaviour.destination = null;
			fleeBehaviour.destination = enemyCar.transform;
			return true;
		}
		return false;
	}

	public bool DistanceFromPad( GameObject go )
	{
		if ( (go.transform.position - gameObject.transform.position).magnitude > padDistance )
			return true;
		return false;
	}

    // Action to make the car go to the base of the ramp
    public bool MoveToRamp()
    {
        seekBehaviour.destination = nearestBasePad.transform;

        return true;
    }

    //public bool WaitForCoin()
    //{
    //    basePadReached = false;
    //    return true;
    //}

    public void IgnoreRampRaycast()
    {
        foreach ( GameObject go in GameObject.FindGameObjectsWithTag( "ramp" ) )
        {
            // Ignore Raycast
            go.layer = 2;
        }
    }

    public bool MoveToMidPad()
    {
        CarOnRamp = true;
        IgnoreRampRaycast();
        seekBehaviour.destination = nearestMidPad.transform;
        return true;
    }

    public bool MoveToCoin()
	{
		seekBehaviour.destination = nearestJumpPad.transform;

        // To avoid to steer all to right nefore taking the coin
        gameObject.GetComponent<AvoidBehaviourVolume>().steer = 10;

        // To check if the coin has been taken, if not, do it again
        StartCoroutine( WaitForCoinTaken() );

        return true;
	}

    public IEnumerator WaitForCoinTaken()
    {
        yield return new WaitForSeconds( 5f );
        if ( !coinTaken )
        {
            // Putting back the steer value to its "default" value
            gameObject.GetComponent<AvoidBehaviourVolume>().steer = 50;
            StartCoroutine( PickCoinLauncherCR() );
        }
    }

	// To know from where the coin is accessible and to move to the base of the correct ramp
	public bool FromCoinToRamp()
	{
		float minDistance = 100000f;

		foreach (GameObject go in GameObject.FindGameObjectsWithTag(baseTag))
		{
			if ((go.transform.position - gameObject.transform.position).magnitude < minDistance)
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

    // Not anymore for an Until Fail, now it's for a Selector
    //public bool BasePadReached()
    //{
    //    if ( basePadReached )
    //        return true;
    //    return false;
    //}

    //public bool MidPadReached()
    //{
    //    if ( midPadReached )
    //        return true;
    //    return false;
    //}

    // For the first Selector in the Pick Coin BT
    public bool NearestPadFound()
    {
        if ( nearestJumpPad )
            return true;
        return false;
    }

    #region Move Around Map Tasks
    public bool DestinationFound()
    {
        if ( !destination )
            return false;
        return true;
    }

    // Can be improved
    public bool PickRandomDestination()
    {
        float randomX, randomZ;
        
        if (Random.value < 0.5)
        {
            randomX = Random.value * -250;
            randomZ = Random.value * -230;
        }
        randomX = Random.value * 250;
        randomZ = Random.value * 230;

        destination = Instantiate( new GameObject(), new Vector3( randomX, -6, randomZ ), new Quaternion() );

        return true;
    }

    public bool MoveToDestination()
    {
        seekBehaviour.destination = destination.transform;

        return true;
    }

    public bool DistanceFromDestination()
    {
        if ( (destination.transform.position - gameObject.transform.position).magnitude > 20 )
            return true;
        return false;
    }

    public bool ResetDestination()
    {
        Destroy( destination );
        destination = null;
        seekBehaviour.destination = null;

        return true;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
		enemyCar = FindEnemy();
		generalCar = gameObject.GetComponent<GeneralCar>();
		seekBehaviour = gameObject.GetComponent<SeekBehaviour>();
		fleeBehaviour = gameObject.GetComponent<FleeBehaviour>();


        #region Attack BT
        CRBT.BTAction a1 = new CRBT.BTAction( Chase );
		CRBT.BTAction a2 = new CRBT.BTAction( KeepDistance );

		CRBT.BTCondition c1 = new CRBT.BTCondition( MyResistanceGreaterThanHis );

		CRBT.BTSequence seq1 = new CRBT.BTSequence( new CRBT.IBTTask[] { c1, a1 }) ;

		CRBT.BTSelector sel1 = new CRBT.BTSelector( new CRBT.IBTTask[] { seq1, a2 } );

		AttackBT = new CRBT.BehaviorTree( sel1 );
        #endregion

        #region Pick Coin BT
        CRBT.BTAction a3 = new CRBT.BTAction( FromCoinToRamp );
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

        PickCoinBT = new CRBT.BehaviorTree( seq2 );
        #endregion

        #region Move Around Map BT
        CRBT.BTCondition c7 = new CRBT.BTCondition( DestinationFound );
        CRBT.BTCondition c8 = new CRBT.BTCondition( DistanceFromDestination );

        CRBT.BTAction a7 = new CRBT.BTAction( PickRandomDestination );
        CRBT.BTAction a8 = new CRBT.BTAction( MoveToDestination );
        CRBT.BTAction a9 = new CRBT.BTAction( ResetDestination );

        CRBT.BTSelector sel3 = new CRBT.BTSelector( new CRBT.IBTTask[] { c7, a7 } );

        CRBT.BTDecoratorUntilFail uf3 = new CRBT.BTDecoratorUntilFail( c8 );

        CRBT.BTSequence seq3 = new CRBT.BTSequence( new CRBT.IBTTask[] { sel3, a8, uf3, a9 } );

        CRBT.BTDecoratorUntilFail uf4 = new CRBT.BTDecoratorUntilFail( seq3 );

        MoveAroundMapBT = new CRBT.BehaviorTree( uf4 );
        #endregion

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
		// same

		FSMState pickCoin = new FSMState();
        pickCoin.enterActions.Add( PickCoinStartCoroutine );
        //pickCoin.stayActions.Add( PickCoinLauncher );
        //pickCoin.stayActions.Add( PickCoinLauncherWrapper );
        pickCoin.exitActions.Add( StopPickCoinBT );

		FSMState attack = new FSMState();
        attack.enterActions.Add( AttackStartCoroutine );
        //attack.stayActions.Add( AttackLauncher );
        //attack.stayActions.Add( AttackLauncherWrapper );
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

    //public void PickCoinLauncher()
    //{
    //    PickCoinBT.Step();
    //}

    public void StopAttackBT()
    {
        seekBehaviour.destination = null;
        fleeBehaviour.destination = null;
    }

    public void StopPickCoinBT()
    {
        seekBehaviour.destination = null;
        nearestCoin = null;
        nearestBasePad = null;
        nearestMidPad = null;
        nearestJumpPad = null;
    }

    public void StopMoveAroundMapBT()
    {
        destination = null;
    }

    #region try with CR
    // Try with CR stopping every step
    public IEnumerator AttackLauncherCR()
    {
        while (AttackBT.Step())
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

    //public void AttackLauncherWrapper()
    //{
    //    AttackLauncherCR();
    //}

    //public void PickCoinLauncherWrapper()
    //{
    //    PickCoinLauncherCR();
    //}

    public void MoveAroundMapStartCoroutine()
    {
        StartCoroutine( MoveAroundMapLauncherCR() );
    }

    public void AttackStartCoroutine()
    {
        StartCoroutine( AttackLauncherCR() );
    }

    public void PickCoinStartCoroutine()
    {
        StartCoroutine( PickCoinLauncherCR() );
    }
    #endregion

	public GameObject FindEnemy()
	{
		GameObject[] cars = GameObject.FindGameObjectsWithTag("car");
		foreach (GameObject go in cars)
		{
			if (go.name != "AICar")
				return go;
		}
		return null;
	}

    // The coroutine that cycles through the FSM
	public IEnumerator MoveThroughFSM()
	{
		while(true)
		{
			generalFSM.Update();
			yield return new WaitForSeconds( reactionTime );
		}
	}

    //public void IfBaseBadReached()
    //{
    //    if ( nearestJumpPad )
    //    {
    //        if ( !DistanceFromPad( nearestBasePad ) )
    //            basePadReached = true;
    //    }
    //}

    //public void IfMidPadReached()
    //{
    //    if ( nearestJumpPad )
    //    {
    //        if ( !DistanceFromPad( nearestMidPad ) )
    //            midPadReached = true;
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
		if (!enemyCar)
			enemyCar = FindEnemy();
        //IfBaseBadReached();
        //IfMidPadReached();
    }
}
