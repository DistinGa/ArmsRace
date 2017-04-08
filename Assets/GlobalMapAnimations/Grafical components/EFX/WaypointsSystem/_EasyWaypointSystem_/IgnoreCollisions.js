#pragma strict

var ignoreCollisionWith: Collider[];

function Start () 
{
 if(ignoreCollisionWith.Length >0 && GetComponent.<Collider>())
  for( var i = 0; i< ignoreCollisionWith.Length; i++)
     Physics.IgnoreCollision(GetComponent.<Collider>(), ignoreCollisionWith[i]);

}

