using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace OnlineShop.EditorTools
{
	[InitializeOnLoad]
	internal static class SceneToolbar
	{
		private const string LogonButtonText = "Logon";
		private const string MainButtonText = "Main";
		private const string LogonPlayButtonText = "Logon & Play";
		private const string LogonPlayTooltip = "Open LogonScene and enter Play Mode";
		private const string OpenSceneTooltipPrefix = "Open ";
		private const string SceneNotFoundText = "Scene not found";
		private const string ConfirmButtonText = "OK";

		private const string ContainerName = "online-shop-scene-toolbar";
		private const string ContainerClassName = "scene-toolbar";
		private const string ToolbarZoneName = "ToolbarZoneLeftAlign";
		private const string StyleSheetPath = "Assets/Editor/SceneToolbar.uss";
		private const string LogonScenePath = "Assets/Scenes/LogonScene.unity";
		private const string MainScenePath = "Assets/Scenes/MainScene.unity";
		private const double UpdateInterval = 0.5;

		private static readonly Type _toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

		private static readonly FieldInfo _rootField =
			_toolbarType?.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);

		private static VisualElement _container;
		private static double _nextUpdate;

		private static bool CanOpenScene => !EditorApplication.isPlayingOrWillChangePlaymode
			&& !EditorApplication.isCompiling
			&& !EditorApplication.isUpdating;

		static SceneToolbar()
		{
			if (_rootField != null)
				EditorApplication.update += Update;
		}

		private static void Update()
		{
			if (EditorApplication.timeSinceStartup < _nextUpdate)
				return;

			_nextUpdate = EditorApplication.timeSinceStartup + UpdateInterval;
			if (_container == null || _container.panel == null)
				Attach();

			_container?.SetEnabled(CanOpenScene);
		}

		private static void Attach()
		{
			foreach (Object toolbar in Resources.FindObjectsOfTypeAll(_toolbarType))
			{
				VisualElement root = _rootField.GetValue(toolbar) as VisualElement;
				VisualElement zone = root?.Q(ToolbarZoneName);
				if (zone == null)
					continue;

				_container = zone.Q(ContainerName);
				if (_container != null)
					return;

				StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
				if (styleSheet == null)
					return;

				_container = new VisualElement { name = ContainerName };
				_container.AddToClassList(ContainerClassName);
				_container.styleSheets.Add(styleSheet);
				AddButton(LogonButtonText, LogonScenePath, false);
				AddButton(MainButtonText, MainScenePath, false);
				AddButton(LogonPlayButtonText, LogonScenePath, true);
				zone.Add(_container);
				return;
			}
		}

		private static void AddButton(string label, string scenePath, bool play)
		{
			ToolbarButton button = new ToolbarButton(() => OpenScene(scenePath, play))
			{
				text = label,
				tooltip = play ? LogonPlayTooltip : OpenSceneTooltipPrefix + scenePath
			};
			_container.Add(button);
		}

		private static void OpenScene(string scenePath, bool play)
		{
			if (!CanOpenScene)
				return;

			if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
			{
				EditorUtility.DisplayDialog(SceneNotFoundText, scenePath, ConfirmButtonText);
				return;
			}

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return;

			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
			if (play)
				EditorApplication.EnterPlaymode();
		}
	}
}

