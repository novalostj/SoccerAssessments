using System;

namespace Quantum.Player
{
    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterController3D* CharacterController;
            public Transform3D* Transform;
        }

        public override void OnInit(Frame f)
        {
            base.OnInit(f);
            f.UpdatePlayerData();
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            Input input = default;

            filter.Transform->Position.Y = 1;

            bool hasLink = frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* link);
            if (frame.Global->gameStarted && hasLink)
            {
                input = *frame.GetPlayerInput(link->Player);
            }

            if (input.Jump.WasPressed && link->bounceBallAbility)
            {
                link->bounceBallAbility = false;
                var ballFilter = frame.Filter<BallInstance>();
                while (ballFilter.Next(out var ballEntity, out var ballInstance))
                {
                    ballInstance.bounceBack = true;
                    frame.Set(ballEntity, ballInstance);
                }
            }

            filter.CharacterController->Move(frame, filter.Entity, input.Direction.XOY);
        }

        private void Stats(Frame frame, Filter filter)
        {
            frame.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* link);
            int linkID = link->Player;
            int ?actorID = frame.PlayerToActorId(link->Player);

            Log.Write($"TEST {linkID} || {actorID}");
        }
    }
}
