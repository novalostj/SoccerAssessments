using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Helpers
{
    public static class MyUtil
    {
        public static int GetActivePlayerCount(this Frame f)
        {
            int count = 0;

            ComponentSet anyLink = ComponentSet.Create<PlayerLink>();
            ComponentFilter<Transform3D> filteredPlayers = f.Filter<Transform3D>(default, anyLink);

            while (filteredPlayers.Next(out _, out _))
                count++;

            return count;
        }

    }
}
