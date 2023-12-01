using Photon.Deterministic;
using System;

namespace Quantum.Game
{
    public unsafe class PlayerSpawnSystem : SystemMainThreadFilter, ISignalOnPlayerDisconnected, ISignalOnPlayerConnected
    {
        void SpawnPlayer(Frame f, PlayerRef player)
        {
            Log.Write($"Create player for {player}");


            RuntimePlayer data = f.GetPlayerData(player);

            EntityRef entityRef = f.Create(f.RuntimeConfig.characterEntity);
            PlayerLink* playerLink = f.Unsafe.GetPointer<PlayerLink>(entityRef);
            playerLink->Player = player;
            playerLink->score = 0;
            playerLink->bounceBallAbility = true;

            f.Unsafe.TryGetPointer(entityRef, out Transform3D* transform);

            transform->Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 2, -4);
            f.Global->playerCount = GetPlayerCount(f);
            f.Global->gameStarted = f.Global->playerCount >= 2;
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            foreach (var playerLink in f.GetComponentIterator<PlayerLink>())
            {
                if (playerLink.Component.Player != player) continue;
                f.Destroy(playerLink.Entity);
            }
            int playerCount = GetPlayerCount(f);
            f.Global->playerCount = playerCount;

            if (playerCount < 2)
                Reset(f);
        }

        private void Reset(Frame f)
        {
            ComponentSet anyBalls = ComponentSet.Create<BallInstance>();
            ComponentFilter<Transform3D, PhysicsBody3D> filteredBalls = f.Filter<Transform3D, PhysicsBody3D>(default, anyBalls);

            ComponentSet anyLink = ComponentSet.Create<PlayerLink>();
            ComponentFilter<Transform3D> filteredPlayers = f.Filter<Transform3D>(default, anyLink);

            while (filteredBalls.Next(out var ballEntity, out var transform, out var physicsBody))
            {
                transform.Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 1, 0);
                physicsBody.Velocity = FPVector3.Zero;

                f.Set(ballEntity, transform);
                f.Set(ballEntity, physicsBody);
            }

            while (filteredPlayers.Next(out var playerEntity, out var transform))
            {
                transform.Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 2, -4);
                f.Unsafe.GetPointer<PlayerLink>(playerEntity)->score = 0;
                f.Global->gameStarted = false;

                f.Set(playerEntity, transform);
            }
        }

        public void OnPlayerConnected(Frame f, PlayerRef player)
        {
            SpawnPlayer(f, player);
        }

        private int GetPlayerCount(Frame f)
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