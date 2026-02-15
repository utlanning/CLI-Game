using Isekai.VSlice.Core.Runtime;

namespace Isekai.VSlice.Core.Systems;

public static class Los
{
    // Bresenham line over grid centers; blocked tiles stop LoS.
    public static bool HasLineOfSight(BattleState s, int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            // skip the origin tile; but block on any intermediate tile and target tile if blocked
            if (!(x == x0 && y == y0))
            {
                if (s.IsBlocked(x, y)) return false;
            }

            if (x == x1 && y == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }

        return true;
    }
}