
public static class Constants
{
    public static class Layers
    {
        public static readonly int TERRAIN = 9;
        public static readonly int TERRAIN_MASK = 1 << TERRAIN;

        public static readonly int UNIT = 10;
        public static readonly int UNIT_MASK = 1 << UNIT;

        public static readonly int BUILDING = 11;
        public static readonly int BUILDING_MASK = 1 << BUILDING;

        public static readonly int LIGHTHOUSE = 12;
        public static readonly int LIGHTHOUSE_MASK = 1 << LIGHTHOUSE;

        public static readonly int SELECTABLE_MASK = (UNIT_MASK | BUILDING_MASK);
        public static readonly int HIT_MASK = (UNIT_MASK | BUILDING_MASK | TERRAIN_MASK);
    }
}
