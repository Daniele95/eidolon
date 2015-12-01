/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Serializer.cs"
 * 
 *	This script serializes saved game data and performs the file handling.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System;
#if !(UNITY_WP8 || UNITY_WINRT || UNITY_WII)
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.IO;
using System.Xml; 
using System.Xml.Serialization;

namespace AC
{

	/**
	 * All of AC's actual file handling, serialising and deserialising is performed within this script.
	 * It's public functions are static, so it does not need to be placed on any scene object.
	 */
	public class Serializer : MonoBehaviour
	{

		/**
		 * <summary>Gets a component on a GameObject in the scene that also has a ConstantID component on it.</summary>
		 * <param name = "constantID">The Constant ID number generated by the ConstantID component</param>
		 * <returns>The component on the GameObject with a Constant ID number that matches the request. If none is found, returns null.</returns>
		 */
		public static T returnComponent <T> (int constantID) where T : Component
		{
			if (constantID != 0)
			{
				T[] objects = FindObjectsOfType (typeof(T)) as T[];
				
				foreach (T _object in objects)
				{
					if (_object.GetComponent <ConstantID>())
					{
						ConstantID[] idScripts = _object.GetComponents <ConstantID>();
						foreach (ConstantID idScript in idScripts)
						{
							if (idScript.constantID == constantID)
							{
								// Found it
								return _object;
							}
						}
					}
				}
			}
			
			return null;
		}


		/**
		 * <summary>Gets the Constant ID number of a GameObject, if it has one.</summary>
		 * <param name = "_gameObject">The GameObject to check for</param>
		 * <returns>The Constant ID number (generated by a ConstantID script), if it has one.  Returns 0 otherwise.</returns>
		 */
		public static int GetConstantID (GameObject _gameObject)
		{
			if (_gameObject.GetComponent <ConstantID>())
			{
				if (_gameObject.GetComponent <ConstantID>().constantID != 0)
				{
					return (_gameObject.GetComponent <ConstantID>().constantID);
				}
				else
				{	
					ACDebug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a Constant ID number.");
				}
			}
			else
			{
				ACDebug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a 'Constant ID' script - please exit Play mode and attach one to it.");
			}
			
			return 0;
		}
		

		/**
		 * <summary>Serializes an object into a binary string.</summary>
		 * <param name = "pObject">The object to serialize</param>
		 * <returns>The object, serialized into a binary string</returns>
		 */
		public static string SerializeObjectBinary (object pObject)
		{
			#if UNITY_WP8 || UNITY_WINRT
			return "";
			#else
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream ();
			binaryFormatter.Serialize (memoryStream, pObject);
			return (Convert.ToBase64String (memoryStream.GetBuffer ()));
			#endif
		}
		

		/**
		 * <summary>De-serializes an object from a binary string.</summary>
		 * <param name = "pString">The object, serialized into a binary string</param>
		 * <returns>The object, deserialized</returns>
		 */
		public static T DeserializeObjectBinary <T> (string pString)
		{
			if (pString == null || pString.Length == 0) return default (T);
			#if UNITY_WP8 || UNITY_WINRT
			return default (T);
			#else
			if (pString.Contains ("<?xml"))
			{
				// Fix converted Options Data
				//PlayerPrefs.DeleteKey ("Options");
				return default (T);
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			return (T) binaryFormatter.Deserialize (memoryStream);
			#endif
		}


		/**
		 * <summary>Serializes an object into an XML string.</summary>
		 * <param name = "pObject">The object to serialize</param>
		 * <returns>The object, serialized into an XML string</returns>
		 */
		public static string SerializeObjectXML <T> (object pObject) 
		{ 
			string XmlizedString = null; 
			
			MemoryStream memoryStream = new MemoryStream(); 
			XmlSerializer xs = new XmlSerializer (typeof (T)); 
			XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8); 
			
			xs.Serialize (xmlTextWriter, pObject); 
			memoryStream = (MemoryStream) xmlTextWriter.BaseStream; 
			XmlizedString = UTF8ByteArrayToString (memoryStream.ToArray());
			
			return XmlizedString;
		}
		

