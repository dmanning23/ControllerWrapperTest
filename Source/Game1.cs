using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FontBuddyLib;
using GameTimer;
using HadoukInput;

namespace ControllerWrapperTest
{
	/// <summary>
	/// this dude verifies that all the controller wrapper is wrapping things for the inputwrapper correctly
	/// checks all controllers are being checked correctly
	/// checks the forward/back is being checked correctly
	/// checks that the scrubbed/powercurve is working correctly
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		#region Members

		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		/// <summary>
		/// A font buddy we will use to write out to the screen
		/// </summary>
		private FontBuddy _text = new FontBuddy();

		/// <summary>
		/// THe controller object we gonna use to test
		/// </summary>
		private ControllerWrapper _controller;

		/// <summary>
		/// The timers we are gonna use to time the button down events
		/// </summary>
		private CountdownTimer[] _ButtonTimer;

		private InputState m_Input = new InputState();

		/// <summary>
		/// The controller index of this player
		/// </summary>
		private PlayerIndex _player = PlayerIndex.One;

		private DeadZoneType _thumbstick = DeadZoneType.Axial;

		private bool _flipped = false;

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
			Content.RootDirectory = "Content";
			graphics.PreferredBackBufferWidth = 1024;
			graphics.PreferredBackBufferHeight = 768;
			graphics.IsFullScreen = false;

			_controller = new ControllerWrapper(PlayerIndex.One, true);
			_ButtonTimer = new CountdownTimer[(int)EKeystroke.RTriggerRelease + 1];

			for (int i = 0; i < ((int)EKeystroke.RTriggerRelease + 1); i++)
			{
				_ButtonTimer[i] = new CountdownTimer();
			}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
#if OUYA
			_text.LoadContent(Content, "ArialBlack14");
#else
			_text.LoadContent(Content, "ArialBlack10");
#endif
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
				Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			//Update the controller
			m_Input.Update();
			_controller.Update(m_Input);

			//check if the player is switching controllers
			if (CheckKeyDown(m_Input, Keys.D1))
			{
				_player = PlayerIndex.One;
				_controller = new ControllerWrapper(_player);
				_controller.Thumbsticks.ThumbstickScrubbing = _thumbstick;
			}
			else if (CheckKeyDown(m_Input, Keys.D2))
			{
				_player = PlayerIndex.Two;
				_controller = new ControllerWrapper(_player);
				_controller.Thumbsticks.ThumbstickScrubbing = _thumbstick;
			}
			else if (CheckKeyDown(m_Input, Keys.D3))
			{
				_player = PlayerIndex.Three;
				_controller = new ControllerWrapper(_player);
				_controller.Thumbsticks.ThumbstickScrubbing = _thumbstick;
			}
			else if (CheckKeyDown(m_Input, Keys.D4))
			{
				_player = PlayerIndex.Four;
				_controller = new ControllerWrapper(_player);
				_controller.Thumbsticks.ThumbstickScrubbing = _thumbstick;
			}

			//check if the player wants to face a different direction
			if (CheckKeyDown(m_Input, Keys.Q))
			{
				_flipped = !_flipped;
			}

			//check if the player wants to switch between scrubbed/powercurve
			if (CheckKeyDown(m_Input, Keys.W))
			{
				_thumbstick++;
				if (_thumbstick > DeadZoneType.PowerCurve)
				{
					_thumbstick = DeadZoneType.Axial;
				}
				_controller.Thumbsticks.ThumbstickScrubbing = _thumbstick;
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			spriteBatch.Begin();

			Vector2 position = new Vector2(graphics.GraphicsDevice.Viewport.TitleSafeArea.Left, graphics.GraphicsDevice.Viewport.TitleSafeArea.Top);
			
			//say what controller we are checking
			_text.Write("Controller Index: " + _player.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//is the controller plugged in?
			_text.Write("Controller Plugged In: " + _controller.ControllerPluggedIn.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//are we using the keyboard?
			_text.Write("Use Keyboard: " + _controller.UseKeyboard.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//say what type of thumbstick scrubbing we are doing
			_text.Write("Thumbstick type: " + _thumbstick.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.LineSpacing;

			//what direction is the player facing
			_text.Write("Player is facing: " + (_flipped ? "left" : "right"), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += (_text.Font.LineSpacing * 2.0f);
			float buttonPos = position.Y;

			//draw the current pressed state of each keystroke
			for (int i = 0; i <= ((int)EKeystroke.BackR); i++)
			{
				//Write the name of the button
				position.X = _text.Write(((EKeystroke)i).ToString() + ": ", position, Justify.Left, 1.0f, Color.White, spriteBatch);

				//is the button currently active
				if (_controller.CheckKeystroke((EKeystroke)i, _flipped))
				{
					position.X = _text.Write("held ", position, Justify.Left, 1.0f, Color.White, spriteBatch);
				}

				//move the position to the next line
				position.Y += _text.Font.LineSpacing;
				position.X = 0.0f;
			}

			//reset position
			position.Y = buttonPos;
			position.X = 256.0f;

			//draw the current released state of each keystroke
			for (int i = (int)EKeystroke.UpRelease; i <= ((int)EKeystroke.NeutralR); i++)
			{
				//Write the name of the button
				position.X = _text.Write(((EKeystroke)i).ToString() + ": ", position, Justify.Left, 1.0f, Color.White, spriteBatch);

				//is the button currently active
				if (_controller.CheckKeystroke((EKeystroke)i, _flipped))
				{
					position.X = _text.Write("held ", position, Justify.Left, 1.0f, Color.White, spriteBatch);
				}

				//move the position to the next line
				position.Y += _text.Font.LineSpacing;
				position.X = 256.0f;
			}

			//write the raw thumbstick direction
			position.X = _text.Write("direction: ", position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.X = _text.Write(_controller.Thumbsticks.LeftThumbstick.Direction.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);

			spriteBatch.End();

			base.Draw(gameTime);
		}

		/// <summary>
		/// Check if a keyboard key was pressed this update
		/// </summary>
		/// <param name="rInputState">current input state</param>
		/// <param name="i">controller index</param>
		/// <param name="myKey">key to check</param>
		/// <returns>bool: key was pressed this update</returns>
		private bool CheckKeyDown(InputState rInputState, Keys myKey)
		{
			return (rInputState.CurrentKeyboardState.IsKeyDown(myKey) && rInputState.LastKeyboardState.IsKeyUp(myKey));
		}

		#endregion //Methods
	}
}
