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
	[Range(0f, 50f)] public float enemyRange = 40f;
	[Range(0f, 50f)] public float coinRange = 20f;
	[Range(0f, 50f)] public float jumpRange = 10f;
	public float reactionTime = .5f;
	public string coinTag = "coin";
	public string jumpTag = "jumpPad";

	public GameObject enemyCar = null;
	private GeneralCar generalCar;
	private SeekBehaviour seekBehaviour;
	private FleeBehaviour fleeBehaviour;

	private float maxHypeValue = 1000f;
	private bool coinTaken = false;

	// Must be changed to true after the jump is done to exit the state
	// Must be change back to false when leaving the state
	private bool jumpTaken = false;

	// To stop chasing a player in the BT if the FSM changes state
	private bool mustChase = false;

	// Same
	private bool mustKeepDistance = false;

	private CRBT.BehaviorTree AttackBT;

	// General FSM
	private FSM generalFSM;


	// FSM CONDITIONS
	public bool EnemiesInRange()
	{
		if ((enemyCar.transform.position - transform.position).magnitude <= enemyRange) return true;
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
			if (((go.transform.position - transform.position).magnitude <= enemyRange) && !coinTaken) return true;
		}
		return false;
	}

	public bool CoinTaken()
	{
		if (coinTaken)
		{
			return true;
		}
		return false;
	}

	public bool JumpInRangeAndHypeNotFull()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(jumpTag))
		{
			if (((go.transform.position - transform.position).magnitude <= enemyRange) && (generalCar.Hype < maxHypeValue))
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

	// BT CONDITIONS
	public bool MyResistanceGreaterThanHis()
	{
		if (generalCar.Defense >= enemyCar.GetComponent<GeneralCar>().Defense)
			return true;
		return false;
	}

	// BT ACTIONS
	// TODO stop the chase when the FSM change state by removing the enemyCar.transform from seekBeahaviour.destination
	public bool Chase()
	{
		// To stop fleeing and start chasing
		fleeBehaviour.destination = null;
		seekBehaviour.destination = enemyCar.transform;
		return true;
	}

	// TODO stop the flee when the FSM change state by removing the enemyCar.transform from fleeBeahaviour.destination
	public bool KeepDistance()
	{
		// To stop chasing and start fleeing
		seekBehaviour.destination = null;
		fleeBehaviour.destination = enemyCar.transform;
		return true;
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

		CRBT.BTDecoratorUntilFail uf1 = new CRBT.BTDecoratorUntilFail(seq1);
		CRBT.BTDecoratorUntilFail uf2 = new CRBT.BTDecoratorUntilFail(a2);

		CRBT.BTSelector sel1 = new CRBT.BTSelector(new CRBT.IBTTask[] { uf1, uf2 });

		AttackBT = new CRBT.BehaviorTree(sel1);

		// General FSM
		FSMState moveAroundMap = new FSMState();
		// TODO define the action of moving around the map

		FSMState jumpForHype = new FSMState();
		// same

		FSMState pickCoin = new FSMState();
		// same

		FSMState attack = new FSMState();
		attack.stayActions.Add( AttackCRLauncher );
		attack.exitActions.Add( stopAttackBT );

	}

	public IEnumerator AttackCR()
	{
		while (AttackBT.Step())
			yield return new WaitForSeconds(reactionTime);
	}

	// Wrapper just to add the coroutine to a FSMAction
	public void AttackCRLauncher()
	{
		StartCoroutine( AttackCR() );
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

    // Update is called once per frame
    void Update()
    {
		if (!enemyCar)
			enemyCar = FindEnemy();
    }
}
