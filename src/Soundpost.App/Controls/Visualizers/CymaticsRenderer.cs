using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Soundpost.App.Controls.Visualizers;

/// <summary>
/// Cymatic (Chladni) sand plate: the nodal lines of a vibrating square plate. The mode numbers (n, m)
/// are driven by where the music's energy sits; louder audio thickens the sand.
/// </summary>
public sealed class CymaticsRenderer : IVisualizerRenderer
{
    public string Name => "Cymatics";

    private const int S = 200;

    private WriteableBitmap? _bmp;
    private byte[]? _pixels;
    private double[]? _grain;
    private double[]? _cosNx, _cosMx, _cosMy, _cosNy;
    private double _n = 3, _m = 2;

    public void Draw(in VizFrame frame)
    {
        float[] bands = frame.Bands;

        if (_bmp is null)
        {
            _bmp = new WriteableBitmap(S, S, 96, 96, PixelFormats.Bgra32, null);
            _pixels = new byte[S * S * 4];
            _cosNx = new double[S];
            _cosMx = new double[S];
            _cosMy = new double[S];
            _cosNy = new double[S];
            _grain = new double[S * S];
            var rnd = new Random(1234);
            for (int k = 0; k < _grain.Length; k++)
            {
                _grain[k] = 0.45 + (rnd.NextDouble() * 0.55);
            }
        }

        int half = bands.Length / 2;
        double nTarget = 2 + (VizAudio.ArgMax(bands, 0, half) / (double)Math.Max(1, half) * 5.0);
        double mTarget = 2 + ((VizAudio.ArgMax(bands, half, bands.Length) - half) / (double)Math.Max(1, bands.Length - half) * 5.0);
        _n += (nTarget - _n) * 0.05;
        _m += (mTarget - _m) * 0.05;

        double energy = VizAudio.Energy(bands);
        double eps = 0.03 + (energy * 0.16 * (0.4 + frame.Sensitivity));

        for (int i = 0; i < S; i++)
        {
            double x = i / (double)(S - 1);
            _cosNx![i] = Math.Cos(_n * Math.PI * x);
            _cosMx![i] = Math.Cos(_m * Math.PI * x);
        }

        for (int j = 0; j < S; j++)
        {
            double y = j / (double)(S - 1);
            _cosMy![j] = Math.Cos(_m * Math.PI * y);
            _cosNy![j] = Math.Cos(_n * Math.PI * y);
        }

        byte[] px = _pixels!;
        int stride = S * 4;
        for (int j = 0; j < S; j++)
        {
            double cmy = _cosMy![j], cny = _cosNy![j];
            int row = j * stride;
            int grow = j * S;
            for (int i = 0; i < S; i++)
            {
                double f = (_cosNx![i] * cmy) - (_cosMx![i] * cny);
                double af = Math.Abs(f);
                double a = af < eps ? 1 - (af / eps) : 0;
                a *= _grain![grow + i];
                int o = row + (i * 4);
                px[o] = (byte)(214 * a);       // B
                px[o + 1] = (byte)(238 * a);   // G
                px[o + 2] = (byte)(255 * a);   // R
                px[o + 3] = 255;               // A
            }
        }

        _bmp.WritePixels(new Int32Rect(0, 0, S, S), px, stride, 0);
        frame.Dc.DrawImage(_bmp, new Rect(0, 0, frame.Width, frame.Height));
    }
}
