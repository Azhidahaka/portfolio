using System;
using System.Collections.Generic;
using UnityEngine;

public class Dispatcher : IDispatcher {
    public List<Action> pending = new List<Action>();
    private static Dispatcher instance;

    public static Dispatcher Instance {
        get {
            if (instance == null) 
                instance = new Dispatcher();
            
            return instance;
        }
    }

    // Schedule code for execution in the main-thread.
    public void Invoke(Action action) {
        lock (pending) {
            pending.Add(action);
        }
    }

    // Execute pending actions.
    public void InvokePending() {
        lock (pending) {
            foreach (Action action in pending) {
                Debug.LogWarning($"InvokePending::{action.Target.ToString()}::{action.Method.Name}");
                action(); // Invoke the action.
            }

            pending.Clear(); // Clear the pending list.
        }
    }

    public static void AddAction(Action action) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"AddAction::{action.Method.Name}");
#endif
        Instance.Invoke(action);
    }
}