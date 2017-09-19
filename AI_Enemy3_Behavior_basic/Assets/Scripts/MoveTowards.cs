using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class MoveTowards : Action
{
    // The transform that the object is moving towards
    public SharedTransform target;
    CharacterData chaData;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }

    public override TaskStatus OnUpdate()
    {
        // Return a task status of success once we've reached the target
        if (Vector3.SqrMagnitude(transform.position - target.Value.position) < 0.8f)
        {
            return TaskStatus.Success;
        }

        // We haven't reached the target yet so keep moving towards it
        //transform.position = Vector3.MoveTowards(transform.position, target.Value.position, chaData.speed * Time.deltaTime);
        var nav = gameObject.GetComponent<NavMeshAgent>();
        if (Vector3.Distance(nav.destination, target.Value.position)>1)
        {
            nav.SetDestination(target.Value.position);
        }
        return TaskStatus.Running;
    }
}
