namespace LoopLegacy.Region
{
    public class RegionEffect
    {
        public RegionEffectType Type { get; private set; }
        public int Duration { get; private set; }

        public RegionEffect(RegionEffectType type, int duration)
        {
            this.Type = type;
            this.Duration = duration;
        }

        public void ReduceDuration()
        {
            Duration--;
        }
    }
}