using Photon.Deterministic;
using System;

namespace Quantum.Game
{
    public unsafe class Goal : SystemMainThreadFilter, ISignalOnCollisionEnter3D
    {
        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            if (!f.IsVerified || !f.Unsafe.TryGetPointer(info.Other, out BallInstance* ballInstance))
                return;

            EntityRef lastBallHolder = ballInstance->lastHolderRef;
            PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(lastBallHolder);
            playerLink->score++;
            f.Events.Goal();

            ResetBall(f);
            GivePlayersAbility(f);
            /*
            Transform3D* ballTransform = f.Unsafe.GetPointer<Transform3D>(info.Other);
            ballTransform.Position = 
            f.Set(info.Other, ballTransform);

            ComponentSet anyPlayer = ComponentSet.Create<PlayerLink>();

            ComponentFilter<Transform3D> filteredPlayers = f.Filter<Transform3D>(default, anyPlayer);

            

            while (filteredPlayers.Next(out var entity, out var playerTransform))
            {
                playerTransform.Position = new FPVector3(0, 1, -4);
                f.Set(entity, playerTransform);
            }
            */
        }

        private static void ResetBall(Frame f)
        {
            ComponentSet anyBalls = ComponentSet.Create<BallInstance>();
            ComponentFilter<Transform3D> filteredBalls = f.Filter<Transform3D>(default, anyBalls);
            while (filteredBalls.Next(out var ball, out var ballTransform))
            {
                ballTransform.Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 1, 0);
                f.Set(ball, ballTransform);
            }
        }

        private static void GivePlayersAbility(Frame f)
        {
            ComponentSet anyPlayer = ComponentSet.Create<PlayerLink>();
            ComponentFilter<PlayerLink> filteredPlayers = f.Filter<PlayerLink>(default, anyPlayer);
            while (filteredPlayers.Next(out var playerEntity, out var playerLink))
            {
                playerLink.bounceBallAbility = true;
                f.Set(playerEntity, playerLink);
            }
        }
    }
}
