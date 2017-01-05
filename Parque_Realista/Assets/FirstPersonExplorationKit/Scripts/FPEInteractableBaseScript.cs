using UnityEngine;
using System.Collections;

//
// FPEInteractableBaseScript
// This script is the base or parent of all Interactable object types. The core
// functionality of Interactable objects happens here. For example, object
// highlighting, interaction strings, etc. All other Interactable classes
// should be based on this base class or a child of this class.
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
[RequireComponent(typeof(Collider))]
public abstract class FPEInteractableBaseScript: MonoBehaviour {

	// Note: You can easily add your own behaviour type by extending this enum. Be sure
	// to also add a case in FPEInteractionManagerScript to handle the new enum value
	public enum eInteractionType {STATIC, PICKUP, ACTIVATE, JOURNAL, AUDIODIARY, INVENTORY};
	protected eInteractionType interactionType = eInteractionType.STATIC;

	// If false, player cannot interact with this object when holding something
	protected bool canInteractWithWhileHoldingObject = true;

	// Some or all instances of Interactable may require a reference to the Interaction Manager for playing audio, etc.
	protected GameObject interactionManager = null;

	[Header("Highlighting and Interacting")]
	[Tooltip("If unchecked, the object will not have Object Highlight Material applied when highlighted with the reticle")]
	public bool highlightOnMouseOver = true;
	[Tooltip("This is the material added to the object when it is highlighted by the reticle, if enabled. If left blank (recommended), the default is used. If you want a special material used instead, specify it here.")]
	public Material objectHighlightMaterial;
	private bool highlightMaterialSet = false;
	private bool hasBeenDiscovered = false;
	[Tooltip("The maximum straight-line distance from the player that an object can be interacted with. Default is 2.0.")]
	public float interactionDistance = 2.0f;

	[Header("Optionally play a Secondary Sound on Interact")]
	[Tooltip("Check this box to play a secondary sound when this object is interacted with. For example, the narration of a journal page.")]
	public bool playSecondarySoundOnInteract = false;
	protected bool hasPlayedOnce = false;
	[Tooltip("If checked, the secondary sound will behave the same way as an audio log, including log title and skip functionality. Note that if this is false, the specified sound will not stop until completed and cannot be skipped.")]
	public bool showWithText = false;
	[Tooltip("If 'Show With Text' is checked, this text will appear as the audio log title.")]
	public string audioLogText = "<DEFAULT PICKUP SOUND AUDIO LOG TEXT>";
	public enum eInteractSoundPlaybackType { PLAY_ONCE, PLAY_EVERY_TIME };
	[Tooltip("PLAY_ONCE means the secondary sound will only play on the first interaction. PLAY_EVER_TIME means it will play on every interaction.")]
	public eInteractSoundPlaybackType soundPlaybackBehaviour = eInteractSoundPlaybackType.PLAY_ONCE;
	[Tooltip("The AudioClip for the secondary sound. If this is not specified, no sound will be played, regardless of other values set in this section.")]
	public AudioClip soundToPlayOnInteract = null;

	[Header("Interaction Stings")]
	[Tooltip("The string that appears below the reticle when the object is highlighted")]
	public string interactionString = "<DEFAULT INTERACTION STRING>";

	public virtual void Awake(){

		if(!objectHighlightMaterial){
			objectHighlightMaterial = Resources.Load("ObjectHighlightMaterial") as Material;
			if(!objectHighlightMaterial){
				Debug.LogError("FPEInteractableBaseScript:: Cannot find ObjectHighlightMaterial in Resources folder!");
			}
		}

	}

	public virtual void Start(){

		interactionManager = GameObject.Find("FPEInteractionManager");
		if(!interactionManager){
			Debug.LogError ("FPEInteractableBaseScript:: Cannot find FPE Interaction Manager!");
		}

	}

