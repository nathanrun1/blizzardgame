using Blizzard.NPCs.BaseStates;
using Blizzard.Obstacles;
using Blizzard.Player;
using Blizzard.Utilities;
using UnityEngine;

namespace Blizzard.NPCs.Concrete.Wolf
{
    public class WolfContext : PassiveNPCContext
    {
        
    }
    
    public abstract class WolfState : PassiveNPCState
    {
        public override void Enter(IStateContext ctx)
        {
            _ctx = ctx as WolfContext;
        }
    }
    
}