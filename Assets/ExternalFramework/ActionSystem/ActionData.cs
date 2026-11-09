using System;
using System.Collections.Generic;
using UnityEngine;

namespace Games.ActionSystem
{
    
    
    [Serializable]
    public class ActionData
    {
        public Dictionary<string, Action<object>> actions;

        public ActionData()
        {
            actions = new Dictionary<string, Action<object>>();
        }

        public Action<object> GetOrCreateAction(string key, Action<object> action)
        {
            if (!actions.ContainsKey(key))
            {
                actions.Add(key, action);
            }
            // else
            // {
            //     Debug.LogError("Action with key " + key + " already exists");
            // }
            return actions[key];
        }

        public void RemoveAction(string key)
        {
            if (actions.ContainsKey(key))
            {
                actions.Remove(key);
            }
        }
        

        public void ExecuteAction(string key, object parameter)
        {
            if (actions.ContainsKey(key))
            {
                actions[key]?.Invoke(parameter);
            }
        }
    }
}