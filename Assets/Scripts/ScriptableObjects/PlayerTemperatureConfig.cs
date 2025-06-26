using UnityEngine;

[CreateAssetMenu(fileName = "PlayerTemperatureConfig", menuName = "ScriptableObjects/Config/PlayerTemperatureConfig")]
public class PlayerTemperatureConfig : ScriptableObject
{
    /// <summary>
    /// Rate of change of body temperature. Effect on temperature delta is calculated as 'bodyTemperatureChangeRate' * (externalTemperature - InternalTemperature)
    /// </summary>
    public float bodyTemperatureChangeRate;
    /// <summary>
    /// Neutral body temperature, used to calculate body heat such that there is an equilibrium at a temperature difference of (neutralExternal - neutralBody).
    /// Should be set to the intended "normal" body temperature of the player to be displayed.
    /// </summary>
    public float neutralBodyTemperature = 36f;
    /// <summary>
    /// Neutral external temperature, used to calculate body heat such that there is an equilibrium at a temperature difference of (neutralExternal - neutralBody).
    /// Should be set to the intended "normal" external temperature where the player would be comfortable when wearing no insulation.
    /// </summary>
    public float neutralExternalTemperature = 20f;
}
