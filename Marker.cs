using UnityEngine;
using System.Collections;

namespace CubePainter
{
    public class Marker : MonoBehaviour
    {
        public enum OnMarkAction { Disable, ScaleDown, BounceScaleDown, BounceAndDisable, Nothing }

        public Color color = Color.red;
        [Range(0f, .5f)]
        public float lightnessVariation = 0;
        public int smudgeRadius = 1;
        [Range(0f, 1f)]
        public float lerp = 1f;
        public int bounces = 1;
        public float scaleDownRate = .1f;
        public OnMarkAction onMarkAction = OnMarkAction.Nothing;
        public bool useGradientColors;
        public bool startWithRandomHue;
        public float gradientStep = .1f;

        int _bounceStart;
        float _startV;
        protected Vector3 _startScale;
        protected Transform _cachedTransform;
        protected Collider _collider;
        public bool placeSmudge = false;

        void Awake()
        {
            _bounceStart = bounces;
            _startScale = transform.localScale;
            _cachedTransform = transform;
            _collider = GetComponent<Collider>();
            if (startWithRandomHue)
                SetRandomHue();
            float h, s;
            Color.RGBToHSV(color, out h, out s, out _startV);
        }

        public void SetRandomHue()
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h = Random.value;
            color = Color.HSVToRGB(h, s, v);
        }

        public virtual void Mark(Decal decal, Vector3 point)
        {
            if (useGradientColors)
            {
                color = Decal.GetNextGradient();
            }
            if (lightnessVariation > 0)
            {
                float h, s, v;
                Color.RGBToHSV(color, out h, out s, out v);
                color = Color.HSVToRGB(h, s, _startV - Random.Range(0, lightnessVariation));
            }
            if (placeSmudge)
                decal.PlaceSmudge(point, color, lerp);
            else decal.PlacePixel(point, color, lerp);
            switch (onMarkAction)
            {
                case OnMarkAction.Disable:
                    gameObject.SetActive(false);
                    break;
                case OnMarkAction.ScaleDown:
                    if (_cachedTransform.localScale.x <= 0)
                        goto case OnMarkAction.Disable;
                    _cachedTransform.localScale = Vector3.MoveTowards(_cachedTransform.localScale, Vector3.zero, scaleDownRate);
                    break;
                case OnMarkAction.BounceScaleDown:
                    if (_cachedTransform.localScale.x <= 0)
                        goto case OnMarkAction.Disable;
                    _cachedTransform.localScale = Vector3.MoveTowards(_startScale, Vector3.zero, 1f - ((float) bounces / _bounceStart));
                    bounces--;
                    break;
                case OnMarkAction.BounceAndDisable:
                    if (bounces <= 0)
                        goto case OnMarkAction.Disable;
                    bounces--;
                    break;
            }

        }
    }

}
