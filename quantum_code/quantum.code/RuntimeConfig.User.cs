using Photon.Deterministic;
using System;

namespace Quantum 
{
    partial class RuntimeConfig 
    {
        public AssetRefEntityPrototype characterEntity;
        public AssetRefEntityPrototype ball;
        public AssetRefEntityPrototype goal;

        partial void SerializeUserData(BitStream stream)
        {
            stream.Serialize(ref characterEntity);
            stream.Serialize(ref ball);
            stream.Serialize(ref goal);
        }
    }
}