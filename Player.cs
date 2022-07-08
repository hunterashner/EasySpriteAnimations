using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EasySpriteAnimations
{
    class Player : DrawableGameComponent, ICollidable
    {
        private Game _game;
        private SpriteBatch _spriteBatch;
        private Vector2 _position;
        private float _walkingSpeed;
        private SpriteAtlas _atlas;
        public enum PlayerAnimationState : int
        {
            IdleDown,
            IdleUp,
            IdleLeft,
            IdleRight,
            WalkDown,
            WalkUp,
            WalkLeft,
            WalkRight,
            LastIdle,
        }

        public PlayerAnimationState PlayerState { get; set; }
        public PlayerAnimationState LastAnimationState { get; set; }

        public bool IsColliding { get; set; }
        public Rectangle BoxCollider { get; set; }

        public static event Action OnCollisionEnter;
        public static event Action OnCOllisionExit;


        public Vector2 Position { get { return this._position; } }

        Rectangle[] _spriteRectangles;
        Texture2D[] _sprites;
        Texture2D _currentSprite;
        int _frameCounter;
        int _frameSelectCount;
        int _framesBetweenRedraw;
        int _framesPerAnimation;
        int _lastStartingFrame;

        public Player(Game game, SpriteBatch spriteBatch, SpriteAtlas atlas) : base(game)
        {
            this._game = game;
            this._spriteBatch = spriteBatch;
            this._atlas = atlas;

            _frameCounter = 0;
            _frameSelectCount = 0;
            _framesBetweenRedraw = 10;
            _framesPerAnimation = 8;
            _lastStartingFrame = 0;

            GenerateAnimationRectangles();
            GenerateSpriteTextures();

            this._position = Vector2.Zero;
            this._walkingSpeed = .1f;
            this.LastAnimationState = PlayerAnimationState.IdleDown;
        }

        public void CheckForCollisions()
        {

        }

        private void GenerateAnimationRectangles()
        {
            int totalCells = this._atlas.Width * this._atlas.Height;
            _spriteRectangles = new Rectangle[totalCells];

            int startingX = 0;
            int startingY = 0;
            int widthX = this._atlas.Width * this._atlas.CellSize.X;
            int heightY = this._atlas.Height * this._atlas.CellSize.Y;

            for(int i = 0; i < _spriteRectangles.Length; i++)
            {
                Point point = new Point(startingX, startingY);
                Rectangle rectangle = new Rectangle(point, this._atlas.CellSize);
                _spriteRectangles[i] = rectangle;
                startingX += this._atlas.CellSize.X;
                if(startingX == widthX)
                {
                    startingY += this._atlas.CellSize.Y;
                    startingX = 0;
                }
            }
        }

        private void GenerateSpriteTextures()
        {
            _sprites = new Texture2D[this._atlas.Width * this._atlas.Height];
            Color[] spritePixels = new Color[this._atlas.CellSize.X * this._atlas.CellSize.Y];
            for(int i = 0; i < _sprites.Length; i++)
            {
                this._atlas.Atlas.GetData<Color>(0, 0, _spriteRectangles[i], spritePixels, 0, spritePixels.Length);
                Texture2D texture = new Texture2D(_spriteBatch.GraphicsDevice, this._atlas.CellSize.X, this._atlas.CellSize.Y);
                texture.SetData<Color>(spritePixels);
                _sprites[i] = texture;
            }
        }

        private void PlayDesiredAnimation(int startingFrame, int endingFrame)
        {
            _frameCounter++;

            if(startingFrame != _lastStartingFrame)
            {
                _frameSelectCount = startingFrame;
            }

            if(_frameCounter == _framesBetweenRedraw)
            {
                _frameSelectCount++;
                _frameCounter = 0;
            }

            if(_frameSelectCount == endingFrame)
            {
                _frameSelectCount = startingFrame;
            }

            _currentSprite = _sprites[_frameSelectCount];
            _lastStartingFrame = startingFrame;
        }

        private void HandleAnimation(PlayerAnimationState state)
        {
            switch (state)
            {
                case PlayerAnimationState.WalkDown:
                    PlayDesiredAnimation(32, 39);
                    LastAnimationState = PlayerAnimationState.IdleDown;
                    break;

                case PlayerAnimationState.WalkUp:
                    PlayDesiredAnimation(40, 47);
                    LastAnimationState = PlayerAnimationState.IdleUp;

                    break;

                case PlayerAnimationState.WalkLeft:
                    PlayDesiredAnimation(48, 55);
                    LastAnimationState = PlayerAnimationState.IdleRight;
                    break;

                case PlayerAnimationState.WalkRight:
                    PlayDesiredAnimation(56, 63);
                    LastAnimationState = PlayerAnimationState.IdleLeft;
                    break;

                //idle animation based on last known movement direction
                case PlayerAnimationState.LastIdle:
                    switch (LastAnimationState)
                    {
                        case PlayerAnimationState.IdleUp:
                            PlayDesiredAnimation(8, 15);
                            break;

                        case PlayerAnimationState.IdleDown:
                            PlayDesiredAnimation(0, 7);
                            break;

                        case PlayerAnimationState.IdleLeft:
                            PlayDesiredAnimation(16, 23);
                            break;

                        case PlayerAnimationState.IdleRight:
                            PlayDesiredAnimation(24, 31);
                            break;
                    }

                    break;
            }
        }

        public void HandleMovement(Vector2 direction, GameTime gameTime)
        {
            //Normalize will throw exception if direction = 0
            if (direction == Vector2.Zero) { return; }
            direction.Normalize();
            this._position += direction * (float)gameTime.ElapsedGameTime.TotalMilliseconds * _walkingSpeed;
        }

        public void CustomUpdate(Vector2 direction, GameTime gameTime)
        {
            HandleMovement(direction, gameTime);
            Update(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            HandleAnimation(PlayerState);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Draw(_currentSprite, _position, Color.White);
            base.Draw(gameTime);
        }

    }
}
