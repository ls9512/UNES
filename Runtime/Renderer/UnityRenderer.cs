using System.Runtime.InteropServices;
using UnityEngine;

namespace Aya.UNes.Renderer
{
    public class UnityRenderer : IRenderer
    {
        public UNes UNes;
        public RenderTexture RenderTexture;

        public string RendererName => "Unity";

        private Texture2D _drawTexture;
        private Color[] _pixelCache;

        public void Draw()
        {
            if (RenderTexture.filterMode != UNes.FilterMode)
            {
                RenderTexture.filterMode = UNes.FilterMode;
            }

            for (var y = 0; y < UNes.GameHeight; y++)
            {
                for (var x = 0; x < UNes.GameWidth; x++)
                {
                    var rawIndex = UNes.GameWidth * y + x;
                    var color = GetColor(UNes.RawBitmap[rawIndex]);
                    var texIndex = UNes.GameWidth * (UNes.GameHeight - y - 1) + x;
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

        public void InitRendering(UNes nes)
        {
            UNes = nes;
            RenderTexture = nes.RenderTexture;
            _drawTexture = new Texture2D(UNes.GameWidth, UNes.GameHeight);
            _pixelCache = new Color[UNes.GameWidth * UNes.GameHeight];
        }

        public void EndRendering()
        {
            
        }
    }
}

