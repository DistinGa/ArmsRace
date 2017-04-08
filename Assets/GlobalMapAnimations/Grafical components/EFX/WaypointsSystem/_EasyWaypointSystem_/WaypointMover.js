//----------------------------------------------------------------------------------
// The most important script. 
// It manages waypointed path from waypointsHolder and move object along it.
// Script also allows to setup different following and looping types for  movement.
//----------------------------------------------------------------------------------

#pragma strict

// List of following types
public enum FollowType 
 { 
   Simple,		  // Just move object as it is (without any rotation or dumping)
   Facing,		  // Roughly face object on current waypoint
   SmoothFacing,  // Face object on current waypoint and adapt path smoothly 
   SmoothDamping, // Speed will be decreased before each waypoint and accelerate after it 
   Simple2D,      // Simple movementt in 2D
   Facing2D,      // Roughly face object on current waypoint in 2D
   SmoothFacing2D,// Smoothly face object on current waypoint in 2D
   Teleport,		  // Simply teleport (transfer immediately) object to current waypoin position
   InvertedFacing
 }

// List of looping types
public enum LoopType 
 { 
   Once,		 // Only one cycle
   Cycled,		 // Infinite amounts of cycles
   PingPong,	 // Move object in another direction when it gets first/last point of path
   SeveralTimes  // Repeat loop several times (specified in numberOfLoops)
 }

// List of axes waypoint position along which should be ignored
class UsedAxis
{
  var x: boolean = false;
  var y: boolean = false;
  var z: boolean = false;
}
 
 
 
var waypointsHolder: WaypointsHolder;			// Move along the path holded in this WaypointsHolder
var followingType: FollowType;					// Choose one of following type to use
var loopingType: LoopType;						// Choose one of looping type to use
var MoveOnWayImmediately: boolean = false;		// Move object immediately to the first waypoint at start
var StartFromNearestWaypoint: boolean = false;	// Start movement from the nearest waupoint
var ignorePositionAtAxis: UsedAxis;				// Ignore waypoint position along those axis
var damping = 3.0;								// Smooth facing/movement value
var movementSpeed: float = 5.0;					// Speed of object movement along the path
var waypointActivationDistance: float = 1.0;	// How far should object be to waypoint for its activation and choosing new
var numberOfLoops: int = 0;						// How much loops should be performed before stop. Use this parameter if loopingType=SeveralTimes
var preventCollisionDistance: float;            // Object suspend movement if there is any obstacle on this distance (in front of him)
var smoothCollisionPreventing: boolean = false;	// Mover will be smoothly decrease speed in front of obstacle
var dynamicWaypointsUpdate: boolean = false;    // Respect Waypoints position update dynamically


// Usefull internal variables, please don't change them blindly
private var currentWaypoint: int = 0;
private var direction: int = 1;
private var velocity = Vector3.zero;
private var targetPosition : Vector3;
private var delayTillTime: float;
private var loopNumber: int = 1;
private var inMove: boolean = false;
private var suspended: boolean = false;
private var previousWaypoint: int = 0;
private var initialMovementSpeed: float;
private var callExitFunction: boolean = false;
private var onWaypoint: boolean;


