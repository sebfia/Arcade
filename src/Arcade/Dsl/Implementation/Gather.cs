namespace Arcade.Dsl.Implementation
{
    public sealed class Gather
    {
        private readonly IGatherFlowConfigurer _last;

        public Gather(IGatherFlowConfigurer last)
        {
            _last = last;
        }

        public IGatherFlowConfigurer Last
        {
            get { return _last; }
        }
    }
}