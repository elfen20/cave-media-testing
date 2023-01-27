using System.Drawing;
using Cave;
using Cave.Media;
using Cave.Media.OpenGL;
using Cave.Media.Video;

namespace openglfw
{
    class Program
    {
        const string dllbasepath = @"c:\dll\glfw\";
        const string assetbasepath = @"..\..\..\..\assets\bitmap";

        IBitmap32Loader bmLoader;
        Glfw3Renderer renderer;
        bool closing = false;
        long counter = 0;
        long millis = 0;
        DateTime startTime = DateTime.MinValue;
        IStopWatch? watch;
        List<IRenderSprite> sprites = new List<IRenderSprite>();

        float zoom;
        Vector2 translation = Vector2.Empty;
        bool dragging;
        Vector2 translationStart = Vector2.Empty;
        Vector2 translationTemp = Vector2.Empty;

        Vector2 cursorPos = Vector2.Empty;


        static void Main(string[] args)
        {
            var dllpath = dllbasepath + ((IntPtr.Size == 4) ? "32" : "64");
            glfw3.ConfigureNativesDirectory(dllpath);
            var p = new Program();
            p.Run();
        }

        public Program()
        {
            bmLoader = new SkiaBitmap32Loader();
            renderer = new Glfw3Renderer();
        }

        private void CheckTimer()
        {
            if (watch != null)
            {
                if (watch.ElapsedMilliSeconds > 100)
                {
                    millis = watch.ElapsedMilliSeconds;
                    renderer?.SetWindowTitle($"FPS: {(float)counter * 1000 / millis:f} / Z: {zoom} / {cursorPos} / {renderer.WorldTranslation}");
                    watch.Reset();
                    counter = 0;
                }
            }
            else
            {
                watch = StopWatch.StartNew();
            }
        }

        private void Run()
        {
            Init();
            LoadSprites();
            while (!closing)
            {
                Render();
                CheckTimer();
                if (Console.KeyAvailable) closing = true;
            }
        }

        private void Render()
        {
            if (renderer == null) throw new Exception("could not render");
            counter++;

            renderer.Clear(Color.DarkBlue);
            renderer.Render(sprites);
            renderer.Present();
        }

        private void Init()
        {
            if (!renderer.IsAvailable) throw new Exception("could not init");
            var rdevs = renderer.GetDevices();
            renderer.AspectCorrection = ResizeMode.TouchFromInside;
            renderer.Closed += Renderer_Closed;
            renderer.ScrollEvent += Renderer_ScrollEvent;
            renderer.MouseButtonChanged += Renderer_MouseButtonChanged;
            renderer.CursorPosChanged += Renderer_CursorPosChanged;

            renderer.Initialize(rdevs[0], RendererMode.Window, RendererFlags.WaitRetrace, 1024, 768, "OpenGL");
            Console.WriteLine("Max Texture size: " + renderer.MaxTextureSize);
        }

        private void Renderer_CursorPosChanged(object? sender, glfw3.CursorPosEventArgs e)
        {

            if (dragging)
            {
                translationTemp = CalcProjectionOffset(e.Position) - translationStart;
                CalcProjection();
            }

            cursorPos = renderer.CalculateProjectionCoordinates(e.Position);

        }

        private void Renderer_MouseButtonChanged(object? sender, glfw3.MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case glfw3.MouseButton.Button1:
                    switch (e.State)
                    {
                        case glfw3.InputState.Press:
                            translationStart = CalcProjectionOffset(e.Position);
                            dragging = true;
                            break;
                        case glfw3.InputState.Release:
                            dragging = false;
                            translation += translationTemp;
                            translationTemp = Vector2.Empty;                            
                            break;
                    }
                    break;
            }
        }

        private Vector2 CalcProjectionOffset(Vector2 coords)
        {
            return renderer.CalculateProjectionCoordinates(coords) + renderer.WorldTranslation;
        }

        private void CalcProjection()
        {
            IRenderSprite sprite = sprites[1];
            var scale = (float)Math.Pow(2d, zoom / 5);
            renderer.WorldScale = Vector2.Create(scale, scale);
            var tv = translation + translationTemp;
            renderer.WorldTranslation = tv;
            //sprite!.Position = Vector3.Create(tv.X,tv.Y,0);
        }

        private void Renderer_ScrollEvent(object? sender, glfw3.ScrollEventArgs e)
        {
            zoom += e.Offset.Y;
            CalcProjection();
        }

        private void LoadSprites()
        {
            IRenderSprite sprite;
            
            sprite = renderer.CreateSprite("bg");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath ,"bg.png")));
            sprites.Add(sprite);

            sprite = renderer.CreateSprite("s1");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath , "s1.png")));
            sprites.Add(sprite);

            sprite = renderer.CreateSprite("n1");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath, "n1.png")));
            sprite.Position = Vector3.Create(-1, 1, 0);
            sprites.Add(sprite);

            sprite = renderer.CreateSprite("n2");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath, "n2.png")));
            sprite.Position = Vector3.Create(1, 1, 0);
            sprites.Add(sprite);

            sprite = renderer.CreateSprite("n3");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath, "n3.png")));
            sprite.Position = Vector3.Create(1, -1, 0);
            sprites.Add(sprite);

            sprite = renderer.CreateSprite("n4");
            sprite.LoadTexture(Bitmap32.FromFile(Path.Join(assetbasepath, "n4.png")));
            sprite.Position = Vector3.Create(-1, -1, 0);
            sprites.Add(sprite);


        }

        private void Renderer_Closed(object? sender, EventArgs e)
        {
            closing = true;
        }
    }
}
