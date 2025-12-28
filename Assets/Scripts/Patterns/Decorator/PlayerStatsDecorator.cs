namespace Patterns.Decorator
{
    // Decorator base class: default olarak inner statlarý aynen geçirir.
    public abstract class PlayerStatsDecorator : IPlayerStats
    {
        protected readonly IPlayerStats inner;

        protected PlayerStatsDecorator(IPlayerStats inner)
        {
            this.inner = inner;
        }

        public virtual float Speed => inner.Speed;
        public virtual int BombCount => inner.BombCount;
        public virtual int BombPower => inner.BombPower;
    }
}
