﻿using UnityEngine;
using UnityEditor;
using System.Collections;

namespace AC
{

	[CustomEditor (typeof (NavigationMesh))]
	public class NavigationMeshEditor : Editor
	{
		
		public override void OnInspectorGUI ()
		{
			NavigationMesh _target = (NavigationMesh) target;

			if (KickStarter.navigationManager)
			{
				KickStarter.navigationManager.ResetEngine ();
				if (KickStarter.navigationManager.navigationEngine != null)
				{
					EditorGUILayout.BeginVertical ("Button");
					_target = KickStarter.navigationManager.navigationEngine.NavigationMeshGUI (_target);
					EditorGUILayout.EndVertical ();
				}
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
	}

}