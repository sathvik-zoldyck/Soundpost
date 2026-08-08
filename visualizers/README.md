# Visualizers

Community render styles for Soundpost's "Sound, seen" view. This is the most fun way to contribute.

A visualizer implements **`IVisualizerRenderer`** — one method that paints a frame from smoothed
audio. The contract lives in
[`src/Soundpost.App/Controls/Visualizers/IVisualizerRenderer.cs`](../src/Soundpost.App/Controls/Visualizers/IVisualizerRenderer.cs),
and the six built-in styles next to it (Ribbon, Spectrum, Radial, Oscilloscope, Cymatics,
Custom Image) are your worked examples — copy the closest one.

```csharp
public sealed class BarsMirrorRenderer : IVisualizerRenderer
{
    public string Name => "Bars Mirror";

    public void Draw(in VizFrame f)
    {
        double w = f.Width, h = f.Height, cy = h / 2;
        int bars = 48;
        double bw = w / bars;
        for (int i = 0; i < bars; i++)
        {
            int b = (int)((double)i / bars * f.Bands.Length);
            double bh = f.Bands[b] * h * 0.45;
            double x = i * bw;
            f.Dc.DrawRectangle(f.Palette.BarBrush, null, new Rect(x, cy - bh, bw - 2, bh * 2));
        }
    }
}
```

## Contribute one

1. Add a file here or under `Controls/Visualizers/` — e.g. `YourStyle.cs`.
2. Implement `IVisualizerRenderer`. Give it a short, clear `Name` — it becomes the style pill.
3. Register it: add `new YourStyleRenderer()` to the `_renderers` array in
   [`Visualizer.cs`](../src/Soundpost.App/Controls/Visualizer.cs). It then appears in the style bar
   automatically — no view changes.
4. Use `frame.Palette` (ready-made pens/brushes, or `.Colors`) so it respects the user's chosen
   colours. Use `VizAudio` for shaped energy (`Energy`, `BandAt`, `BandAvg`, `ArgMax`).
5. Keep `Draw` allocation-light — it runs every frame. The built-ins cache palette-derived pens on
   `frame.Palette.Version` and reuse per-frame scratch buffers; do the same if you allocate.
6. Draws a user image? Also implement the empty `IRequiresImage` marker — the view then offers a
   picker + drop zone and hands you the image on `frame.Image`.
7. Open a PR with a short **GIF** of it reacting to music. That GIF is what sells it.

> Runtime loading of external visualizer DLLs is a later milestone ([Plugin SDK](../PLUGIN_SDK.md));
> today a new style is a class compiled into the app and registered in that one array.

## Ideas wanted

Particle field, radial burst, Lissajous, waveform tunnel, VU-needle, spectrogram, starfield.
Claim one in an issue so we don't double up.
