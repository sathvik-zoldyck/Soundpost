# Themes

Reskin the Soundpost console. A theme is a WPF `ResourceDictionary` that overrides the design tokens
(colors, and optionally type) — see the token list in the
[Style Guide](../STYLE_GUIDE.md#part-2--design).

## Contribute one

1. Add `themes/YourTheme.xaml` overriding the brush/token resources.
2. Keep it coherent with the console's *shape* language — you're changing the palette, not the
   physics. One accent, used as light.
3. Open a PR with a **screenshot** of the Mixer and the Visualizer under your theme.

Light themes are welcome too, as long as the meters and accent still read clearly.
