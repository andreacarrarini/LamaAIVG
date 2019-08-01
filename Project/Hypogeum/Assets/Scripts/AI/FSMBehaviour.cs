using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FSMBehaviour : MonoBehaviour
{
	[Range(0f, 50f)] public float enemyRange = 40f;
	[Range(0f, 50f)] public float coinRange = 20f;
	[Range(0f, 50f)] public float jumpRange = 10f;
	public float reactionTime = .5f;
	public string targetTag = "player";
	public string coinTag = "coin";
	public string jumpTag = "jumpPad";
	private GeneralCar generalCar;
	private float maxHypeValue = 1000f;
	private bool coinTaken = false;

	// Must be changed to true after the jump is done to exit the state
	// Must be change back to false when leaving the state
	private bool jumpTaken = false;

	// To stop chasing a player in the BT if the FSM changes state
	private bool mustChase = false;

	// Same
	private bool mustKeepDistance = false;

	private CRBT.BehaviorTree AI;


	// FSM CONDITIONS
	public bool EnemiesInRange()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(targetTag))
		{
			if ((go.transform.position - transform.position).magnitude <= enemyRange) return true;
		}
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
		foreach (GameObject go in GameObject.FindGameObjectsWithTag(targetTag))
		{
			if (generalCar.Defense >= go.GetComponent<GeneralCar>().Defense)
				return true;
		}
		return false;
	}

	// BT ACTIONS
	public bool Chase()
	{
		return true;
	}

	public bool KeepDistance()
	{
		return true;
	}

	// Start is called before the first frame update
	void Start()
    {
		CRBT.BTAction a1 = new CRBT.BTAction(Chase);
		CRBT.BTAction a2 = new CRBT.BTAction(KeepDistance);

		CRBT.BTCondition c1 = new CRBT.BTCondition(MyResistanceGreaterThanHis);

		CRBT.BTSequence seq1 = new CRBT.BTSequence(new CRBT.IBTTask[] { c1, a1 });

		CRBT.BTDecoratorUntilFail uf1 = new CRBT.BTDecoratorUntilFail(seq1);
		CRBT.BTDecoratorUntilFail uf2 = new CRBT.BTDecoratorUntilFail(a2);

		CRBT.BTSelector sel1 = new CRBT.BTSelector(new CRBT.IBTTask[] { uf1, uf2 });

		AI = new CRBT.BehaviorTree(sel1);

		StartCoroutine(AttackCR());
	}

	public IEnumerator AttackCR()
	{
		while (AI.Step())
			yield return new WaitForSeconds(reactionTime);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
