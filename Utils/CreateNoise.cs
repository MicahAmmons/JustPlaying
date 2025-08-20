using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PlayingAround.Utils
{
    public static  class CreateNoise
    {
        private static GraphicsDevice _graphics;
        public static void LoadContent(GraphicsDevice device)
        {
            _graphics = device;
        }
        public static Texture2D GenerateTileableFBM(int size, int octaves = 4, float lacunarity = 2f, float gain = 0.5f, int? seed = null)
        {
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            int grid = 32; // base grid resolution
            float[,] baseGrid = new float[grid + 1, grid + 1];
            for (int y = 0; y <= grid; y++)
                for (int x = 0; x <= grid; x++)
                    baseGrid[x, y] = (float)rng.NextDouble();

            float SampleWrapped(float u, float v, float freq)
            {
                float x = (u * freq) % grid;
                float y = (v * freq) % grid;
                if (x < 0) x += grid;
                if (y < 0) y += grid;

                int x0 = (int)Math.Floor(x);
                int y0 = (int)Math.Floor(y);
                int x1 = (x0 + 1) % grid;
                int y1 = (y0 + 1) % grid;

                float tx = x - x0;
                float ty = v - (float)Math.Floor(v); // keep 0..1 for smoothstep below

                // smoothstep curve
                tx = tx * tx * (3 - 2 * tx);
                ty = ty * ty * (3 - 2 * ty);

                float a = baseGrid[x0, y0];
                float b = baseGrid[x1, y0];
                float c = baseGrid[x0, y1];
                float d = baseGrid[x1, y1];

                float ab = MathHelper.Lerp(a, b, tx);
                float cd = MathHelper.Lerp(c, d, tx);
                return MathHelper.Lerp(ab, cd, ty);
            }

            var data = new Color[size * size];
            for (int py = 0; py < size; py++)
            {
                float v = (float)py / size;
                for (int px = 0; px < size; px++)
                {
                    float u = (float)px / size;
                    float amp = 1f;
                    float freq = 1f;
                    float sum = 0f;
                    float norm = 0f;

                    for (int o = 0; o < octaves; o++)
                    {
                        sum += SampleWrapped(u, v, freq) * amp;
                        norm += amp;
                        amp *= gain;
                        freq *= lacunarity;
                    }

                    float val = sum / norm; // 0..1
                    byte b = (byte)(val * 255);
                    data[py * size + px] = new Color((byte)b, (byte)b, (byte)b, (byte)255);
                }
            }

            var tex = new Texture2D(_graphics, size, size);
            tex.SetData(data);
            return tex;
        }

    }
}
