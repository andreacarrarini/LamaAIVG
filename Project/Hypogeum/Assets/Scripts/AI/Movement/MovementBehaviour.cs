using System;
using UnityEngine;

public abstract class MovementBehaviour : MonoBehaviour {
	public abstract Vector3 GetAcceleration (MovementStatus status);
}