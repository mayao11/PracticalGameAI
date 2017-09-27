using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class FireAction : Action
{
    // The transform that the object is moving towards
    CharacterData chaData;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }

    public override TaskStatus OnUpdate()
    {
        chaData.Fire();
        return TaskStatus.Success;
    }
}
