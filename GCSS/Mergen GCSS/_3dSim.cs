using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;

namespace Mergen_GCSS
{
    public class _3dSim
    {

        public float x, y, z;

        public void UpdateRotation(float newX, float newY, float newZ)
        {
               x = newX;
                y = newY;
                z = newZ;
            
            // Burada OpenGL işlemlerini güncelleyebilirsiniz
            // Örneğin, glControl'ün yeniden çizilmesini sağlayabilirsiniz
        }
        public void DrawCylinder(float radius, float height, int slices)
        {

            float halfHeight = height / 2.0f;
            float columnInset = 0.15f * radius; // Kolonların merkeze olan mesafesi
            float layerHeight = height / 3.0f; // Her bir tabakanın yüksekliği
            float wallThickness = 0.1f * height; // Üst taban kalınlığı

            // Alt taban
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(1.0f, 0.0f, 0.0f); // Kırmızı renk
            GL.Normal3(0, -1, 0);
            GL.Vertex3(0, -halfHeight, 0);
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, -halfHeight, z);
            }
            GL.End();

            // Ortadaki iki tabaka
            for (int j = 1; j <= 2; j++)
            {
                float currentHeight = -halfHeight + j * layerHeight;
                GL.Begin(PrimitiveType.TriangleFan);
                // Her tabakayı farklı renkte çiz
                switch (j)
                {
                    case 1:
                        GL.Color3(0.5f, 0.5f, 0.0f); // Sarı renk
                        break;
                    case 2:
                        GL.Color3(0.5f, 0.0f, 0.5f); // Mor renk
                        break;
                }
                GL.Normal3(0, 0, 0);
                GL.Vertex3(0, currentHeight, 0);
                for (int i = 0; i <= slices; i++)
                {
                    float angle = i * 2.0f * (float)Math.PI / slices;
                    float x = (float)Math.Cos(angle) * radius;
                    float z = (float)Math.Sin(angle) * radius;
                    GL.Vertex3(x, currentHeight, z);
                }
                GL.End();
            }

            // Üst tabanın duvarları (aynı genişlikte iniyor)
            GL.Begin(PrimitiveType.QuadStrip);
            GL.Color3(0.0f, 0.5f, 1.0f); // Açık mavi renk
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float xOuter = (float)Math.Cos(angle) * radius;
                float zOuter = (float)Math.Sin(angle) * radius;
                float xInner = xOuter;
                float zInner = zOuter;

                GL.Normal3(xOuter, 0, zOuter);
                GL.Vertex3(xOuter, halfHeight, zOuter);
                GL.Vertex3(xInner, halfHeight - wallThickness, zInner);
            }
            GL.End();

            // Üst tabanın alt kısmındaki tabaka
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Color3(0.0f, 0.0f, 1.0f); // Mavi renk
            GL.Normal3(0, -1, 0);
            GL.Vertex3(0, halfHeight - wallThickness, 0);
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float x = (float)Math.Cos(angle) * radius;
                float z = (float)Math.Sin(angle) * radius;
                GL.Vertex3(x, halfHeight - wallThickness, z);
            }
            GL.End();

            // 4 Kolon
            GL.Color3(0.0f, 1.0f, 0.0f); // Yeşil renk
            float columnRadius = 0.1f * radius; // Kolon yarıçapı

            for (int i = 0; i < 4; i++)
            {
                float angle = i * (float)Math.PI / 2;
                float x = (float)Math.Cos(angle) * (radius - columnInset);
                float z = (float)Math.Sin(angle) * (radius - columnInset);

                DrawColumn(x, z, columnRadius, height);
            }
        }

        public void DrawColumn(float x, float z, float columnRadius, float height)
        {
            float halfHeight = height / 2.0f;
            int slices = 16; // Kolon için dilim sayısı

            GL.Begin(PrimitiveType.QuadStrip);
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float dx = (float)Math.Cos(angle) * columnRadius;
                float dz = (float)Math.Sin(angle) * columnRadius;

                GL.Normal3(dx, 0, dz);
                GL.Vertex3(x + dx, -halfHeight, z + dz);
                GL.Vertex3(x + dx, halfHeight, z + dz);
            }
            GL.End();

            // Kolon alt tabanı
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, -1, 0);
            GL.Vertex3(x, -halfHeight, z);
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float dx = (float)Math.Cos(angle) * columnRadius;
                float dz = (float)Math.Sin(angle) * columnRadius;
                GL.Vertex3(x + dx, -halfHeight, z + dz);
            }
            GL.End();

            // Kolon üst tabanı
            GL.Begin(PrimitiveType.TriangleFan);
            GL.Normal3(0, 1, 0);
            GL.Vertex3(x, halfHeight, z);
            for (int i = 0; i <= slices; i++)
            {
                float angle = i * 2.0f * (float)Math.PI / slices;
                float dx = (float)Math.Cos(angle) * columnRadius;
                float dz = (float)Math.Sin(angle) * columnRadius;
                GL.Vertex3(x + dx, halfHeight, z + dz);
            }
            GL.End();
        }
    }
}

