using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public enum RotatePosType
{
    Base,
}

public class RotatePos : Action
{
    // The speed of the object
    CharacterData chaData;

    public RotatePosType type;

    public override void OnStart()
	{
        chaData = gameObject.GetComponent<CharacterData>();
	}

	public override TaskStatus OnUpdate()
	{
        switch(type)
        {
            case RotatePosType.Base:
                chaData.RotateTo(chaData.basePosition + chaData.baseForward);
                break;
        }
		return TaskStatus.Running;
	}
}