//----------------------------------------------------------------------------------------------
// Object with this script hold waypoints as path and visualize it
// If list of waypoints is empty - Script will try to gather all child objects as waypoints on start
//----------------------------------------------------------------------------------------------

#pragma strict


var color: Color = Color(0,1,0, 0.5);     // Debug path lines color
var waypoints: Waypoint[];				  // List of all waypoints assigned to this path
var colorizeWaypoints: boolean = true;    // Repaint all waypoints in the color


//=============================================================================================================
// If list of waypoints is empty - try to gather all child objects(with waypoint script attached) as waypoints
function Start () 
{

 if (waypoints.Length == 0)
	 {
	   waypoints = new Array(transform.childCount);
	   
	   var childrenWaypoints: Component[] = GetComponentsInChildren (Waypoint);
	   
	   var i: int = 0;
	   for (var waypoint : Waypoint in childrenWaypoints) 
	      {
	        waypoints[i] = waypoint;
	        i++;
	      }
	  }
}
//----------------------------------------------------------------------------------
function AddWaypoint () 
{
  if (waypoints.Length == 0)  waypoints = new Array(0);
 
  var array = new Array (waypoints);
  array.length++;
					    
  waypoints = array.ToBuiltin(Waypoint) as Waypoint[];
  if (waypoints.Length > 1) waypoints[waypoints.Length - 1] = new Waypoint();

}

//----------------------------------------------------------------------------------
function DeleteWaypoint () 
{
  if (waypoints.Length > 0)
	 {
	   var array = new Array (waypoints);
	   array.length--;
						    
	   waypoints = array.ToBuiltin(Waypoint) as Waypoint[];
	 }
	 
}

//----------------------------------------------------------------------------------
// Draw debug visualization
function OnDrawGizmos() 
{
  Gizmos.color = color;
  
  if (waypoints.Length > 0)
    for (var i=0; i<(waypoints.Length-1); i++)
      if (waypoints[i] && waypoints[i+1])  
         {
           Gizmos.DrawLine (waypoints[i].gameObject.transform.position, waypoints[i+1].gameObject.transform.position);
           if (colorizeWaypoints) waypoints[i+1].color = color;
         }
 }
 
//----------------------------------------------------------------------------------