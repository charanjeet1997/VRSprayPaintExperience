using System.IO;
using UnityEditor;

namespace Games.Platformer3D.Utilities
{
	using UnityEngine;
	using System;
	using System.Collections;


	public static class Setup
	{
		public static void CreateDefaultFolders()
		{
			
		}

		static class Folders
		{
			public static void CreateDefault(string root, params string[] folders)
			{
				var fullPath = Path.Combine(Application.dataPath, root);
				foreach (var folder in folders)
				{
					var folderPath = Path.Combine(fullPath, folder);
					if (!Directory.Exists(folderPath))
					{
						Directory.CreateDirectory(folderPath);
					}
				}
			}
		}

		public static class Assets
		{
			public static void ImportAssets(string asset, string subFolder, string folder = "C:/Users/chara/AppData/Roaming/Unity/Asset Store-5.x")
			{
				AssetDatabase.ImportPackage(Combine(folder,subFolder,asset), false);
			}

			private static string Combine(string folder, string subFolder, string asset)
			{
				return Path.Combine(folder, subFolder, asset);
			}
		}
	}
}