using System.Collections.Generic;
using UnityEngine;

namespace Radon.Modules
{
    internal struct CardState
    {
        public string Id { get; }
        public int Rank { get; }
        public int Ammo { get; }

        public CardState(string id, int rank, int ammo)
        {
            Id = id;
            Rank = rank;
            Ammo = ammo;
        }
    }

    internal struct PickupState
    {
        public string Id { get; }
        public Vector3 Position { get; }
        public int Ammo { get; }

        public PickupState(string id, Vector3 position, int ammo)
        {
            Id = id;
            Position = position;
            Ammo = ammo;
        }
    }

    internal struct WallState
    {
        public GameObject Wall { get; }
        public string Scene { get; }

        public WallState(GameObject wall, string scene)
        {
            Wall = wall;
            Scene = scene;
        }
    }

    internal class State
    {
        public LevelData Level { get; set; }
        public Vector3 Position { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public List<CardState> Cards { get; set; }
        public int Health { get; set; }
        public HashSet<string> DeadEnemies { get; set; }
        public List<PickupState> Pickups { get; set; }
        public HashSet<string> SpawnerCards { get; set; }
        public HashSet<string> Chests { get; set; }
        public List<WallState> Walls { get; set; }
    }
}