//=============================================================================================================
// Setup initial data according to specified parameters
function Start () 
{
	// Make the rigid body not change rotation
   	if (GetComponent.<Rigidbody>()) GetComponent.<Rigidbody>().freezeRotation = true;
   	 initialMovementSpeed = movementSpeed;
   	 
   	 
   	yield WaitForSeconds(0.01);
   	if(StartFromNearestWaypoint)
  	   {
  	     var waypointPosition: Vector3;
  	     var nearestWaypointID: int;
  	     var previousSmallestDistance: float = Mathf.Infinity;
  	     var distance: float;
  	     
  	     for (var i:int = 0; i < waypointsHolder.waypoints.Length; i++)
  	      {
  	         waypointPosition = waypointsHolder.waypoints[i].gameObject.transform.position;
		  	 waypointPosition = IgnorePositionByAxis(waypointPosition);
		  	 
		  	 distance = Vector3.Distance(transform.position, waypointPosition);
		  	  
		  	 if (distance < previousSmallestDistance)
		  	   {
		  	     nearestWaypointID = i;
		  	     previousSmallestDistance = distance;
		  	   }
             
		  }
		  
		  currentWaypoint = nearestWaypointID;
		  
  	   }
  	   else currentWaypoint = 0;
  	   
   	if(MoveOnWayImmediately) transform.position = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;
   	
   	targetPosition = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;
   	targetPosition = IgnorePositionByAxis(targetPosition);
  	
}

