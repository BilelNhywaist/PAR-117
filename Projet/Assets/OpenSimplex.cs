using System;
using System.Runtime.CompilerServices;

public class OpenSimplexNoise
{
    private const double STRETCH_CONSTANT_3D = -1.0 / 6.0;
    private const double SQUISH_CONSTANT_3D = 1.0 / 3.0;
    private const double NORM_CONSTANT_3D = 103.0;

    private short[] perm;
    private short[] permGradIndex3D;

    public OpenSimplexNoise() : this(DateTime.Now.Ticks) { }

    public OpenSimplexNoise(long seed)
    {
        perm = new short[256];
        permGradIndex3D = new short[256];
        short[] source = new short[256];
        for (short i = 0; i < 256; i++)
        {
            source[i] = i;
        }
        seed = seed * 6364136223846793005L + 1442695040888963407L;
        seed = seed * 6364136223846793005L + 1442695040888963407L;
        seed = seed * 6364136223846793005L + 1442695040888963407L;
        for (int i = 255; i >= 0; i--)
        {
            seed = seed * 6364136223846793005L + 1442695040888963407L;
            int r = (int)((seed + 31) % (i + 1));
            if (r < 0) r += (i + 1);
            perm[i] = source[r];
            permGradIndex3D[i] = (short)((perm[i] % (gradients3D.Length / 3)) * 3);
            source[r] = source[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FastFloor(double x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    public double Evaluate(double x, double y, double z)
    {
        double stretchOffset = (x + y + z) * STRETCH_CONSTANT_3D;
        double xs = x + stretchOffset;
        double ys = y + stretchOffset;
        double zs = z + stretchOffset;

        int xsb = FastFloor(xs);
        int ysb = FastFloor(ys);
        int zsb = FastFloor(zs);

        double squishOffset = (xsb + ysb + zsb) * SQUISH_CONSTANT_3D;
        double xb = xsb + squishOffset;
        double yb = ysb + squishOffset;
        double zb = zsb + squishOffset;

        double x0 = x - xb;
        double y0 = y - yb;
        double z0 = z - zb;

        double dx_ext0, dy_ext0, dz_ext0;
        double dx_ext1, dy_ext1, dz_ext1;
        int xsv_ext0, ysv_ext0, zsv_ext0;
        int xsv_ext1, ysv_ext1, zsv_ext1;

        double value = 0;
        double attn0 = 2 - x0 * x0 - y0 * y0 - z0 * z0;
        if (attn0 > 0)
        {
            attn0 *= attn0;
            value += attn0 * attn0 * Extrapolate(xsb, ysb, zsb, x0, y0, z0);
        }

        return value / NORM_CONSTANT_3D;
    }

    private double Extrapolate(int xsb, int ysb, int zsb, double dx, double dy, double dz)
    {
        int index = permGradIndex3D[(perm[(perm[xsb & 0xFF] + ysb) & 0xFF] + zsb) & 0xFF];
        return gradients3D[index] * dx + gradients3D[index + 1] * dy + gradients3D[index + 2] * dz;
    }

    private static sbyte[] gradients3D = new sbyte[] {
        -11,  4,  4,     -4,  11,  4,    -4,  4,  11,
         11,  4,  4,      4,  11,  4,     4,  4,  11,
        -11, -4,  4,     -4, -11,  4,    -4, -4,  11,
         11, -4,  4,      4, -11,  4,     4, -4,  11,
        -11,  4, -4,     -4,  11, -4,    -4,  4, -11,
         11,  4, -4,      4,  11, -4,     4,  4, -11,
        -11, -4, -4,     -4, -11, -4,    -4, -4, -11,
         11, -4, -4,      4, -11, -4,     4, -4, -11
    };
}