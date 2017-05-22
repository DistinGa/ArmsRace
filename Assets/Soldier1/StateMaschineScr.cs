using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
    
public class StateMaschineScr : MonoBehaviour {
	public int maxAmmo, maxHealth;
	public int damage;
	public int shootCount=1;
	public bool doubleShoot;
	public float walkSpeed,runSpeed,jumpSpeed;
	[HideInInspector]
	public int poseHeight=0;//0-fall,1-stay,2-sit,3-jump
	protected int ammo, health;
	protected string pose="idle";
	protected string newPose="idle";
	protected float floor, speedX,speedY;
	protected Animator anim;
	protected AnimatorStateInfo aInfo;
	//-------------------------------------------------------------------
	protected void init(){
		health=maxHealth;
		anim=GetComponent<Animator>();
		poseHeight=0;
		floor=transform.position.y;
	}
	protected void updateAnimator(){//----------------------------------------------------------------------- state machine -----------------------------------------------------------------------------------------------
		aInfo=anim.GetCurrentAnimatorStateInfo(0);
		int scaleX=(int)transform.localScale.x;
		Vector3 pos=transform.position;
		float  DeltaTime=Time.deltaTime;
		if(aInfo.IsName("idle")){//--------------------------------------
			if(!hurt(aInfo)){ 
				if(ammo<=0){to("reload");}// to reload
				to(newPose);
				poseHeight=1;
			}
		}else if(aInfo.IsName("shoot")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");ammo-=shootCount;}//back to idle
				
				poseHeight=1;
			}
		}else if(aInfo.IsName("doubleShoot")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");ammo-=shootCount*2;}//back to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("hurt")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");}//back to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("hurtHead")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");}//back to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("sitDown")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("sit");}//continue to sit
				poseHeight=1;
			}
		}else if(aInfo.IsName("sit")){
			if(!hurt(aInfo)){ 
				if(ammo<=0){to("sitReload");}// to reload
				to(newPose);
				poseHeight=2;
			}
		}else if(aInfo.IsName("sitUp")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");}//continue to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("sitReload")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("sit");ammo=maxAmmo; }//back to sit
				poseHeight=2;
			}
		}else if(aInfo.IsName("reload")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");ammo=maxAmmo;}//back to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("jump")){// ----- jump ---------- jump ---------- jump ---------- jump ---------- jump ---------- jump ---------- jump ---------- jump ---------- jump ---------- jump -----
			pos.x+=speedX*DeltaTime;
			if(speedY==0f){
				pos.y=floor+0.2f;
				speedY=jumpSpeed;
			}else{
				if(pos.y>floor){
					speedY-=5.0f*DeltaTime;//decay
					pos.y+=speedY*DeltaTime;
				}else{
					pos.y=floor;
					transform.position=pos;
					to("land");//proceed to land
				}
			}
			if(newPose=="fall"){
				to("fall");
			}else if(newPose=="hurtHead"){
				if(transform.position.y>1.1 && transform.position.y<1.5){
					poseHeight=3;
					to("fall");
				}else{
					to("idle");//back to idle
				}
			}else if(newPose=="hurt"){
				if(transform.position.y>0.3 && transform.position.y<1.1){
					poseHeight=3;
					to("fall");
				}else{
					to("idle");//back to idle
				}
			}else{
				transform.position=pos;
				poseHeight=3;
			}
		}else if(aInfo.IsName("land")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");}//proceed to idle
				poseHeight=1;
			}
		}else if(aInfo.IsName("roll")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("idle");}//back to idle
				poseHeight=2;
			}
		}else if(aInfo.IsName("run")){
			if(!hurt(aInfo)){ 
				pos.x+=scaleX*runSpeed*DeltaTime;
				transform.position=pos;
				to(newPose);
				poseHeight=1;
			}
		}else if(aInfo.IsName("walk")){
			if(!hurt(aInfo)){ 
				pos.x+=scaleX*walkSpeed*DeltaTime;
				transform.position=pos;
				to(newPose);
				poseHeight=1;
			}
		}else if(aInfo.IsName("fall")){// ---------------------------fall with fall -------------------------------
			if(pos.y>floor){
				pos.y-=speedY;
				speedY*=1.25f*DeltaTime;// decay
			}else{
				speedY=0f;
				pos.y=floor;
			}
			
			pos.x-=scaleX*speedX*DeltaTime;// decay
			speedX*=0.125f*DeltaTime;
			transform.position=pos;
			if(aInfo.normalizedTime>=0.95f){to("standUp");health=maxHealth;}//  god mode --- !
			poseHeight=3;
		}else if(aInfo.IsName("sitFall")){
			if(aInfo.normalizedTime>=0.95f){to("standUp");}
			poseHeight=0;
		}else if(aInfo.IsName("standUp")){
			if(aInfo.normalizedTime>=0.95f){to("idle");}//continue to idle
			health=maxHealth;
			poseHeight=2;
		}else if(aInfo.IsName("sitHurt")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("sit");}//back to sit
				poseHeight=2;
			}
		}else if(aInfo.IsName("sitShoot")){
			if(!hurt(aInfo)){ 
				if(aInfo.normalizedTime>=0.95f){to("sit");ammo-=shootCount;}//back to sit
				poseHeight=2;
				
			}
		}else{
			print("unknown state");
		}
	}
	void to(string st){
		if(!doubleShoot && st=="doubleShoot"){
			st="shoot";
		}
		anim.Play(st);
		newPose=st;
		}
	bool hurt(AnimatorStateInfo ai){
		bool retVal=false;
		if(poseHeight==1){//stay
			if(!aInfo.IsName("fall")){
				if(newPose=="fall"){poseHeight=0;anim.Play("fall");retVal=true;}
				else if(newPose=="hurt"){poseHeight=1;anim.Play("hurt");retVal=true;}
				else if(newPose=="hurtHead"){poseHeight=1;anim.Play("hurtHead");retVal=true;}
				else{retVal=false;}
			}
		}else if(poseHeight==2){//sit
			if(!aInfo.IsName("sitFall")){
				if(newPose=="fall"){poseHeight=0;anim.Play("sitFall");retVal=true;}
				else if(newPose=="hurt"){poseHeight=1;anim.Play("sitHurt");retVal=true;}
				else{retVal=false;}
			}
		}
		return retVal;
	}
}