	// Base version of interact does nothing. Implement special interactions in 
	// child classes as required (start animations, play audio files, spawn 
	// objects, etc.)
	public virtual void interact(){

		// If specified, we play a narration or other sound effect when the journal is activated.
		if(playSecondarySoundOnInteract && soundToPlayOnInteract){
			
			if(!hasPlayedOnce || soundPlaybackBehaviour == eInteractSoundPlaybackType.PLAY_EVERY_TIME){
				
				if(showWithText){
					
					interactionManager.GetComponent<FPEInteractionManagerScript>().playSecondaryInteractionAudio(soundToPlayOnInteract, true, audioLogText);
					hasPlayedOnce = true;
					
				}else{
					
					interactionManager.GetComponent<FPEInteractionManagerScript>().playSecondaryInteractionAudio(soundToPlayOnInteract, false, "");
					hasPlayedOnce = true;
					
				}
				
			}
			
		}else if(playSecondarySoundOnInteract && !soundToPlayOnInteract){
			Debug.LogWarning("FPEInteractableBaseScript:: Interactable '"+gameObject.transform.name+"' has 'Play Secondary Sound On Interact' checked, but there is no 'Sound To Play On Interact' Audio Clip specified.");
		}

	}

	public void highlightObject(){

		if(!hasBeenDiscovered){
			discoverObject();
		}

		setHighlightMaterial();

	}
	
	public void unHighlightObject(){
		removeHighlightMaterial();
	}

	public eInteractionType getInteractionType(){
		return interactionType;
	}

	public bool interactionsAllowedWhenHoldingObject(){
		return canInteractWithWhileHoldingObject;
	}

	public float getInteractionDistance(){
		return interactionDistance;
	}

	// Add highlight material as the last material on the object
	private void setHighlightMaterial(){

		if(objectHighlightMaterial && !highlightMaterialSet && highlightOnMouseOver){
			
			MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			foreach(MeshRenderer m in childMeshRenderers){
				
				Material[] childMaterials = m.materials;
				int materialCount = childMaterials.Length;
				Material[] newMaterials = new Material[materialCount + 1];
				
				for(int i = 0; i < materialCount; i++){
					newMaterials[i] = childMaterials[i];
				}
				
				Texture mainTexture = m.material.mainTexture;
				newMaterials[materialCount] = objectHighlightMaterial;
				
				if(mainTexture){
					objectHighlightMaterial.mainTexture = mainTexture;
				}else{
					Debug.LogWarning("Game Object '" + gameObject.name + "' had a mesh renderer called '" + m + "' with NO mainTexture set. This will result in previous highlighted object's main texture being applied to " + gameObject.name + " which will look weird");
				}
				
				m.materials = newMaterials;
				
				MeshFilter[] childMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

				foreach(MeshFilter mf in childMeshFilters){

					if(mf && mf.mesh && !gameObject.isStatic){
						mf.mesh.RecalculateBounds();
					}

				}
				
			}

			highlightMaterialSet = true;
			
		}
		
	}
	
	// Remove the last material (namely, highlight material) that we added
	private void removeHighlightMaterial(){

		if(objectHighlightMaterial && highlightMaterialSet){
			
			MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			
			foreach(MeshRenderer m in childMeshRenderers){
				
				Material[] childMaterials = m.materials;
				int childMaterialsCount = childMaterials.Length - 1;
				
				if(childMaterialsCount >= 0){
					
					Material[] resetMaterials = new Material[childMaterialsCount];
					
					for(int i = 0; i < childMaterialsCount; i++){
						resetMaterials[i] = childMaterials[i];
					}
					
					m.materials = resetMaterials;
					
					if(!gameObject.isStatic){
						gameObject.GetComponentInChildren<MeshFilter>().mesh.RecalculateBounds();
					}
					
				}
				
			}

			highlightMaterialSet = false;

		}
		
	}

	// Override this (and be sure to call base version) if your child object's discovery includes other functionality (see Audio Diary script)
	public virtual void discoverObject(){
		hasBeenDiscovered = true;
	}

}
