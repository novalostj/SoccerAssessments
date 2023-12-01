using Photon.Deterministic;

namespace Quantum.Game
{
    public unsafe class Ball : SystemMainThreadFilter<Ball.Filter>, ISignalOnCollision3D
    {
        public struct Filter
        {
            public EntityRef BallEntity;
            public PhysicsBody3D* PhysicsBody;
            public Transform3D* Transform;
        }

        public void OnCollision3D(Frame f, CollisionInfo3D info)
        {
            if (!f.IsVerified || !f.Unsafe.TryGetPointer(info.Other, out PlayerLink* playerLink))
                return;

            f.Unsafe.GetPointer<BallInstance>(info.Entity)->lastHolderRef = info.Other;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.Transform->Position.Y = 1;
            if (!f.Unsafe.TryGetPointer<BallInstance>(filter.BallEntity, out var BallInstance) || !BallInstance->bounceBack)
                return;

            filter.PhysicsBody->AddLinearImpulse(FPVector3.Back * BallInstance->bouceBackForce);
            BallInstance->bounceBack = false;
        }
    }
}
