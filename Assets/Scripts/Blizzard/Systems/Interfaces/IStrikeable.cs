namespace Blizzard.Interfaces
{
    /// <summary>
    /// A GameObject that is "Strikeable" by a general purpose player-sided weapon. GameObjects implementing
    /// this interface should be a part of the "Strikeable" collision layer group.
    /// </summary>
    public interface IStrikeable
    {
        /// <summary>
        /// Strikes the Strikeable, inflicting damage.
        /// </summary>
        /// <param name="damage">Damage to inflict</param>
        /// <param name="death">Whether the strike causes the death of the strikeable</param>
        public void Strike(int damage, out bool death);
    }
}