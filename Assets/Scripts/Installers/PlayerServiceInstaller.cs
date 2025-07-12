using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Blizzard.Player
{
    public class PlayerServiceInstaller : MonoInstaller
    {
        [SerializeField] PlayerCtrl _playerPrefab;
        [SerializeField] Transform _environment;
        [SerializeField] CinemachineCamera _cinemachineCamera;

        public override void InstallBindings()
        {
            Debug.Log("Installing player service...");
            Container.BindInterfacesAndSelfTo<PlayerService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_playerPrefab, _environment, _cinemachineCamera);
        }
    }
}
