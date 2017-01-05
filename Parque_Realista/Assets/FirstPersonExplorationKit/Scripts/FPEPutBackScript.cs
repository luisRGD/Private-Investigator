using UnityEngine;
using System.Collections;

//
// FPEPutBackScript
// This script is for put back objects. In its simplist form, it's a 
// trigger collider. If the collider is not set to be a trigger, this
// will be toggled on Awake. The physics layer is also set to
// be FPEPutBackObjects.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//

[RequireComponent (typeof (Collider))]
public class FPEPutBackScript : MonoBehaviour {

	[Tooltip("If set in the inspector, this put back position will be tied to the assigned object. This allows for drag and drop assignment in the Scene editor.")]
	public GameObject myPickupObject = null;

	[Tooltip("The maximum straight-line distance from the player that an object can be put back. Strongly recommended that this be given the same value as the Pickup Object's interaction distance if manually creating Put Back object.")]
	public float interactionDistance = 2.0f;

	private int pickupObjectID = 0;

	void Awake(){

		if(!gameObject.GetComponent<Collider>().isTrigger){
			gameObject.GetComponent<Collider>().isTrigger = true;
		}

		gameObject.layer = LayerMask.NameToLayer("FPEPutBackObjects");

		if(myPickupObject != null) {
			pickupObjectID = myPickupObject.GetInstanceID();
		}

	}

	public int getPickupObjectID(){
		return pickupObjectID;
	}

	public void setPickupObjectID(int id){
		pickupObjectID = id;
	}

	public void setInteractionDistance(float distance){
		interactionDistance = distance;
	}

	public float getInteractionDistance(){
		return interactionDistance;
	}

}
