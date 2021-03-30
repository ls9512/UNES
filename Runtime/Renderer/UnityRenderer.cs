using System.Runtime.InteropServices;
using UnityEngine;

namespace Aya.UNES.Renderer
{
    public class UnityRenderer : IRenderer
    {
        public UNESBehaviour UnesBehaviour;
        public RenderTexture RenderTexture;

        public string Name => "Unity";

        private Texture2D _drawTexture;
        private Color[] _pixelCache;

        private Texture2D _clearTexture;

        public void Init(UNESBehaviour nes)
        {
            UnesBehaviour = nes;
            RenderTexture = nes.RenderTexture;
            _drawTexture = new Texture2D(UNESBehaviour.GameWidth, UNESBehaviour.GameHeight);
            _pixelCache = new Color[UNESBehaviour.GameWidth * UNESBehaviour.GameHeight];

            _clearTexture = new Texture2D(1, 1);
            _clearTexture.SetPixel(0, 0, Color.clear);
            _clearTexture.Apply();
        }

        public void HandleRender()
        {
            if (RenderTexture.filterMode != UnesBehaviour.FilterMode)
            {
                RenderTexture.filterMode = UnesBehaviour.FilterMode;
            }

            for (var y = 0; y < UNESBehaviour.GameHeight; y++)
            {
                for (var x = 0; x < UNESBehaviour.GameWidth; x++)
                {
                    var rawIndex = UNESBehaviour.GameWidth * y + x;
                    var color = GetColor(UnesBehaviour.RawBitmap[rawIndex]);
                    var texIndex = UNESBehaviour.GameWidth * (UNESBehaviour.GameHeight - y - 1) + x;
                    _pixelCache[texIndex] = color;
                }
            }

            _drawTexture.SetPixels(_pixelCache);
            _drawTexture.Apply();

            Graphics.Blit(_drawTexture, RenderTexture);
        }

        public Color GetColor(uint value)
        {
            var r = 0xFF0000 & value;
            r >>= 16;
            var b = 0xFF & value;
            var g = 0xFF00 & value;
            g >>= 8;
            var color = new Color(r / 255f, g / 255f, b / 255f);
            return color;
        }

        public void End()
        {
            Graphics.Blit(_clearTexture, RenderTexture);
        }
    }
}

