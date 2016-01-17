﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Camera))]

public class Comic : MonoBehaviour {

	public Shader Normals;
	public Shader Colored_objects;
  public Shader Outline;
  public float intensity;
  private Material material;

  // parameters
  public float sensitivityDepth = 1.0f;
  public float sensitivityNormals = 1.0f;
  public float sampleDist = 1.0f;
  public float edgesOnly = 0.0f;
  public Color edgesOnlyBgColor = Color.white;

	void Start () {
	 	GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();	 	
		foreach(GameObject myObj in allObjects) {		  
			if (myObj.GetComponent<Renderer>()) {		 
				Material[] allMaterials = myObj.GetComponent<Renderer>().sharedMaterials;
        if(myObj.tag!="Color") {
				  foreach(Material mat in allMaterials)
            if (mat.name != "gioia")
					  mat.shader = Normals; // a material executes all the passes in the shader
		    }
        else if (myObj.tag=="Color" ) {
				  foreach(Material mat in allMaterials)
					  mat.shader = Colored_objects;    
        }
      }
    }
    material = new Material(Outline);
    //GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
  }
  // Postprocess the image
  void OnRenderImage (RenderTexture source, RenderTexture destination) {
    material.SetFloat("_bwBlend", intensity);

    // shader uniforms
    Vector2 sensitivity = new Vector2 (sensitivityDepth, sensitivityNormals);
    material.SetVector ("_Sensitivity", new Vector4 (sensitivity.x, sensitivity.y, 1.0f, sensitivity.y));
    material.SetFloat ("_BgFade", edgesOnly);
    material.SetFloat ("_SampleDistance", sampleDist);
    material.SetVector ("_BgColor", edgesOnlyBgColor);

    Graphics.Blit (source, destination, material,0);
  }

}
