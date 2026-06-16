using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMap.Data
{
    public enum AutoTileMaskSource
    {
        RenderLayer,
        GroundLayer,
        OverlayLayer,
        EffectiveLayer
    }

    [CreateAssetMenu(menuName = "Tools/World/Auto Tile Set", fileName = "AutoTileSet")]
    public class AutoTileSet : ScriptableObject
    {
        [SerializeField] private TileId tileId;
        [SerializeField] private TileId[] connectsTo = new TileId[0];
        [SerializeField] private AutoTileMaskSource maskSource = AutoTileMaskSource.RenderLayer;
        [SerializeField] private TileBase fallbackTile;
        [SerializeField] private bool autoFillMasksFromNineSlice = true;
        [SerializeField] private bool clearMasksBeforeAutoFill = true;
        [SerializeField] private NineSliceTiles nineSliceTiles = new NineSliceTiles();
        [SerializeField, HideInInspector] private TileBase[] tilesByMask = new TileBase[256];

        public TileId TileId => tileId;
        public AutoTileMaskSource MaskSource => maskSource;
        public TileBase FallbackTile => fallbackTile;
        public bool HasFallbackTile => fallbackTile != null;

#pragma warning disable 0649
        [System.Serializable]
        private class NineSliceTiles
        {
            [Header("Optional")]
            public TileBase isolated;

            [Header("3x3 Patch")]
            public TileBase topLeft;
            public TileBase top;
            public TileBase topRight;
            public TileBase left;
            public TileBase center;
            public TileBase right;
            public TileBase bottomLeft;
            public TileBase bottom;
            public TileBase bottomRight;

            public bool HasAnyTile()
            {
                return isolated != null
                    || topLeft != null
                    || top != null
                    || topRight != null
                    || left != null
                    || center != null
                    || right != null
                    || bottomLeft != null
                    || bottom != null
                    || bottomRight != null;
            }
        }
#pragma warning restore 0649

        public bool Matches(byte otherTileId)
        {
            if (otherTileId == (byte)tileId)
            {
                return true;
            }

            for (int i = 0; i < connectsTo.Length; i++)
            {
                if (otherTileId == (byte)connectsTo[i])
                {
                    return true;
                }
            }

            return false;
        }

        public TileBase GetTile(int mask)
        {
            if (mask >= 0 && mask < tilesByMask.Length && tilesByMask[mask] != null)
            {
                return tilesByMask[mask];
            }

            return fallbackTile;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureMaskArray();

            if (autoFillMasksFromNineSlice)
            {
                AutoFillMasksFromNineSlice();
            }
        }

        [ContextMenu("Auto Fill Masks From Nine Slice")]
        private void AutoFillMasksFromNineSlice()
        {
            EnsureMaskArray();
            if (nineSliceTiles == null || !nineSliceTiles.HasAnyTile())
            {
                return;
            }

            if (clearMasksBeforeAutoFill)
            {
                for (int i = 0; i < tilesByMask.Length; i++)
                {
                    tilesByMask[i] = null;
                }
            }

            for (int mask = 0; mask < tilesByMask.Length; mask++)
            {
                TileBase tile = ResolveNineSliceTile(mask);
                if (tile != null)
                {
                    tilesByMask[mask] = tile;
                }
            }

            if (fallbackTile == null)
            {
                fallbackTile = nineSliceTiles.center
                    ?? nineSliceTiles.isolated
                    ?? nineSliceTiles.top
                    ?? nineSliceTiles.left
                    ?? nineSliceTiles.topLeft;
            }
        }

        private TileBase ResolveNineSliceTile(int mask)
        {
            bool up = Has(mask, 1);
            bool right = Has(mask, 4);
            bool down = Has(mask, 16);
            bool left = Has(mask, 64);

            if (!up && !right && !down && !left && nineSliceTiles.isolated != null)
            {
                return nineSliceTiles.isolated;
            }

            if (!up && !left && nineSliceTiles.topLeft != null)
            {
                return nineSliceTiles.topLeft;
            }

            if (!up && !right && nineSliceTiles.topRight != null)
            {
                return nineSliceTiles.topRight;
            }

            if (!down && !left && nineSliceTiles.bottomLeft != null)
            {
                return nineSliceTiles.bottomLeft;
            }

            if (!down && !right && nineSliceTiles.bottomRight != null)
            {
                return nineSliceTiles.bottomRight;
            }

            if (!up && nineSliceTiles.top != null)
            {
                return nineSliceTiles.top;
            }

            if (!down && nineSliceTiles.bottom != null)
            {
                return nineSliceTiles.bottom;
            }

            if (!left && nineSliceTiles.left != null)
            {
                return nineSliceTiles.left;
            }

            if (!right && nineSliceTiles.right != null)
            {
                return nineSliceTiles.right;
            }

            return nineSliceTiles.center;
        }

        private void EnsureMaskArray()
        {
            if (tilesByMask != null && tilesByMask.Length == 256)
            {
                return;
            }

            var resized = new TileBase[256];
            if (tilesByMask != null)
            {
                int copyCount = Mathf.Min(tilesByMask.Length, resized.Length);
                for (int i = 0; i < copyCount; i++)
                {
                    resized[i] = tilesByMask[i];
                }
            }

            tilesByMask = resized;
        }

        private static bool Has(int mask, int bit)
        {
            return (mask & bit) != 0;
        }
#endif
    }
}
