using UnityEngine;

namespace Blizzard.Utilities.Assistants
{
    /// <summary>
    /// Random number generation utility methods
    /// </summary>
    public static class RandomAssistant
    {
        /// <summary>
        /// Generates a random value from a binomial distribution
        /// </summary>
        /// <param name="n"># of trials</param>
        /// <param name="p">Probability of success</param>
        /// <returns></returns>
        public static int GenerateBinomial(int n, float p)
        {
            int successCount = 0;
            for (int i = 0; i < n; ++i)
            {
                if (Random.value <= p) successCount++;
            }

            return successCount;
        }
    }
}