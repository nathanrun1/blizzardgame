namespace Blizzard.Player.Tools
{
    /// <summary>
    /// A GameObject that is "hittable" by a tool. GameObjects with this interface
    /// should be included in the "Hittable" collision layer group.
    /// </summary>
    public interface IHittable
    {
        /// <summary>
        /// Hits the hittable with given damage and tool type
        /// </summary>
        /// <param name="toolType"></param>
        /// <param name="damage"></param>
        /// <param name="death">Whether hit causes the "death" of the hittable</param>
        public void Hit(int damage, ToolType toolType, out bool death);
    }
}