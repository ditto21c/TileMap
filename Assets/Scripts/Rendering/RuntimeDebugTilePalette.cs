using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMap.Data;
using TileMap.Utility;

namespace TileMap.Rendering
{
    public class RuntimeDebugTilePalette : MonoBehaviour
    {
        private readonly Dictionary<byte, TileBase> tileCache = new Dictionary<byte, TileBase>(256);

        public TileBase GetTile(byte tileId)
        {
            if (!tileCache.TryGetValue(tileId, out var tile))
            {
                tile = CreateRuntimeTile(tileId);
                tileCache[tileId] = tile;
            }

            return tile;
        }

        private TileBase CreateRuntimeTile(byte tileId)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = RuntimeSpriteFactory.GetWhiteSprite();
            tile.color = GetColor((TileId)tileId);
            tile.colliderType = Tile.ColliderType.None;
            tile.name = $"RuntimeTile_{tileId}";
            return tile;
        }

        private Color GetColor(TileId tileId)
        {
            switch (tileId)
            {
                case TileId.Void: return new Color32(0, 0, 0, 0);
                case TileId.SpawnPlayer: return new Color32(250, 244, 116, 255);
                case TileId.GrassA: return new Color32(85, 153, 66, 255);
                case TileId.GrassB: return new Color32(101, 171, 78, 255);
                case TileId.DirtA: return new Color32(132, 103, 70, 255);
                case TileId.RoadA: return new Color32(194, 175, 118, 255);
                case TileId.TallGrass: return new Color32(52, 120, 44, 255);
                case TileId.RockLarge: return new Color32(112, 112, 120, 255);
                case TileId.RuinFloor: return new Color32(146, 132, 112, 255);
                case TileId.OldWall: return new Color32(92, 84, 78, 255);
                case TileId.Campfire: return new Color32(255, 165, 79, 255);
                case TileId.MerchantSpot: return new Color32(117, 198, 255, 255);
                case TileId.EnemyTier1:
                case TileId.EnemyTier3:
                case TileId.EnemyCave1:
                case TileId.EnemyFire1:
                    return new Color32(219, 85, 85, 255);
                case TileId.OreCopper: return new Color32(194, 111, 67, 255);
                case TileId.OreIron: return new Color32(144, 147, 156, 255);
                case TileId.OreSilver: return new Color32(204, 214, 227, 255);
                case TileId.OreMythril: return new Color32(113, 234, 255, 255);
                case TileId.WaterShallow: return new Color32(78, 164, 217, 255);
                case TileId.WaterDeep: return new Color32(48, 90, 173, 255);
                case TileId.ForestFloorA: return new Color32(48, 109, 50, 255);
                case TileId.ForestFloorB: return new Color32(40, 92, 43, 255);
                case TileId.BushDense: return new Color32(26, 84, 33, 255);
                case TileId.TreePine: return new Color32(20, 70, 28, 255);
                case TileId.SwampShallow: return new Color32(79, 108, 77, 255);
                case TileId.SwampDeep: return new Color32(45, 62, 52, 255);
                case TileId.ResourceWood: return new Color32(111, 78, 48, 255);
                case TileId.ShrineNature: return new Color32(164, 240, 135, 255);
                case TileId.TreasureForest:
                case TileId.TreasureMinor:
                case TileId.TreasureCave:
                    return new Color32(253, 214, 84, 255);
                case TileId.CaveFloorA: return new Color32(87, 93, 106, 255);
                case TileId.Pit: return new Color32(21, 22, 28, 255);
                case TileId.MineTrack: return new Color32(146, 123, 83, 255);
                case TileId.CaveBossGate: return new Color32(157, 96, 176, 255);
                case TileId.ForgeUnderground: return new Color32(240, 128, 54, 255);
                case TileId.AshGround: return new Color32(104, 86, 86, 255);
                case TileId.LavaShallow: return new Color32(232, 113, 47, 255);
                case TileId.LavaDeep: return new Color32(184, 42, 18, 255);
                case TileId.BossArena: return new Color32(255, 87, 87, 255);
                case TileId.ShrineFire: return new Color32(255, 189, 85, 255);
                case TileId.EndPortal: return new Color32(154, 255, 225, 255);
                case TileId.EventMinor: return new Color32(177, 124, 255, 255);
                default: return Color.magenta;
            }
        }
    }
}
