using System;
using UnityEngine;

namespace Automathon.Game
{
    [RequireComponent(typeof(MeshRenderer))]
    public class HealthBarView : MonoBehaviour
    {
        [Header("Taille (unités monde)")]
        [SerializeField] private float width = 1.6f;
        [SerializeField] private float height = 0.28f;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.9f, -0.1f);

        [Header("Chip")]
        [SerializeField] private float chipHold = 0.4f;   // secondes de maintien après un dégât

        [Header("Animation (en ratio/sec)")]
        [SerializeField] private float displaySpeed = 3.0f;
        [SerializeField] private float chipSpeed = 0.8f;
        [SerializeField] private bool hideWhenFull = false;
        [SerializeField] private float fadeSpeed = 6f;

        private Func<float> ratioSource;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock mpb;

        private float display = 1f, chip = 1f, visibility = 1f, prevTarget = 1f, chipHoldTimer;

        private static readonly int FillID = Shader.PropertyToID("_Fill");
        private static readonly int ChipID = Shader.PropertyToID("_FillChip");
        private static readonly int AspectID = Shader.PropertyToID("_Aspect");
        private static readonly int AlphaMulID = Shader.PropertyToID("_AlphaMul");

        public void Bind(Func<float> ratioSource)
        {
            this.ratioSource = ratioSource;
            display = chip = ratioSource != null ? Mathf.Clamp01(ratioSource()) : 1f;

        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mpb = new MaterialPropertyBlock();

            transform.localScale = new Vector3(width, height, 1f);
            transform.localPosition = localOffset;

            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(AspectID, width / height);
            meshRenderer.SetPropertyBlock(mpb);
        }

        private void LateUpdate()
        {
            if (transform.parent != null)
                transform.position = transform.parent.position + localOffset;
            transform.rotation = Quaternion.identity;

            if (ratioSource == null) return;

            float target = Mathf.Clamp01(ratioSource());
            float dt = Time.deltaTime;

            if (target < prevTarget) chipHoldTimer = chipHold;   // nouveau dégât → on relance le maintien
                prevTarget = target;

            display = Mathf.MoveTowards(display, target, displaySpeed * dt);

            if (target > chip)
                chip = target;                                   // soin : pas de chip
            else if (chipHoldTimer > 0f)
                chipHoldTimer -= dt;                             // on tient le chip en place
            else
                chip = Mathf.MoveTowards(chip, display, chipSpeed * dt);
            chip = Mathf.Max(chip, display);

            float targetVis = (hideWhenFull && target >= 0.999f && Mathf.Abs(display - target) < 0.001f) ? 0f : 1f;
            visibility = Mathf.MoveTowards(visibility, targetVis, fadeSpeed * dt);

            meshRenderer.GetPropertyBlock(mpb);
            mpb.SetFloat(FillID, display);
            mpb.SetFloat(ChipID, chip);
            mpb.SetFloat(AlphaMulID, visibility);
            meshRenderer.SetPropertyBlock(mpb);
        }
    }
}