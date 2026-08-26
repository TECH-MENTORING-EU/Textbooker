namespace Booker.Tests;

/// <summary>
/// Minimal valid image payloads (magic bytes plus a little body) shared by the
/// photo-pipeline suites so signatures stay single-sourced.
/// </summary>
internal static class TestImages
{
    internal static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
    internal static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
}
