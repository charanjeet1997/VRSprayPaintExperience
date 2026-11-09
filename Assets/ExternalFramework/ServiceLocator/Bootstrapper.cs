using System;
using DataBindingFramework;
using Games.ActionSystem;
using Games.CameraManager;
using UnityEngine;
using Games.GameStateFramework;


namespace ServiceLocatorFramework
{
	public static class Bootstrapper
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void InitiailzeBeforeSceneLoad()
		{
			// Debug.Log("Initialized");
			// Initialize default service locator.
			ServiceLocator.Initiailze();
			ServiceLocator.Current.Register<IObserverManager>(new ObserverManager());
			ServiceLocator.Current.Register<IPropertyManager>(new PropertyManager());
			ServiceLocator.Current.Register<ICameraManager>(new CameraManager());
			ServiceLocator.Current.Register(new ActionData());
		}
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void InitializeAfterSceneLoad()
		{
		}

		
	}
}