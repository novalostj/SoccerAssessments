using Photon.Deterministic;
using Quantum.Helpers;
using System;

namespace Quantum.Game
{
    public unsafe class BallSpawner : SystemMainThreadFilter, ISignalOnPlayerConnected
    {

        public void OnPlayerConnected(Frame f, PlayerRef player)
        {
            if (f.GetActivePlayerCount() <= 1)
                return;

            ComponentSet anyBalls = ComponentSet.Create<BallInstance>();
            ComponentFilter<Transform3D> filteredBalls = f.Filter<Transform3D>(default, anyBalls);

            while (filteredBalls.Next(out _, out _))
                return;

            SpawnBalls(f);
        }

        public void SpawnBalls(Frame f)
        {
            EntityRef entityRef = f.Create(f.RuntimeConfig.ball);
            var ballTransform = f.Unsafe.GetPointer<Transform3D>(entityRef);
            var ballInstance = f.Unsafe.GetPointer<BallInstance>(entityRef);

            ballTransform->Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 1, 0);
            ballInstance->instance = entityRef;
        }

    }
}
