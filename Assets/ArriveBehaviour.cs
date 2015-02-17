using UnityEngine;
using BGE;

public class ArriveBehaviour : MonoBehaviour {

	public Vector3 Position = new Vector3();
	public Vector3 TargetPos = new Vector3();
	public Vector3 seekTarget;
	public Vector3 arriveTarget;
	public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 force;
    public float mass;
    public float maxSpeed;
	public bool ArriveEnabled;
	public bool PursuitEnabled;
	public bool SeekEnabled;
	
	public GameObject offsetPursuitTarget;
	public bool offsetPursuitEnabled;
	public Vector3 Offset;
	public GameObject offsetPursueTarget;
	public GameObject pursueTarget;
	
	public Path path;

    public bool pathFollowingEnabled;
    public bool Looped;
	// Use this for initialization
	
	public ArriveBehaviour(){
		mass = 1;
        velocity = Vector3.zero;
        force = Vector3.zero;
        acceleration = Vector3.zero;
        maxSpeed = 10.0f;

        path = new Path();
        Looped = false;
	}
	void Start () {
		transform.position = new Vector3 (0,0,0);
		TargetPos = new Vector3 (30, 20, -10);


		if(offsetPursuitEnabled){
			if(offsetPursuitTarget != null){
				Offset = offsetPursuitTarget.transform.position - transform.position;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (PursuitEnabled)
		{
			force += pursuit(pursueTarget);
		}
		if (SeekEnabled)
		{
			force += seek(seekTarget);
		}        
		if (ArriveEnabled)
		{
			force += arrive(arriveTarget);
		}
		
		if (offsetPursuitEnabled)
		{
			force += OffsetPursuit(offsetPursueTarget);
		}
		if (pathFollowingEnabled)
		{
			path.Draw();
			force += FollowPath();
		}
		acceleration =  force / mass;
		velocity += acceleration * Time.deltaTime;
		Vector3.ClampMagnitude(velocity, maxSpeed);
		
		
		
		transform.position += velocity * Time.deltaTime;
		
		if (velocity.magnitude > float.Epsilon)
		{
			transform.forward = velocity.normalized;
			velocity *= 0.99f;
		}
		
		force = Vector3.zero;
	}

	Vector3 FollowPath()
	{
		Vector3 next = path.NextWaypoint();
		float dist = (transform.position - next).magnitude;
		float waypointDistance = 5;
		if (dist < waypointDistance)
		{
			next = path.Advance();
		}
		if (! path.Looped && path.IsLast())
		{
			return arrive(next);
		}
		else
		{
			return seek(next);
		}
	}
	Vector3 arrive(Vector3 arriveTarget){
		Vector3 toTarget = arriveTarget - transform.position;
		
		float distance = toTarget.magnitude;
		
		float slowingDistance = 10;
	
		
		float ramped = (distance / slowingDistance) * maxSpeed;
		float clamped = Mathf.Min(ramped, maxSpeed);
		Vector3 desired = (toTarget / distance) * clamped;
		return desired - velocity;
	}
	Vector3 seek(Vector3 seekTarget){
		Vector3 desired = seekTarget - transform.position;
		desired.Normalize();
		desired *= maxSpeed;
		LineDrawer.DrawTarget(seekTarget, Color.blue);
		return desired - velocity;
	}
	Vector3 pursuit(GameObject pursueTarget){
		Vector3 toTarget = pursueTarget.transform.position - transform.position;
		float distance = toTarget.magnitude;
		
		float time = distance / maxSpeed;
		Vector3 target =
			pursueTarget.transform.position +
				pursueTarget.GetComponent<ArriveBehaviour>().velocity * time;
		Debug.DrawLine(target, target + Vector3.forward);
		return seek(target);
	}
	Vector3 OffsetPursuit(GameObject offsetPursueTarget){
		Vector3 targetPos = offsetPursueTarget.transform.TransformPoint(Offset);
		
		Vector3 toTarget = targetPos - transform.position;
		float distance = toTarget.magnitude;
		float time = distance / maxSpeed;
		Vector3 target = targetPos
			+ offsetPursueTarget.GetComponent<ArriveBehaviour>().velocity * time;
		
		
		return arrive(target);
	}
}
