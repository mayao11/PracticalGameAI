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
                    var pos = new Vector3(targetPos.Value.x, transform.position.y, targetPos.Value.z);
                    if (Vector3.SqrMagnitude(transform.position - targetPos.Value) < 0.1f)
                    {
                        return TaskStatus.Success;
                    }
                    // We haven't reached the target yet so keep moving towards it
                    //transform.position = Vector3.MoveTowards(transform.position, targetPos.Value, chaData.speed * Time.deltaTime);
                    nav.SetDestination(targetPos.Value);
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
            case MovePositionType.Cover:
                {
                    Vector3 pos;
                    if (invadeDegree.Value < 75)
                    {
                        pos = GameObject.Find("CoverPos3").transform.position;
                    }
                    else if (invadeDegree.Value < 105)
                    {
                        pos = GameObject.Find("CoverPos2").transform.position;
                    }
                    else
                    {
                        pos = GameObject.Find("CoverPos1").transform.position;
                    }
                    pos = new Vector3(pos.x, pos.y, pos.z);
                    pos.y = transform.position.y;
                    if (Vector3.Distance(transform.position, pos) < 0.1f)
                    {
                        return TaskStatus.Success;
                    }
                    Debug.Log("----"+ Vector3.Distance(transform.position, pos));
                    //transform.position = Vector3.MoveTowards(transform.position, pos, chaData.speed * Time.deltaTime);
                    nav.SetDestination(pos);
                    return TaskStatus.Running;
                }
            default:
                return TaskStatus.Failure;
        }
    }
}
