using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>
///
/// </summary>
namespace Landscape2.Runtime.LicenseAuth
{
    public class NamedPipeDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> queue = new ConcurrentQueue<Action>();

        private void Update()
        {
            while (queue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        public static void Enqueue(Action action)
        {
            queue.Enqueue(action);
        }
    }
}