		/**
		 * <summary>De-serializes an object from an XML string.</summary>
		 * <param name = "pXmlizedString The object, serialized into an XML string</param>
		 * <returns>The object, deserialized</returns>
		 */
		public static object DeserializeObjectXML <T> (string pXmlizedString) 
		{ 
			XmlSerializer xs = new XmlSerializer (typeof (T)); 
			MemoryStream memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)); 
			return xs.Deserialize(memoryStream); 
		}
		
		
		private static string UTF8ByteArrayToString (byte[] characters) 
		{		
			UTF8Encoding encoding = new UTF8Encoding(); 
			string constructedString = encoding.GetString (characters, 0, characters.Length);
			return (constructedString); 
		}
		
		
		private static byte[] StringToUTF8ByteArray (string pXmlString) 
		{ 
			UTF8Encoding encoding = new UTF8Encoding(); 
			byte[] byteArray = encoding.GetBytes (pXmlString); 
			return byteArray; 
		}


		/**
		 * <summary>Deserializes a RememberData class from a binary string.</summary>
		 * <param name = "pString">The RememberData, serialized as a binary string</param>
		 * <returns>The RememberData class, deserialized</returns>
		 */
		public static T DeserializeRememberData <T> (string pString) where T : RememberData
		{
			#if !(UNITY_WP8 || UNITY_WINRT || UNITY_WII)
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			T myObject;//
			myObject = binaryFormatter.Deserialize (memoryStream) as T;//
			return myObject;//
			//return binaryFormatter.Deserialize (memoryStream) as T;
			#else
			return null;
			#endif
		}


		/**
		 * <summary>Deserializes a List of SingleLevelData from a binary string.</summary>
		 * <param name = "pString">The List of SingleLevelData, serialized as a binary string</param>
		 * <returns>The List of SingleLevelData, deserialized</returns>
		 */
		public static List<SingleLevelData> DeserializeRoom (string pString)
		{
			#if !(UNITY_WP8 || UNITY_WINRT || UNITY_WII)
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
			return (List<SingleLevelData>) binaryFormatter.Deserialize (memoryStream);
			#else
			return null;
			#endif
		}


		/**
		 * <summary>Deletes a save game file by name.</summary>
		 * <param name = "fullFileName">The full filename of the file to delete.  If this is run on the WebPlayer platform, this is the name of the PlayerPrefs key to delete.</param>
		 */
		public static void DeleteSaveFile (string fullFileName)
		{
			#if UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII
			
			if (PlayerPrefs.HasKey (fullFileName))
			{
				PlayerPrefs.DeleteKey (fullFileName);
				ACDebug.Log ("PlayerPrefs key deleted: " + fullFileName);
			}

			#else

			FileInfo t = new FileInfo (fullFileName);
			if (t.Exists)
			{
				t.Delete ();
				ACDebug.Log ("File deleted: " + fullFileName);
			}

			#endif
		}


		/**
		 * <summary>Creates a save game file by name.</summary>
		 * <param name = "fullFileName">The full filename of the file to create.  If this is run on the WebPlayer platform, this is the name of the PlayerPrefs key to create.</param>
		 * <param name = "_data">The serialized data to save within the file</param>
		 */
		public static void CreateSaveFile (string fullFileName, string _data)
		{
			#if UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII
			
			PlayerPrefs.SetString (fullFileName, _data);
			ACDebug.Log ("PlayerPrefs key written: " + fullFileName);
			
			#else
			
			StreamWriter writer;
			FileInfo t = new FileInfo (fullFileName);
			
			if (!t.Exists)
			{
				writer = t.CreateText ();
			}
			
			else
			{
				t.Delete ();
				writer = t.CreateText ();
			}
			
			writer.Write (_data);
			writer.Close ();

			ACDebug.Log ("File written: " + fullFileName);
			#endif
		}
		

		/**
		 * <summary>Loads a save game file by name.</summary>
		 * <param name = "fullFileName">The full filename of the file to load.  If this is run on the WebPlayer platform, this is the name of the PlayerPrefs key to load.</param>
		 * <param name = "doLog">If True, then a log of the action will be written in the Console window</param>
		 * <returns>The contents of the save file</returns>
		 */
		public static string LoadSaveFile (string fullFileName, bool doLog)
		{
			string _data = "";
			
			#if UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII
			
			_data = PlayerPrefs.GetString (fullFileName, "");
			
			#else

			if (File.Exists (fullFileName))
			{
				StreamReader r = File.OpenText (fullFileName);
				
				string _info = r.ReadToEnd ();
				r.Close ();
				_data = _info;
			}
			
			#endif

			if (doLog && _data != "")
			{
				ACDebug.Log ("File Read: " + fullFileName);
			}
			return (_data);
		}
		

		/**
		 * <summary>Converts a compressed string into an array of Paths object's nodes.</summary>
		 * <param name = "path">The Paths object to send the results to</param>
		 * <param name = "pathData">The compressed string</param>
		 * <returns>The Paths object, with the recreated nodes</returns>
		 */
		public static Paths RestorePathData (Paths path, string pathData)
		{
			if (pathData == null)
			{
				return null;
			}
			
			path.affectY = true;
			path.pathType = AC_PathType.ForwardOnly;
			path.nodePause = 0;
			path.nodes = new List<Vector3>();
			
			if (pathData.Length > 0)
			{
				string[] nodesArray = pathData.Split ("|"[0]);
				
				foreach (string chunk in nodesArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					float _x = 0;
					float.TryParse (chunkData[0], out _x);
					
					float _y = 0;
					float.TryParse (chunkData[1], out _y);
					
					float _z = 0;
					float.TryParse (chunkData[2], out _z);
					
					path.nodes.Add (new Vector3 (_x, _y, _z));
				}
			}
			
			return path;
		}
		

		/**
		 * <summary>Compresses a Paths object's nodes into a single string, that can be stored in a save file.</summary>
		 * <param name = "path">The Paths object to compress</param>
		 * <returns>The compressed string</returns>
		 */
		public static string CreatePathData (Paths path)
		{
			System.Text.StringBuilder pathString = new System.Text.StringBuilder ();
			
			foreach (Vector3 node in path.nodes)
			{
				pathString.Append (node.x.ToString ());
				pathString.Append (":");
				pathString.Append (node.y.ToString ());
				pathString.Append (":");
				pathString.Append (node.z.ToString ());
				pathString.Append ("|");
			}
			
			if (path.nodes.Count > 0)
			{
				pathString.Remove (pathString.Length-1, 1);
			}
			
			return pathString.ToString ();
		}


		/**
		 * <summary>Saves a save game's screenshot texture to disk.</summary>
		 * <param name = "screenshotTex">The screenshot texture to save</param>
		 * <param name = "fileName">The full filename and filepath to save the screenshot to</param>
		 */
		public static void SaveScreenshot (Texture2D screenshotTex, string fileName)
		{
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT && !UNITY_WII
			byte[] bytes = screenshotTex.EncodeToJPG ();
			File.WriteAllBytes (fileName, bytes);
			#endif
		}


		/**
		 * <summary>Deletes a save game's screenshot texture.</summary>
		 * <param name = "fileName">The full filename and filepath of the screenshot</param>
		 */
		public static void DeleteScreenshot (string fileName)
		{
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT && !UNITY_WII
			if (File.Exists (fileName))
			{
				File.Delete (fileName);
			}
			#endif
		}


		/**
		 * <summary>Loads a save game's screenshot texture.</summary>
		 * <param name = "fileName">The full filename and filepath of the screenshot</param>
		 */
		public static Texture2D LoadScreenshot (string fileName)
		{
			#if !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_WINRT && !UNITY_WII
			if (File.Exists (fileName))
			{
				byte[] bytes = File.ReadAllBytes (fileName);
				Texture2D screenshotTex = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
				screenshotTex.LoadImage (bytes);
				return screenshotTex;
			}
			#endif
			return null;
		}


		/**
		 * <summary>Serializes an object, either XML or Binary, depending on the platform.</summary>
		 * <param name = "pObject">The object to serialize</param>
		 * <returns>The object, serialized to a string</returns>
		 */
		public static string SaveScriptData <T> (object pObject)
		{
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				return Serializer.SerializeObjectXML <T> (pObject);
			}
			else
			{
				return Serializer.SerializeObjectBinary (pObject);
			}
		}


		/**
		 * <summary>De-serializes a RememberData class, either from XML or Binary, depending on the platform.</summary>
		 * <param name = "stringData">The RememberData class, serialized as a string</param>
		 * <returns>The de-serialized RememberData class</return>
		 */
		public static T LoadScriptData <T> (string stringData) where T : RememberData
		{
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				// iOS fix: Sending to wrong type causes error, so pre-empt by checking type
				string typeName = typeof (T).ToString ().Replace ("AC.", "");
				
				if (stringData.Contains (typeName))
				{
					return (T) Serializer.DeserializeObjectXML <T> (stringData);
				}
			}
			else
			{
				return (T) Serializer.DeserializeRememberData <T> (stringData);
			}
			
			return null;
		}

	}

}