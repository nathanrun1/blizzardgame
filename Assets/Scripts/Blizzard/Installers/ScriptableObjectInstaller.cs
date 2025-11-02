using UnityEngine;
using Zenject;
using System;
using System.Collections.Generic;
using Blizzard.Utilities.Logging;

namespace Blizzard.Installers
{
    public class ScriptableObjectInstaller : MonoInstaller
    {
        [SerializeField] private List<ScriptableObject> scriptableObjects;

        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            BLog.Log("Injecting into SOs");
            foreach (var so in scriptableObjects)
            {
                Container.QueueForInject(so);
                BLog.Log($"Injecting into {so.name}");
            }
        }
    }
}