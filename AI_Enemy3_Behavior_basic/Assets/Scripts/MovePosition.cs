using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public enum MovePositionType
{
    Home,
    Target,
    MapPos,
    Cover,
}

public class MovePosition : Action
{
    // The speed of the object
    CharacterData chaData;
    // The transform that the object is moving towards
    public SharedTransform target;
    public SharedVector3 targetPos;
    public SharedFloat invadeDegree;

    public MovePositionType type;
    public string posName;

    public override void OnAwake()
    {
        chaData = gameObject.GetComponent<CharacterData>();
    }


    public override TaskStatus OnUpdate()
    {
        var nav = gameObject.GetComponent<NavMeshAgent>();
        // Return a task status of success once we've reached the target
        switch (type)
        {
            case MovePositionType.Home:
                if (Vector3.SqrMagnitude(transform.position - chaData.basePosition) < 0.1f)
                {
                    return TaskStatus.Success;
                }
                // We haven't reached the target yet so keep moving towards it
                //transform.position = Vector3.MoveTowards(transform.position, chaData.basePosition, chaData.speed * Time.deltaTime);
                nav.SetDestination(chaData.basePosition);
                chaData.RotateTo(chaData.basePosition);
                return TaskStatus.Running;
            case MovePositionType.Target:
                {
                    Debug.Log("Dest: " + targetPos.Value);
                    Debug.Log("Degree: " + invadeDegree.Value);
                    var pos = new Vector3(target.Value.position.x, transform.position.y, target.Value.position.z);
                    if (Vector3.SqrMagnitude(transform.position - target.Value.position) < 0.1f)
                    {
                        return TaskStatus.Success;
                    }
                    // We haven't reached the target yet so keep moving towards it
                    //transform.position = Vector3.MoveTowards(transform.position, targetPos.Value, chaData.speed * Time.deltaTime);
                    nav.SetDestination(target.Value.position);
                    return TaskStatus.Running;
                }
            case MovePositionType.MapPos:
                {
                    var p = GameObject.Find(posName).transform.position;
                    Vector3 pos = new Vector3(p.x, p.y, p.z);
                    pos.y = transform.position.y;
                    if (Vector3.SqrMagnitude(transform.position - pos) < 0.1f)
                    {
                        return TaskStatus.Success;
                    }
                    //transform.position = Vector3.MoveTowards(transform.position, pos, chaData.speed * Time.deltaTime);
                    nav.SetDestination(pos);
                    return TaskStatus.Running;
                }
            default:
                return TaskStatus.Failure;
        }
    }
}
