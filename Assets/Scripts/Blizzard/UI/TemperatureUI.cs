using System;
using UnityEngine;
using Zenject;
using TMPro;
using Blizzard.Player;
using Blizzard.Temperature;
using Blizzard.UI.Core;


// TODO: temperature UI show body and area temp using services thx

namespace Blizzard.UI
{
    public class TemperatureUI : UIBase
    {
        [Inject] private PlayerService _playerService;
        [Inject] private TemperatureService _temperatureService;

        [Header("References")] [SerializeField]
        private TextMeshProUGUI _bodyTemperature;

        [SerializeField] private TextMeshProUGUI _areaTemperature;

        public override void Setup(object args)
        {
        }

        private void FixedUpdate()
        {
            _bodyTemperature.text =
                "Body: " + Math.Round(_playerService.PlayerTemperature.BodyTemperature, 2).ToString() + '�';
            _areaTemperature.text = "Area: " +
                                    Math.Round(
                                        _temperatureService.GetTemperatureAtWorldPos(_playerService.PlayerCtrl.transform
                                            .position), 2).ToString() + '�';
        }
    }
}