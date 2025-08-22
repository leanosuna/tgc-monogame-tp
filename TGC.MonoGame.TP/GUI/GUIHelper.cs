using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace TGC.MonoGame.TP.GUI
{
    /// <summary>
    /// Handles ImGui rendering and font management for the game, providing helpful
    /// font loading/selecting, plain text and a simple button abstractions
    /// </summary>
    internal static class GUIHelper
    {
        private static ImGuiRenderer _controller;
        private static Dictionary<int, ImFontPtr> _fonts = new Dictionary<int, ImFontPtr>();
        private static int _genButtonId = 0;

        const string defaultFontPath = "Content/Fonts/CascadiaMono.ttf";
        public static void Init(TGCGame game)
        {
            _controller = new ImGuiRenderer(game);

            LoadDefaultFont();
        }

        /// <summary>
        /// Sets up ImGui for a new frame, must be called before any drawing calls
        /// WARNING: Required as first draw step
        /// </summary>
        public static void StartFrame(GameTime gt)
        {
            _genButtonId = 0;

            var io = ImGui.GetIO();
            io.DeltaTime = (float)gt.ElapsedGameTime.TotalSeconds;
            _controller.UpdateInput();
            ImGui.NewFrame();
        }

        /// <summary>
        /// Draws ImGui elements to the current target 
        /// WARNING: Required as last draw step
        /// </summary>}
        public static unsafe void RenderFrame()
        {
            ImGui.Render();
            _controller.RenderDrawData(ImGui.GetDrawData());
        }

        /// <summary>
        /// Loads/restores the default font
        /// </summary>
        public static void LoadDefaultFont()
        {
            LoadCustomFont(defaultFontPath, 15, 100, 5);
        }

        /// <summary>
        /// Loads a font from a ttf file with specified sizes, adds them to the internal 
        /// dictionary and recreates the font atlas
        /// </summary>
        public static void LoadCustomFont(string path, int from, int to, int step)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path), "Font path cannot be null or empty.");

            if (from < 1 || to < from || step < 1)
                throw new ArgumentException($"invalid arguments {from}->{to} step {step}");

            List<int> sizes = new List<int>();
            for (int i = from; i <= to; i += step)
                sizes.Add(i);

            LoadCustomFont(path, sizes.ToArray());
        }

        /// <summary>
        /// Loads a font from a ttf file with specified sizes, adds them to the internal 
        /// dictionary and recreates the font atlas
        /// </summary>
        public static void LoadCustomFont(string path, int[] sizes)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path), "Font path cannot be null or empty.");

            if (sizes == null || sizes.Length == 0)
                throw new ArgumentException("Sizes array cannot be null or empty.");

            var io = ImGui.GetIO();

            _fonts.Clear();

            foreach (var size in sizes)
                _fonts[size] = io.Fonts.AddFontFromFileTTF(path, size);

            _controller.RebuildFontAtlas();

        }

        /// <summary>
        /// Pushes a font of specified size onto the ImGui stack.
        /// WARNING: Must be called with a corresponding PopFont() after drawing
        /// </summary>
        public static void PushFont(int size)
        {
            if (size == 0)
            {
                ImGui.PushFont(_fonts.First().Value);
                return;
            }

            if (!_fonts.TryGetValue(size, out var font))
                throw new Exception($"font size {size} not found");

            ImGui.PushFont(font);

        }

        /// <summary>
        /// Pops the current font from the ImGui stack
        /// WARNING: Must be called after a PushFont() to avoid exceptions.
        /// </summary>
        public static void PopFont()
        {
            ImGui.PopFont();
        }

        /// <summary>
        /// Draws text at a specified position with a specified color and smallest size.
        /// </summary>
        public static void DrawText(string text, Vector2 position, Vector4 color)
        {
            PushFont(0);
            var drawList = ImGui.GetForegroundDrawList();
            drawList.AddText(position.ToNumerics(), ImGui.ColorConvertFloat4ToU32(color.ToNumerics()), text);
            PopFont();

        }

        /// <summary>
        /// Draws text at a specified position with a specified color and size.
        /// </summary>
        public static void DrawText(string text, Vector2 position, Vector4 color, int fontSize)
        {
            PushFont(fontSize);
            var drawList = ImGui.GetForegroundDrawList();
            drawList.AddText(position.ToNumerics(), ImGui.ColorConvertFloat4ToU32(color.ToNumerics()), text);
            PopFont();

        }

        /// <summary>
        /// Draws a button within a small imgui window at a specified position and size.
        /// </summary>
        public static void DrawButton(string name, Vector2 position, Vector2 size, Action action)
        {
            DrawButton(name, position, size, action, 0);
        }

        /// <summary>
        /// Draws a button within a small imgui window at a specified position, size and font size
        /// </summary>
        public static void DrawButton(string name, Vector2 position, Vector2 size, Action action, int fontSize)
        {
            PushFont(fontSize);
            var guiName = $"btn_{_genButtonId}";
            ImGui.SetNextWindowPos(position.ToNumerics());

            ImGui.Begin(guiName, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse);
            if (ImGui.Button(name))
                action.Invoke();
            ImGui.End();

            _genButtonId++;
            PopFont();
        }


    }
}
