using UnityEngine;
using System.Collections;
//using StateMaschineScr
    
public class character : StateMaschineScr {
	bool ready=false;
	float myTime=0f;
	//-------------------------------------------------------------------
	public static GameObject[] characters;//global variable stores all characters
	[HideInInspector]
	public static int chrs=0;
	TextMesh txt;
	//-------------------------------------------------------------------
	void Start(){character.chrs++;StartCoroutine(go());}
	IEnumerator go(){
		yield return new WaitForSeconds(1);
		if(character.characters==null){// if array is null
			character.characters=new GameObject[chrs];
		}
		character.characters[chrs-1]=gameObject;
		chrs--;
		txt=transform.Find("txt").gameObject.GetComponent<TextMesh>();
		base.init();
		ready=true;
	}
	public void shoot(){
		int sca=(int)transform.localScale.x;
		float dist=10f;//
		float preDist=dist;
		GameObject enemy=null;
		foreach(GameObject obj in character.characters){
			if(obj!=gameObject){
				dist=transform.position.x-obj.transform.position.x;// distance between this and target and direction (negative-to Right positive -to Left)
				if(dist*sca<0){//check direction
					if(Mathf.Abs(dist)<Mathf.Abs(preDist)){//check enemy is near
						character scr=obj.GetComponent<character>();//get enemy script
						if(scr.poseHeight!=0){
							preDist=dist;
							enemy=obj;
						}
					}
				}
			}
			
		}
		if(enemy){
			character scr=enemy.GetComponent<character>();//get enemy script
			if(poseHeight==1){// enemy stay
				if(scr.poseHeight==1){
					if(aInfo.IsName("doubleShoot")){
						scr.hurt(damage*2,1f,sca*-1);
					}else{
						scr.hurt(damage,1.5f,sca*-1);
					}
				}else if(scr.poseHeight==2){
					scr.hurt(damage,1f,sca*-1);
				}
			}else if(poseHeight==2){// enemy sit
				if(scr.poseHeight==2){
					scr.hurt(damage,1f,sca*-1);
				}
			}//
		}
		
	}
	void Update(){
		if(ready){
			if(myTime<Time.time){
				myTime=Time.time+Random.Range(0.2f,1f);
				float nPos=Random.Range(0f,1f);
				if(nPos<0.1f){
					newPose="sit";
				}else if(nPos<0.2f){
					newPose="idle";
				}else if(nPos<0.3f){	
					newPose="shoot";
				}else if(nPos<0.4f){
					newPose="doubleShoot";
				}else if(nPos<0.5f){	
					newPose="roll";
				}else if(nPos<0.6f){	
					newPose="jump";
				}else if(nPos<0.7f){
					transform.localScale=new Vector3(transform.localScale.x*-1f,1f,1f);
				}else if(nPos<0.8f){	
					newPose="run";
				}else if(nPos<0.9f){
					newPose="walk";
				}else {	
					newPose="sitShoot";
				}	
			}
			if(transform.position.x<-3f){
				transform.localScale=new Vector3(1f,1f,1f);
			}else if(transform.position.x>3f){
				transform.localScale=new Vector3(-1f,1f,1f);
			}
			updateAnimator();
			txt.text=(string)"Health: "+health+"\n"+"Ammo: "+ammo+"/"+maxAmmo;
		}
	}
	public void hurt(int dam,float damageHeight, int dir){
		if(poseHeight!=0){// not fall
			transform.localScale=new Vector3(dir,transform.localScale.y,transform.localScale.z);
			health-=dam;
			if(poseHeight==3){// if jump
					newPose="fall";
					speedX=runSpeed;
					speedY=0;
			}
			if(health<=0){// if died --------------------------
				if(poseHeight==1){// if idle
					newPose="fall";
				}else if(poseHeight==2){// if sit
					newPose="sitFall";
				}
				speedX=runSpeed;
			}else{	// hurt --------------------------------
				if(poseHeight==1){// if idle
					if(damageHeight>1.3f){// head shoot
						newPose="hurtHead";
					}else{// body shoot
						newPose="hurt";
					}
				}else if(poseHeight==2){// if sit
					if(damageHeight<=1.3f){// head shoot
						newPose="sitHurt";
					}
				}
			}
		}
	}
}