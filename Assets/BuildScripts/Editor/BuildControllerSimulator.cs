using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class BuildControllerSimulator : MonoBehaviour {

	[MenuItem("Tools/Gemserk/VR/Build Controller Simulator")]
	public static void Build ()
	{
		
		string[] levels = new string[] {"Assets/Scenes/SimulatorCaptureScene.unity"};

		// Build player.
		BuildPipeline.BuildPlayer(levels, "Build/ControllerSimulator.apk", BuildTarget.Android, BuildOptions.None);
		
		BuildPlayerOptions bpo = new BuildPlayerOptions();
	}
}
