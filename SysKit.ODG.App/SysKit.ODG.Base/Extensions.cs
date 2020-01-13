using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysKit.ODG.Base.DTO.Generation;
using SysKit.ODG.Base.Utils;

namespace SysKit.ODG.Base
{
    public static class Extensions
    {
        public static IEnumerable<T> GetRandom<T>(this List<T> source, int number)
        {
            var usedEntries = new HashSet<int>();
            var maxValue = source.Count;
            var numberOfEntries = Math.Min(number, maxValue);
            int counter = 0;

            while (counter < numberOfEntries)
            {
                int randomIndex;
                do
                {
                    randomIndex = RandomThreadSafeGenerator.Next(maxValue);
                } while (usedEntries.Contains(randomIndex));

                usedEntries.Add(randomIndex);
                counter++;
                yield return source[randomIndex];
            }
        }
    }
}
