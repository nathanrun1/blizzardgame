using Unity.Cinemachine;
using UnityEngine;
using Zenject;
using Blizzard.Player;

namespace Blizzard.Installers
{
    public class PlayerServiceInstaller : MonoInstaller
    {
        [SerializeField] private PlayerCtrl _playerPrefab;
        [SerializeField] private Transform _environment;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerService>()
                .FromNew()
                .AsSingle()
                .WithArguments(_playerPrefab, _environment, _cinemachineCamera);

            Debug.Log("Installed Player Service");
        }
    }
}