//----------------------------------------------------------------------------------
//Main loop
function Update () 
{
     
    var collisionPrevented: boolean = false;
    
    var hit : RaycastHit;
    var p1 : Vector3 = transform.position;
    var p2 : Vector3 = p1;
    
    
    // Cast character controller shape preventCollisionDistance meters forward, to see if it is about to hit anything
    if (preventCollisionDistance > 0)
	    if (Physics.CapsuleCast (p1, p2, 0.5, transform.forward, hit, preventCollisionDistance)) 
	      {
	       if (!smoothCollisionPreventing) collisionPrevented = true;
		     else // smoothly decrease speed infront of obstacle
				{
				 movementSpeed = initialMovementSpeed * Vector3.Distance(transform.position, hit.point) / preventCollisionDistance;
				 if (movementSpeed < initialMovementSpeed/preventCollisionDistance) collisionPrevented = true;
				}
		  }
		 else
		   movementSpeed = initialMovementSpeed; 
	    
    
    
    
    // Respect current Waypoint position-update dynamically (even if mover is already  on the way to this waypoint)
	    if (dynamicWaypointsUpdate  &&  currentWaypoint >= 0) 
           if(targetPosition != waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position)
               {       
                 targetPosition = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;
                 transform.LookAt(targetPosition);
                 targetPosition = IgnorePositionByAxis(targetPosition);
              }  
              
              
    
 // Process movement if waypoint exists and there is no delay assigned to it
 if (!suspended  &&  !collisionPrevented  &&  currentWaypoint >= 0  &&  delayTillTime < Time.time) 
  {
  	
  	inMove = true; 
  	
   // Activate waypoint when object is closer than waypointActivationDistance
   if(Vector3.Distance(transform.position, targetPosition) < waypointActivationDistance) 
    {
      // Init delay if it's specified in waypoint
      if (waypointsHolder.waypoints[currentWaypoint].delay>0) delayTillTime = Time.time + waypointsHolder.waypoints[currentWaypoint].delay;
      // Try to call function in object if there is any function name specified in waypoint "callFunction" parameter
      if (waypointsHolder.waypoints[currentWaypoint].callFunction != "") SendMessage (waypointsHolder.waypoints[currentWaypoint].callFunction, SendMessageOptions.DontRequireReceiver);
      
      // If waypoint have specified newMoverSpeed bigger than 0 -  change current WaipointMover speed 
      if(waypointsHolder.waypoints[currentWaypoint].newMoverSpeed > 0) ChangeWaypointMoverSpeed(waypointsHolder.waypoints[currentWaypoint].newMoverSpeed);
      
      // Select next waypoint according to direction 
      previousWaypoint = currentWaypoint;
      currentWaypoint += direction;
      onWaypoint = true;
      // currentWaypoint = Random.Range(0, waypointsHolder.waypoints.Length);
       
       
      // Choose next waypoint/actions according to loopingType, if object reaches first or last  waypoint
      if(currentWaypoint > waypointsHolder.waypoints.Length-1 || currentWaypoint<0)
  	    switch (loopingType)
  		   {
			  case LoopType.Once: 
			    currentWaypoint = -1;
			  break;
			  
			  case LoopType.Cycled:
			  	currentWaypoint = currentWaypoint < 0 ? waypointsHolder.waypoints.Length-1 : 0; 
			  break;
			  
			  case LoopType.PingPong:
			  	 direction = -direction;
			  	 currentWaypoint += direction;
			  break;
			  
			  case LoopType.SeveralTimes:
			    if (loopNumber < numberOfLoops) 
			       {
			        currentWaypoint = 0;
			        loopNumber++;
			       }
			        else
			         currentWaypoint = -1;
			  break;
   		   }
   		   

   	  // Get/update next waypoint XYZ position in World coordinates
	   if(currentWaypoint >= 0 && waypointsHolder.waypoints[currentWaypoint])
	   	    {  	 
		   	  targetPosition = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;
		   	  targetPosition = IgnorePositionByAxis(targetPosition); 
	  	    }
	  	   else
		     if (currentWaypoint < waypointsHolder.waypoints.Length && currentWaypoint>=0)
		          { 
			        currentWaypoint -= direction;
			        Debug.LogWarning("Waypoint is missed in " + waypointsHolder.gameObject.name);
			      }
			      
		callExitFunction = true;	      
  			   	
   }
    else  // When object leaves waypoint - try to call function(specified in waypoint "callExitFunction" parameter) in object
     {
      onWaypoint = false;
       
      if(waypointsHolder.waypoints[previousWaypoint].callExitFunction != ""  &&  callExitFunction)
        if(Vector3.Distance(transform.position, waypointsHolder.waypoints[previousWaypoint].gameObject.transform.position) < waypointActivationDistance) 
           {
             SendMessage (waypointsHolder.waypoints[previousWaypoint].callExitFunction, SendMessageOptions.DontRequireReceiver); 
             callExitFunction = false;
           }
     }
             
    
// Choose or update rotation/facing according to facingType
	
	 // Look at and dampen the rotation
	  if (followingType == FollowType.SmoothFacing)
	   {
		 var rotation = Quaternion.LookRotation(targetPosition - transform.position);
		 transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
			
		 transform.Translate(Vector3.forward*movementSpeed*Time.deltaTime);
	   }
		
	 // Just Look at	
	  if (followingType == FollowType.Facing)
	   {
		 transform.LookAt(targetPosition);
		 transform.Translate(Vector3.forward*movementSpeed*Time.deltaTime);

	   }
		
	 // Inverted Look at	
	  if (followingType == FollowType.InvertedFacing)
	   {
		 transform.LookAt(targetPosition);
		 transform.Rotate(Vector3.up, 180);
		 transform.Translate(-Vector3.forward*movementSpeed*Time.deltaTime);


	   }

	 // Move without rotation
	  if (followingType == FollowType.Simple)  transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);   
	  
	  if (followingType == FollowType.SmoothDamping) transform.position = Vector3.SmoothDamp(transform.position, targetPosition, velocity, movementSpeed);

	 
	  if (followingType == FollowType.Simple2D)
	   {
		SmoothLookAt2D(transform, targetPosition, damping);
		transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
	   }	
			
    // Roughly face object on current waypoint in 2D
	  if (followingType == FollowType.Facing2D)
	   {
		var targetDir = targetPosition - transform.position;
		var angle = Vector3.Angle(targetDir, transform.right);
		
		if(angle > 3) transform.rotation.eulerAngles.z += angle;
		    
		 transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
		 
	   }
	   
	   
    // Smoothly face object on current waypoint in 2D
	  if (followingType == FollowType.SmoothFacing2D)
	   {
		targetDir = targetPosition - transform.position;
		angle = Vector3.Angle(targetDir, transform.right);
		

		if(angle > 3) 
		   transform.rotation.eulerAngles.z = Mathf.Lerp(transform.rotation.eulerAngles.z, transform.rotation.eulerAngles.z - angle, damping * Time.deltaTime);
		
		 transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
	   } 
	   
	   if (followingType == FollowType.Teleport)  transform.position = targetPosition;
	   
 }
 else 
   inMove = false;  


  
}

