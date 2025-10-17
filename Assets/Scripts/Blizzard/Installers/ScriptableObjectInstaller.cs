using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;

namespace Blizzard.Installers
{
    public class ScriptableObjectInstaller : MonoInstaller
    {
        [SerializeField] List<ScriptableObject> scriptableObjects;

        public override void InstallBindings()
        {
            Debug.Log("Injecting into SOs");
            foreach (ScriptableObject so in scriptableObjects)
            {
                Container.QueueForInject(so);
                Debug.Log($"Injecting into {so.name}");
            }
        }
    }
}
