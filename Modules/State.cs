using System.Collections.Generic;
using UnityEngine;

namespace Radon.Modules
{
    internal class State
    {
        public LevelData Level { get; }
        public Vector3 Position { get; }
        public float RotationX { get; }
        public float RotationY { get; }
        public List<(string id, int rank, int ammo)> Cards { get; }

        public State(LevelData level, Vector3 position, float rotationX, float rotationY, List<(string id, int rank, int ammo)> cards)
        {
            Level = level;
            Position = position;
            RotationX = rotationX;
            RotationY = rotationY;
            Cards = cards;
        }
    }
}
