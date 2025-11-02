// using Zenject;
// using Blizzard.Obstacles;
// using Blizzard.Utilities.Logging;
//
// namespace Blizzard.Installers
// {
//     public class InteractionServiceInstaller : MonoInstaller
//     {
//         // ReSharper disable Unity.PerformanceAnalysis
//         public override void InstallBindings()
//         {
//             Container.BindInterfacesAndSelfTo<InteractionService>()
//                 .AsSingle();
//
//             BLog.Log("Installed Interaction Service");
//         }
//     }
// }