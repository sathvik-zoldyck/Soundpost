# Visualizers

Community render styles for Soundpost's "Sound, seen" view. This is the most fun way to contribute.

A visualizer implements `IVisualizerRenderer` — one method that draws a frame from smoothed audio
data (FFT bands + waveform). See the contract and a full example in
[PLUGIN_SDK.md §3](../PLUGIN_SDK.md#3-visualizer-plugins).

## Contribute one

1. Add a file here: `visualizers/YourStyle.cs` (or a small project).
2. Implement `IVisualizerRenderer`; give it a clear `Name`.
3. Use the [design palette](../STYLE_GUIDE.md#part-2--design) and the frame's `Palette` so it respects
   the user's chosen colors.
4. Keep it allocation-light — it runs every frame.
5. Open a PR with a short **GIF** of it reacting to music. That GIF is what sells it.

## Ideas wanted

Cymatics (sand/Chladni), custom-image reactive, particle field, radial burst, oscilloscope variants,
Lissajous, waveform tunnel, VU-needle. Claim one in an issue so we don't double up.
