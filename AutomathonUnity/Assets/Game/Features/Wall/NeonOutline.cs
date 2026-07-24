using UnityEngine;

namespace Automathon.Game
{
    // Turns a SpriteRenderer into a hollow glowing rectangle: hides the filled sprite and draws its
    // outline with a LineRenderer. The line lives on a separate, unscaled, world-space object
    // (fixed world-unit width) so a non-uniform parent scale can never stretch the line.
    //
    // Authored on the Wall/Shield prefab and configured in the Inspector. Static objects (walls,
    // whose size is set at spawn) call Rebuild() once; moving objects (shields) set Follow = true.
    public class NeonOutline : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetSprite; // sprite to outline (authored on the prefab)
        [SerializeField] private Material outlineMaterial;    // additive HDR line material asset
        [SerializeField] private float width = 0.07f;         // world units
        [SerializeField] private bool follow = false;         // recompute every frame (moving/rotating objects)

        private LineRenderer lr;
        private GameObject lineGo;

        private void Start()
        {
            EnsureLine();
            Rebuild();
        }

        // Rebuild the outline from the sprite's current world rectangle. Call after the sprite's
        // size/transform is finalised (e.g. WallView sets the wall size at spawn).
        public void Rebuild()
        {
            EnsureLine();
            UpdateCorners();
        }

        private void EnsureLine()
        {
            if (lineGo != null) return;
            if (targetSprite != null) targetSprite.enabled = false; // hollow: hide the filled sprite

            lineGo = new GameObject("NeonOutline"); // root (scale 1) -> width never stretches
            lr = lineGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 4;
            lr.widthMultiplier = width;
            lr.numCornerVertices = 0;
            lr.numCapVertices = 0;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lr.sharedMaterial = outlineMaterial;
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.sortingOrder = 1;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void LateUpdate()
        {
            if (follow) UpdateCorners();
        }

        private void UpdateCorners()
        {
            if (targetSprite == null || lr == null) return;

            // Local half-size (Tiled/Sliced use size, Simple uses the sprite bounds).
            Vector2 half = targetSprite.drawMode == SpriteDrawMode.Simple
                ? (Vector2)targetSprite.sprite.bounds.extents
                : targetSprite.size * 0.5f;

            Transform t = targetSprite.transform;
            Vector3 scale = t.lossyScale;
            Vector3 right = t.right * (half.x * scale.x);
            Vector3 up = t.up * (half.y * scale.y);
            Vector3 c = t.position;

            lr.SetPosition(0, c - right - up);
            lr.SetPosition(1, c + right - up);
            lr.SetPosition(2, c + right + up);
            lr.SetPosition(3, c - right + up);
        }

        private void OnDestroy()
        {
            if (lineGo != null) Destroy(lineGo);
        }
    }
}
