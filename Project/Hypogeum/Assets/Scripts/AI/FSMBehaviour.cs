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
	[Range(0f, 1000f)] public float enemyRange = 500f;
	[Range(0f, 50f)] public float coinRange = 20f;
	[Range(0f, 50f)] public float jumpRange = 10f;
	[Range(0f, 50f)] public float padDistance = 10f;
	public float reactionTime = .5f;
	public string coinTag = "coin";
	public string jumpTag = "jumpPad";

	public GameObject enemyCar = null;
	private GeneralCar generalCar;
	private SeekBehaviour seekBehaviour;
	private FleeBehaviour fleeBehaviour;
	private GameObject nearestJumpPad = null;
	private GameObject nearestCoin = null;

	private float maxHypeValue = 1000f;
	private bool coinTaken = false;

	// Must be changed to true after the jump is done to exit the state
	// Must be change back to false when leaving the state
	private bool jumpTaken = false;

	// To stop chasing a player in the BT if the FSM changes state
	private bool mustChase = false;

	// Same
	private bool mustKeepDistance = false;

	// FSMState Attack Behaviour Tree
	private CRBT.BehaviorTree AttackBT;

	// General FSM
	private FSM generalFSM;

	// Property for coinTaken
	public bool CoinTaken { get => coinTaken; set => coinTaken = value; }


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
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(jumpTag))
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

	// Cndition for an Until Fail
	public bool DistanceFromPad()
	{
		if ( (nearestJumpPad.transform.position - gameObject.transform.position).magnitude > padDistance )
			return true;
		return false;
	}

	public bool MoveToRamp()
	{
        seekBehaviour.destination = nearestJumpPad.transform;
		return true;
	}

	public bool MoveToCoin()
	{
		seekBehaviour.destination = nearestCoin.transform;
		return true;
	}

	// To know from where the coin is accessible and to move to the base of the correct ramp
	public bool FromCoinToRamp()
	{
		float minDistance = 500f;
		GameObject nearestCoin = null;

		foreach (GameObject go in GameObject.FindGameObjectsWithTag(coinTag))
		{
			if ((go.transform.position - gameObject.transform.position).magnitude < minDistance)
			{
				minDistance = (go.transform.position - gameObject.transform.position).magnitude;
				nearestCoin = go;
			}
		}

		switch (Mathf.Ceil(nearestCoin.transform.position.y))
		{
			case 6:
				nearestJumpPad = GameObject.Find( "DragonPad" );
				break;
			case 17:
				nearestJumpPad = GameObject.Find( "WoodPad" );
				break;
			case 22:
				nearestJumpPad = GameObject.Find( "StonePad" );
				break;
			case 40:
				nearestJumpPad = GameObject.Find( "BigPad" );
				break;
		}

		if ( !nearestJumpPad )
			return true;
		return false;
	}

	// Start is called before the first frame update
	void Start()
    {
		enemyCar = FindEnemy();
		generalCar = gameObject.GetComponent<GeneralCar>();
		seekBehaviour = gameObject.GetComponent<SeekBehaviour>();
		fleeBehaviour = gameObject.GetComponent<FleeBehaviour>();

		// Atack BT
		CRBT.BTAction a1 = new CRBT.BTAction(Chase);
		CRBT.BTAction a2 = new CRBT.BTAction(KeepDistance);

		CRBT.BTCondition c1 = new CRBT.BTCondition(MyResistanceGreaterThanHis);

		CRBT.BTSequence seq1 = new CRBT.BTSequence(new CRBT.IBTTask[] { c1, a1 });

		CRBT.BTSelector sel1 = new CRBT.BTSelector(new CRBT.IBTTask[] { seq1, a2 });

		AttackBT = new CRBT.BehaviorTree(sel1);

		// Pick Coin BT
		// TODO

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
		// TODO define enter, stay and exit actions

		FSMState jumpForHype = new FSMState();
		// same

		FSMState pickCoin = new FSMState();
		// same

		FSMState attack = new FSMState();
		attack.stayActions.Add( AttackLauncher );
		attack.exitActions.Add( stopAttackBT );

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

		generalFSM = new FSM( moveAroundMap );
		StartCoroutine( MoveThroughFSM() );
		#endregion
	}

	//public IEnumerator AttackCR()
	//{
	//	while (AttackBT.Step())
	//		yield return new WaitForSeconds(reactionTime);
	//}

	//// Wrapper just to add the coroutine to a FSMAction
	//public void AttackCRLauncher()
	//{
	//	StartCoroutine( AttackCR() );
	//}

	public void AttackLauncher()
	{
		AttackBT.Step();
	}

	public void stopAttackBT()
	{
		seekBehaviour.destination = null;
		fleeBehaviour.destination = null;
	}

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

	// TODO trying to adapt to the BT slide by putting the FSM.update() in while condition as step() in the slide
	// need to change FSM.update() from void to bool
	// Edit: the coroutine needs to always cycle through the FSM ==> while(true) 
	public IEnumerator MoveThroughFSM()
	{
		while(true)
		{
			generalFSM.Update();
			yield return new WaitForSeconds( reactionTime );
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (!enemyCar)
			enemyCar = FindEnemy();
		
    }
}
