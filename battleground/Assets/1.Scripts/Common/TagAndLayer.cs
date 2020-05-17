using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;
namespace FC
{
    public class TagAndLayer
    {
        public class LayerName
        {
            public const string Default = "Default";
            public const string TrasnparentFX = "TransparentFX";
            public const string IgnoreRayCast = "Ignore Raycast";
            public const string Water = "Water";
            public const string UI = "UI";
            public const string Cover = "Cover";
            public const string IgnoreShot = "Ignore Shot";
            public const string CoverInvisible = "Cover Invisible";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string Bound = "Bound";
            public const string Environment = "Environment";
        }

        public enum LayerIndex
        {
            Default = 0,
            TransparentFX = 1,
            IgnoreRayCast = 2,
            Water  = 4,
            UI = 5,
            Cover = 8,
            IgnoreShot = 9,
            CoverInvisible = 10,
            Player = 11,
            Enemy = 12,
            Bound = 13,
            Environment = 14,
            
        }

        public static int GetLayerByName(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        public class TagName
        {
            public const string Untagged = "Untagged";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string GameController = "GameController";
            public const string Finish = "Finish";
        }

    }
}