//----------------------------------------------------------------------------------
// Reverse path-movement direction and sends object to previous waypoint
function ReverseDirection () 
{
	  previousWaypoint = currentWaypoint;
	  direction = -direction;
	  
	  currentWaypoint =  direction > 0 ? currentWaypoint+1 : currentWaypoint-1;
	  if (currentWaypoint < 0) currentWaypoint =  waypointsHolder.waypoints.Length-1; 
	    else 
	      if (currentWaypoint > waypointsHolder.waypoints.Length-1) currentWaypoint = 0; 
	      
	  targetPosition = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;

}

//----------------------------------------------------------------------------------
// Set path-movement direction and sends object to related waypoint
function SetDirection (dir: int) 
{
	  previousWaypoint = currentWaypoint;
	  direction = dir;
	  
	  currentWaypoint =  direction > 0 ? currentWaypoint+1 : currentWaypoint-1; 
	  if (currentWaypoint < 0) currentWaypoint =  waypointsHolder.waypoints.Length-1; 
	    else 
	      if (currentWaypoint > waypointsHolder.waypoints.Length-1) currentWaypoint = 0; 
	      
	  targetPosition = waypointsHolder.waypoints[currentWaypoint].gameObject.transform.position;

}
//----------------------------------------------------------------------------------
// Return is mover on Waypoint  or not
function IsOnWaypoint (): boolean
{
 return onWaypoint;
 
}


//----------------------------------------------------------------------------------
// Return object to position of previous waypoint
function ReturnToPreviousWaypoint () 
{
	  currentWaypoint = previousWaypoint;
	  transform.position = waypointsHolder.waypoints[previousWaypoint].gameObject.transform.position;
	  targetPosition = waypointsHolder.waypoints[previousWaypoint].gameObject.transform.position;

}

//----------------------------------------------------------------------------------
// Return true if object is moving now
function isMoving (): boolean 
{
	return inMove;
}

//----------------------------------------------------------------------------------
// Fully suspend waypoint-controlled movement
function Suspend (state: boolean) 
{
  suspended = state;
  
}

//----------------------------------------------------------------------------------
// Fully suspend waypoint-controlled movement
function Pause () 
{
  suspended = true;
  
}
//----------------------------------------------------------------------------------
// Resumes suspendede movement
function Unpause () 
{
  suspended = false;
  
}

//----------------------------------------------------------------------------------
// Set new movement speed 
function ChangeWaypointMoverSpeed (newSpeed:float) 
{
	movementSpeed = newSpeed;
	initialMovementSpeed = movementSpeed;
}

//----------------------------------------------------------------------------------
// Reset position along ignored axis to transform.position
function IgnorePositionByAxis(positionToUpdate: Vector3): Vector3
{
   var updatedPos: Vector3 = positionToUpdate;
   
	if (ignorePositionAtAxis.x) updatedPos.x = transform.position.x;
	if (ignorePositionAtAxis.y) updatedPos.y = transform.position.y;
	if (ignorePositionAtAxis.z) updatedPos.z = transform.position.z;
	
	return updatedPos;
	
}


//----------------------------------------------------------------------------------
// Smoothly LookAt targetPosition in 2D
function SmoothLookAt2D (objectTransform:Transform, targetPosition: Vector2, smoothingValue: float) 
{
 		var relative : Vector3  = objectTransform.InverseTransformPoint(targetPosition);
		var angle : float = Mathf.Atan2(relative.y, relative.x) * Mathf.Rad2Deg;
		
		objectTransform.Rotate (0, 0, Mathf.LerpAngle(0, angle, Time.deltaTime * smoothingValue) );
  
 }
//----------------------------------------------------------------------------------
