using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mojipet.UI.Components
{
    // Freehand pixel-painting canvas: drag paints circular brush strokes into a
    // Texture2D shown via RawImage. No stroke recognition/scoring -- this is
    // purely for capturing the player's own handwritten glyph.
    public sealed class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private const int BrushRadius = 6;

        private RectTransform _rectTransform;
        private RawImage _rawImage;
        private Texture2D _texture;
        private Color32[] _clearPixels;
        private Color32 _brushColor;

        public bool HasDrawn { get; private set; }

        public static DrawingCanvas Create(Transform parent, int resolution, Color backgroundColor, Color brushColor)
        {
            var go = new GameObject("DrawingCanvas", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<DrawingCanvas>();
            canvas.Initialize(resolution, backgroundColor, brushColor);
            return canvas;
        }

        private void Initialize(int resolution, Color backgroundColor, Color brushColor)
        {
            _rectTransform = (RectTransform)transform;
            _rawImage = GetComponent<RawImage>();
            _brushColor = brushColor;

            _texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            _clearPixels = new Color32[resolution * resolution];
            var bg = (Color32)backgroundColor;
            for (var i = 0; i < _clearPixels.Length; i++)
            {
                _clearPixels[i] = bg;
            }

            _rawImage.texture = _texture;
            Clear();
        }

        public void Clear()
        {
            _texture.SetPixels32(_clearPixels);
            _texture.Apply();
            HasDrawn = false;
        }

        public Texture2D GetTexture()
        {
            return _texture;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PaintAt(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            PaintAt(eventData);
        }

        private void PaintAt(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var rect = _rectTransform.rect;
            var normalizedX = (localPoint.x - rect.x) / rect.width;
            var normalizedY = (localPoint.y - rect.y) / rect.height;

            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
            {
                return;
            }

            var centerX = Mathf.RoundToInt(normalizedX * _texture.width);
            var centerY = Mathf.RoundToInt(normalizedY * _texture.height);

            PaintBrush(centerX, centerY);
            _texture.Apply();
            HasDrawn = true;
        }

        private void PaintBrush(int centerX, int centerY)
        {
            for (var y = -BrushRadius; y <= BrushRadius; y++)
            {
                for (var x = -BrushRadius; x <= BrushRadius; x++)
                {
                    if (x * x + y * y > BrushRadius * BrushRadius)
                    {
                        continue;
                    }

                    var px = centerX + x;
                    var py = centerY + y;
                    if (px < 0 || px >= _texture.width || py < 0 || py >= _texture.height)
                    {
                        continue;
                    }

                    _texture.SetPixel(px, py, _brushColor);
                }
            }
        }
    }
}
