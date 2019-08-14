﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gym.Threading;
using NGraphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point = NGraphics.Point;
using Size = System.Drawing.Size;

namespace Ebby.Gym.Rendering {
    /// <summary>
    ///     A form with PictureBox that accepts <see cref="IImageCanvas"/> and renders it on it. Start <see cref="Viewer"/> by calling <see cref="Run"/>
    /// </summary>
    public partial class Viewer : Form {
        private int _lastSize = 0;
        private ManualResetEventSlim _ready = new ManualResetEventSlim();

        /// <summary>
        ///     Starts a <see cref="Viewer"/> in seperate thread.
        /// </summary>
        /// <param name="height">The height of the form</param>
        /// <param name="width">The width of the form</param>
        /// <param name="title">The title of the form, also mentioned in the thread name.</param>
        public static Viewer Run(int width, int height, string title = null) {
            Viewer v = null;
            using (var me = new ManualResetEventSlim()) {
                var thread = new Thread(() => {
                    v = new Viewer(width + 12, height + 12, title);
                    me.Set();
                    v.ShowDialog();
                });
                thread.Start();
                thread.Name = $"Viewer{(string.IsNullOrEmpty(title) ? "" : $"-{title}")}";

                if (!me.Wait(10_000))
                    throw new Exception("Starting viewer timed out.");
            }

            Debug.Assert(v != null, "At this point viewer shouldn't be null.");

            return v;
        }

        public Viewer(int width, int height, string title = null) {
            InitializeComponent();
            Height = height;
            Width = width;
            if (title != null)
                this.Text = title;
        }

        /// <summary>
        ///     Renders this canvas onto <see cref="PictureFrame"/>.
        /// </summary>
        /// <param name="canvas">Canvas painted from <see cref="NGraphics"/></param>
        public void Render(Image<Rgba32> canvas) {
            if (InvokeRequired) {
                Invoke(new Action(() => Render(canvas)));
                return;
            }

            using (var ms = new MemoryStream(_lastSize)) {
                canvas.SaveAsBmp(ms);
                _lastSize = (int) ms.Length;
                Clear();
                PictureFrame.Image = new Bitmap(ms);
            }
        }

        /// <summary>
        ///     Renders this canvas onto <see cref="PictureFrame"/>.
        /// </summary>
        /// <param name="canvas">Canvas painted from <see cref="NGraphics"/></param>
        public void RenderAsync(Image<Rgba32> canvas) {
            if (InvokeRequired) {
                BeginInvoke(new Action(() => RenderAsync(canvas)));
                return;
            }

            using (var ms = new MemoryStream(_lastSize)) {
                canvas.SaveAsBmp(ms);
                _lastSize = (int) ms.Length;
                Clear();
                PictureFrame.Image = new Bitmap(ms);
            }
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Shown" /> event.</summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data. </param>
        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            _ready.Set();
        }

        public void Clear() {
            if (PictureFrame.Image != null) {
                var img = PictureFrame.Image;
                PictureFrame.Image = null;
                img.Dispose();
            }
        }

        /// <summary>
        ///     Performs a rendering test.
        /// </summary>
        /// <param name="offset"></param>
        public void TestRendering(int offset = 0) {
            //IImageCanvas canvas = Platforms.Current.CreateImageCanvas(new NGraphics.Size(Height, Width), scale: 1);
            //var skyBrush = new LinearGradientBrush(Point.Zero, Point.OneY, Colors.Blue, Colors.White);
            //canvas.FillRectangle(new Rect(canvas.Size), skyBrush);
            //canvas.FillEllipse(10, 10, 30 + offset, 30 + offset, Colors.Yellow);
            //canvas.FillRectangle(50, 60, 60, 40, Colors.LightGray);
            //canvas.FillPath(new PathOp[] {
            //    new MoveTo(40, 60),
            //    new LineTo(120, 60),
            //    new LineTo(80, 30),
            //    new ClosePath()
            //}, Colors.Gray);
            //
            //Render(canvas);
        }


        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            PictureFrame.Image.TryDispose();
            _ready.TryDispose();
        }
    }
}