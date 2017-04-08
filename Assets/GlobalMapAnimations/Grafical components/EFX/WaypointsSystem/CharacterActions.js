//----------------------------------------------------------------------------------------------
// Simple example script to handle character animation
// Please update it or create new according to requirements of your project
// You also can now randomize the animation in few ways:
//  1) Simple by direct calling functions, RandomizeIdle(), RandomizeMove(), etc
//  2) By adding calling those functions form the waypoint: 
//  3) By using special script for trigger - ChangeAnimation_Trigger. Just attach it to any object with collider and  chose which animation you want to randomize when actor enters the  trigger-zone.

//----------------------------------------------------------------------------------------------


#pragma strict


var waypointMover: WaypointMover;

var idleAnimation: AnimationClip;
var idleVariations: AnimationClip[];

var moveAnimation: AnimationClip;
var moveVariations: AnimationClip[];

var jumpAnimation: AnimationClip;
var jumpVariations: AnimationClip[];

var jumpForce: Vector3;


private var inSpecialAction: boolean = false;
private var animationComponent: Animation;


//----------------------------------------------------------------------------------
function Start () 
{
  animationComponent = GetComponent.<Animation>();

  if (!idleAnimation  &&  idleVariations.Length > 0)  idleAnimation = idleVariations[0];
  if (!moveAnimation  &&  moveVariations.Length > 0)  moveAnimation = moveVariations[0];
  if (!jumpAnimation  &&  jumpVariations.Length > 0)  jumpAnimation = jumpVariations[0];
}

//----------------------------------------------------------------------------------
function Update () 
{
  if(!inSpecialAction)
     if (waypointMover.isMoving()) Move(); else Idle();
   else
     if (!animationComponent.isPlaying) inSpecialAction = false;
}

//----------------------------------------------------------------------------------
function Idle () 
{
 if (!animationComponent.isPlaying || animationComponent.IsPlaying(moveAnimation.name)) animationComponent.Play(idleAnimation.name);
}



function Move () 
{
 if (animationComponent.isPlaying)  animationComponent.Play(moveAnimation.name);
}



function Jump() 
{
 inSpecialAction = true;
 animationComponent.Play(jumpAnimation.name);
 GetComponent.<Rigidbody>().AddRelativeForce(jumpForce, ForceMode.Impulse);
}

//----------------------------------------------------------------------------------
function RandomizeAnimation(variations: AnimationClip[]): AnimationClip
{
  return variations[Random.Range(0, variations.Length)];

}

//----------------------------------------------------------------------------------
function RandomizeIdle() 
{
   if (idleVariations.Length > 0)  idleAnimation = RandomizeAnimation (idleVariations);

}

//----------------------------------------------------------------------------------
function RandomizeMove() 
{
  if (moveVariations.Length > 0)  moveAnimation = RandomizeAnimation (moveVariations);

}

//----------------------------------------------------------------------------------
function RandomizeJump() 
{
  if (jumpVariations.Length > 0)  jumpAnimation = RandomizeAnimation (jumpVariations);

}

//----------------------------------------------------------------------------------
