﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle : MonoBehaviour {

	public float radius = 10f;
	public float itemScale = 0.3f;
	//manual rotation
	public float rotateSensitivity = 100f;
	//auto rotation
	public float rotateSpeed = 80f;
	//check if user is manually dragging
	private bool isAutoRotating = true;

	private VariantAdapter adapter;
	private GameObject[] variantModels;
	//array to hold set of clones of models
	private GameObject[] clones;
	//Vector to set scaling of models
	private Vector3 scaleVector;
	//Material prop block to store individual material color
	private MaterialPropertyBlock propBlock;
	//3D text to display variant name
	private GameObject variantName;

	// Use this for initialization
	void Start () {
		//get adapter
		adapter = transform.parent.gameObject.GetComponent<VariantAdapter>();
		//get models
		variantModels = adapter.variantModels;
		//initialize prop block
		propBlock = new MaterialPropertyBlock();
		//scaleVector to scale the models
		scaleVector = new Vector3(itemScale, itemScale, itemScale);
		//initialize clones array
		clones = new GameObject[variantModels.Length];
		//get 3D text object
		variantName = transform.GetChild(0).gameObject;

		//position of item
		Vector3 pos;
		//create small-scaled food model items into a circle
		for (int i = 0; i < variantModels.Length; i++) {
	        float angle = i * Mathf.PI * 2 / variantModels.Length;
	        pos = transform.position;
	        pos += new Vector3(Mathf.Cos(angle), transform.position.y, Mathf.Sin(angle)) * radius;
	        clones[i] = Instantiate(variantModels[i], pos, Quaternion.identity, transform);
	        clones[i].transform.localScale = scaleVector;

	        //check if variant is selected => if no then make it partially visible
	        if (i != adapter.selected) {
	        	SetVisibility(clones[i], 0.5f);
	        }

	    	//add a collider to each item to enable on child click
	    	AddItemCollider(clones[i]);

	    	//add mouse listener to child
	    	ItemMouseEvent mouseEvent = clones[i].AddComponent<ItemMouseEvent>();
	    	mouseEvent.parent = this;
	    	mouseEvent.position = i;
	    	mouseEvent.rotateSensitivity = rotateSensitivity;
    	}

    	//show variant name
    	ChangeVariantName(adapter.GetSelectedVarName());
	}

	//make the circle auto rotate
	void Update () {
		if (isAutoRotating) {
			transform.Rotate(Vector3.up * (rotateSpeed / 10) * Time.deltaTime);
		}
	}

	void ChangeVariantName (string name) {
    	TextMesh textMesh = variantName.GetComponent<TextMesh>();
    	textMesh.text = adapter.GetSelectedVarName();
	}

	void SetVisibility (GameObject model, float transparentValue) {	
		//use this to handle individual material color
		MeshRenderer rend;

		rend = model.GetComponent<MeshRenderer>();
		if (rend != null) {
			rend.GetPropertyBlock(propBlock);
			propBlock.SetColor("_Color", new Color(1, 1, 1, transparentValue));
			rend.SetPropertyBlock(propBlock);
		}

		foreach (MeshRenderer childRend in model.GetComponentsInChildren<MeshRenderer>()) {
			childRend.GetPropertyBlock(propBlock);
			propBlock.SetColor("_Color", new Color(1, 1, 1, transparentValue));
			childRend.SetPropertyBlock(propBlock);
		}
	}

	void AddItemCollider (GameObject item) {
		FitBoxCollider itemFit = item.AddComponent<FitBoxCollider>();
		itemFit.GetFit(itemScale * 2f);
	}

	void OnItemClick (int position) {
		//fade selected item
		SetVisibility(clones[adapter.selected], 0.5f);
		//unfade newly selected item
		SetVisibility(clones[position], 1f);
		//notify adapter about the change
		adapter.SelectVariant(position);
		//update variant name
		ChangeVariantName(adapter.GetSelectedVarName());
	}

	void SetAutoRotation (bool isActive) {
		isAutoRotating = isActive;
	}

	void RotateCircle (Vector3 rotVect) {
		transform.Rotate(rotVect);
	}

	class ItemMouseEvent : MonoBehaviour {

		public SelectionCircle parent;
		public float rotateSensitivity;
		public int position;

		void OnMouseUpAsButton () {
			parent.OnItemClick(position);
		}

		//disable auto rotation
		void OnMouseDown () {
			parent.SetAutoRotation(false);
		}

		//enable auto rotation
		void OnMouseUp () {
			parent.SetAutoRotation(true);
		}

		//delegate mouse dragging detection to children
		void OnMouseDrag () {
			float h = Input.GetAxis("Mouse X") * rotateSensitivity * Mathf.Deg2Rad;
  			parent.RotateCircle(Vector3.up * -h);
		}
	}
}
