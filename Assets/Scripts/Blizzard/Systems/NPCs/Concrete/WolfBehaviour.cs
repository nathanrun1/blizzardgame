using Blizzard.NPCs.Concrete.Wolf;
using Blizzard.Utilities;
using UnityEngine;

namespace Blizzard.NPCs.Concrete
{
    public class WolfBehaviour : NPCBehaviour
    {
        [Header("Behaviour Config")] 
        [SerializeField] private WolfConfig _config;

        private StateMachine _stateMachine;
    }
}