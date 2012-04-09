using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace KinectSabre.Render
{
    public class RenderGame : Game
    {
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D colorTexture;
        Cube sabre;
        readonly SoundEffect[] soundEffects = new SoundEffect[4];
        readonly SoundEffectInstance[] soundInstances = new SoundEffectInstance[4];
        int currentSoundIndex = -1;
        readonly Random rnd = new Random();

        readonly BloomComponent bloom;

        private readonly byte[] colorBuffer;

        public RenderGame()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 800,
                PreferredBackBufferHeight = 600,
                GraphicsProfile = GraphicsProfile.HiDef,
            };


            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(30);

            Content.RootDirectory = "Content";

            colorBuffer = new byte[640 * 480 * 4];

            bloom = new BloomComponent(this,1,0,1);


            Components.Add(bloom);
        }

        public Vector3 P1LeftHandPosition { get; set; }
        public Vector3 P1RightHandPosition { get; set; }
        public Vector3 P1LeftElbowPosition { get; set; }
        public Vector3 P1LeftWristPosition { get; set; }
        public Vector3 P1RightElbowPosition { get; set; }
        public Vector3 P1RightWristPosition { get; set; }
        public bool P1IsActive { get; set; }

        public Vector3 P2LeftHandPosition { get; set; }
        public Vector3 P2RightHandPosition { get; set; }
        public Vector3 P2LeftElbowPosition { get; set; }
        public Vector3 P2LeftWristPosition { get; set; }
        public Vector3 P2RightElbowPosition { get; set; }
        public Vector3 P2RightWristPosition { get; set; }
        public bool P2IsActive { get; set; }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            colorTexture = new Texture2D(GraphicsDevice, 640, 480, false, SurfaceFormat.Color);

            sabre = new Cube(GraphicsDevice);

            soundEffects[0] = Content.Load<SoundEffect>("Hum 1");
            soundEffects[1] = Content.Load<SoundEffect>("Hum 2");
            soundEffects[2] = Content.Load<SoundEffect>("Hum 4");
            soundEffects[3] = Content.Load<SoundEffect>("Hum 5");

            for (int index = 0; index < 4; index++)
            {
                soundInstances[index] = soundEffects[index].CreateInstance();
                soundInstances[index].IsLooped = false;
            }
        }

        protected override void UnloadContent()
        {
            colorTexture.Dispose();
            for (int index = 0; index < 4; index++)
            {
                soundInstances[index].Dispose();
                soundEffects[index].Dispose();
            }
        }

        public void UpdateColorTexture(byte[] bits)
        {
            for (int index = 0; index < colorTexture.Width * colorTexture.Height; index++)
            {
                byte b = bits[index * 4];
                byte g = bits[index * 4 + 1];
                byte r = bits[index * 4 + 2];

                colorBuffer[index * 4] = r;
                colorBuffer[index * 4 + 1] = g;
                colorBuffer[index * 4 + 2] = b;
                colorBuffer[index * 4 + 3] = 0;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GraphicsDevice == null)
                return;
            GraphicsDevice.Textures[2] = null;
            colorTexture.SetData(colorBuffer);

            base.Update(gameTime);

            if (currentSoundIndex == -1 || soundInstances[currentSoundIndex].State != SoundState.Playing)
            {
                currentSoundIndex = rnd.Next(4);

                soundInstances[currentSoundIndex].Play();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Color Texture
            GraphicsDevice.BlendState = BlendState.Opaque;
            spriteBatch.Begin();
            spriteBatch.Draw(colorTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            // Sabre
            bloom.ColorTexture = colorTexture;
            bloom.setColor(1, 1, 0);
            bloom.BeginDraw();
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (P1IsActive)
            {
                //bloom.setColor(1, 0, 0);
                //bloom.ColorTexture = colorTexture;
                //bloom.BeginDraw();
                //GraphicsDevice.Clear(Color.Black);
                //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                DrawSabre(P1RightHandPosition, P1RightWristPosition, P1RightElbowPosition);
            }
            if (P2IsActive)
            {
                //bloom.setColor(0, 1, 0);
                //bloom.ColorTexture = colorTexture;
                //bloom.BeginDraw();
                //GraphicsDevice.Clear(Color.Black);
                //GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                DrawSabre(P2RightHandPosition, P2RightWristPosition, P2RightElbowPosition);
            }
            // Base
            base.Draw(gameTime);
        }

        void DrawSabre(Vector3 leftPoint, Vector3 middlePoint, Vector3 rightPoint)
        {
            sabre.View = Matrix.Identity;
            sabre.Projection = Matrix.CreatePerspectiveFieldOfView(0.87f, GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000.0f);

            Vector3 axis = Vector3.Normalize((Vector3.Normalize(leftPoint - middlePoint) + Vector3.Normalize(leftPoint - rightPoint) + Vector3.Normalize(middlePoint - rightPoint)) / 3.0f);

            Vector3 rotAxis = Vector3.Cross(Vector3.Up, axis);
            float rotcos = Vector3.Dot(axis, Vector3.Up);

            Matrix rotate = Matrix.CreateFromAxisAngle(rotAxis, (float)Math.Acos(rotcos));

            sabre.World = Matrix.CreateScale(0.01f, 1.3f, 0.01f) * rotate * Matrix.CreateTranslation(leftPoint * new Vector3(1, 1, -1f)) * Matrix.CreateTranslation(0, -0.08f, 0);
            sabre.Draw();
        }
    }
}
