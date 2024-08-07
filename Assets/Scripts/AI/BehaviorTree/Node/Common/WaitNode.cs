using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNode : ActionNode
{
	public float duration = 1f;

	private float enterTime = 0f;

	protected override void OnStart()
	{
		enterTime = Time.time;
	}

	protected override void OnStop()
	{
	}

	protected override State OnUpdate()
	{
		if (Time.time - enterTime < duration)
		{
			return State.Running;
		}

		return State.Success;
	}
}
