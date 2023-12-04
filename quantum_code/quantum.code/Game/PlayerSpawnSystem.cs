using Photon.Deterministic;
using Quantum.Helpers;

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
            playerLink->Entity = entityRef;

            playerLink->colorR = f.Global->RngSession.Next(0, 255);
            playerLink->colorG = f.Global->RngSession.Next(0, 255);
            playerLink->colorB = f.Global->RngSession.Next(0, 255);

            f.Unsafe.TryGetPointer(entityRef, out Transform3D* transform);

            transform->Position = new FPVector3(f.Global->RngSession.Next(-4, 4), 2, -4);
            f.Global->playerCount = f.GetActivePlayerCount();
            f.Global->gameStarted = f.Global->playerCount >= 2;
        }

        public void OnPlayerDisconnected(Frame f, PlayerRef player)
        {
            foreach (var playerLink in f.GetComponentIterator<PlayerLink>())
            {
                if (playerLink.Component.Player != player) continue;
                f.Destroy(playerLink.Entity);
            }
            int playerCount = f.GetActivePlayerCount();
            f.Global->playerCount = playerCount;

            if (playerCount < 2)
                Reset(f);
        }

        private void Reset(Frame f)
        {
            ComponentSet anyBalls = ComponentSet.Create<BallInstance>();
            ComponentFilter<Transform3D> filteredBalls = f.Filter<Transform3D>(default, anyBalls);

            ComponentSet anyLink = ComponentSet.Create<PlayerLink>();
            ComponentFilter<Transform3D> filteredPlayers = f.Filter<Transform3D>(default, anyLink);

            while (filteredBalls.Next(out var ballEntity, out var transform))
                f.Destroy(ballEntity);

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
    }
}