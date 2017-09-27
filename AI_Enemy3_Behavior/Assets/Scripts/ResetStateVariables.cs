using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ResetStateVariables : Action
{
    // The transform that the object is moving towards
    CharacterData chaData;
    public SharedBool beSnipped;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }

    public override TaskStatus OnUpdate()
    {
        chaData.beShotShooter = null;
        beSnipped.Value = false;
        return TaskStatus.Success;
    }
}
