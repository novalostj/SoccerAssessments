namespace Quantum.Game
{
    unsafe class GoalSpawner : SystemMainThreadFilter
    {
        public override void OnInit(Frame f)
        {
            base.OnInit(f);
            EntityRef entityRef = f.Create(f.RuntimeConfig.goal);
        }
    }
}
