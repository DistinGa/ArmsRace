//----------------------------------------------------------------------------------------------
// Basic script that contains waypoint visualisation options and associated actions
//----------------------------------------------------------------------------------------------

#pragma strict

// Debug visualization options
var color: Color;			// Waypoint gizmo color
var radius: float = 0.25;   // Waypoint gizmo size
var iconName: String; 		// Waypoint gizmo icon filename 

// Associated actions
var delay: float = 0;			// Delay movement for next waypoint, when Mover object rich the waypoint
var callFunction: String;		// Call function with this name, when Mover object rich the waypoint
var callExitFunction: String;	// Call function with this name, when Mover object leaves the waypoint
var newMoverSpeed: float;  		// If more than 0 - will change current WaipointMover speed to this

//=============================================================================================================
// Draw debug visualization
function OnDrawGizmos () 
{
   Gizmos.color = color;
   Gizmos.DrawSphere(transform.position, radius);
   
  if (iconName !="") 
   Gizmos.DrawIcon (Vector3(transform.position.x, transform.position.y+radius*1.5, transform.position.z), iconName, true);
 
}

//----------------------------------------------------------------------------------

