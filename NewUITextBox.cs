using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WheresMyItems
{
	// NewUITextBox is a WIP that may make it into tmodloader proper.
	internal class NewUITextBox : UITextPanel<string>
	{
		private bool focused = false;
		private int _cursor;
		private int _frameCount;
		private int _maxLength = 30;
		private string hintText;

		public event Action OnFocus;

		public event Action OnUnfocus;

		public NewUITextBox(string text, float textScale = 1, bool large = false) : base("", textScale, large)
		{
			hintText = text;
			//	keyBoardInput.newKeyEvent += KeyboardInput_newKeyEvent;
		}

		public override void Click(UIMouseEvent evt)
		{
			Focus();
			base.Click(evt);
		}

		private void KeyboardInput_newKeyEvent(char obj)
		{
			// Problem: keyBoardInput.newKeyEvent only fires on regular keyboard buttons.

			if (!focused) return;
			//if (obj.Equals((char)Keys.Back)) // '\b'
			//{
			//	Backspace();
			//}
			//else if (obj.Equals((char)Keys.Enter))
			//{
			//	Unfocus();
			//	Main.chatRelease = false;
			//}
			//else
			if (Char.IsLetterOrDigit(obj))
			{
				if (Char.IsDigit(obj))
				{
					Main.blockKey = "D" + obj.ToString();
				}
				Write(obj.ToString());
			}
		}

		public void Unfocus()
		{
			if (focused)
			{
				focused = false;
				Main.blockInput = false;

				OnUnfocus?.Invoke();
				//WheresMyItemsUI.visible = false;
			}
		}

		public void Focus()
		{
			if (!focused)
			{
				focused = true;
				Main.blockInput = true;
				Main.clrInput();

				OnFocus?.Invoke();

				counter = 0;
			}
		}

		public override void Update(GameTime gameTime)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (focused && !ContainsPoint(MousePosition) && (Main.mouseLeft || Main.mouseRight) && !WheresMyItemsUI.dragging)
			{
				Main.LocalPlayer.mouseInterface = true;
				Unfocus();
			}
			base.Update(gameTime);
		}

		public void Write(string text)
		{
			base.SetText(base.Text.Insert(this._cursor, text));
			this._cursor += text.Length;
			_cursor = Math.Min(Text.Length, _cursor);
			Recalculate();
		}

		public override void SetText(string text, float textScale, bool large)
		{
			if (text.ToString().Length > this._maxLength)
			{
				text = text.ToString().Substring(0, this._maxLength);
			}
			base.SetText(text, textScale, large);

			this.MinWidth.Set(120, 0f);

			this._cursor = Math.Min(base.Text.Length, this._cursor);
		}

		public void SetTextMaxLength(int maxLength)
		{
			this._maxLength = maxLength;
		}

		public void Backspace()
		{
			if (this._cursor == 0)
			{
				return;
			}
			base.SetText(base.Text.Substring(0, base.Text.Length - 1));
			Recalculate();
		}

		public void CursorLeft()
		{
			if (this._cursor == 0)
			{
				return;
			}
			this._cursor--;
		}

		public void CursorRight()
		{
			if (this._cursor < base.Text.Length)
			{
				this._cursor++;
			}
		}

		internal int counter = 0;

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			counter++;
			if (focused)
			{
				Terraria.GameInput.PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				SetText(Main.GetInputText(Text));
				//Main.keyCount = 0;
			}
			if (counter > 5 && WheresMyItems.RandomBuffHotKey.JustPressed)
			{
				Unfocus();
			}
			//if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
			{
				Main.chatRelease = false;
				Main.drawingPlayerChat = false;
				Main.inputTextEnter = true;
			}
			if (Main.inputTextEnter || Main.inputTextEscape)
			{
				Main.chatRelease = false;
				Unfocus();
				Main.inputTextEscape = false;
				Main.playerInventory = false;
				Main.LocalPlayer.releaseInventory = false;
				Main.LocalPlayer.controlInv = false;
			}
			//HandleSpecialKeys();
			this._cursor = base.Text.Length;
			//base.DrawSelf(spriteBatch);
			{
				CalculatedStyle innerDimensions2 = base.GetInnerDimensions();
				Vector2 pos2 = innerDimensions2.Position();
				if (IsLarge)
				{
					pos2.Y -= 10f * TextScale * TextScale;
				}
				else
				{
					pos2.Y -= 2f * TextScale;
				}
				//pos2.X += (innerDimensions2.Width - TextSize.X) * 0.5f;
				if (IsLarge)
				{
					Utils.DrawBorderStringBig(spriteBatch, Text, pos2, TextColor, TextScale, 0f, 0f, -1);
					return;
				}
				Utils.DrawBorderString(spriteBatch, Text, pos2, TextColor, TextScale, 0f, 0f, -1);
			}

			this._frameCount++;

			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 pos = innerDimensions.Position();
			DynamicSpriteFont spriteFont = base.IsLarge ? Main.fontDeathText : Main.fontMouseText;
			Vector2 vector = new Vector2(spriteFont.MeasureString(base.Text.Substring(0, this._cursor)).X, base.IsLarge ? 32f : 16f) * base.TextScale;
			if (base.IsLarge)
			{
				pos.Y -= 8f * base.TextScale;
			}
			else
			{
				pos.Y -= 1f * base.TextScale;
			}
			if (Text.Length == 0)
			{
				Vector2 hintTextSize = new Vector2(spriteFont.MeasureString(hintText.ToString()).X, IsLarge ? 32f : 16f) * TextScale;
				pos.X += 5;//(hintTextSize.X);
				if (base.IsLarge)
				{
					Utils.DrawBorderStringBig(spriteBatch, hintText, pos, Color.Gray, base.TextScale, 0f, 0f, -1);
					return;
				}
				Utils.DrawBorderString(spriteBatch, hintText, pos, Color.Gray, base.TextScale, 0f, 0f, -1);
				pos.X -= 5;
				//pos.X -= (innerDimensions.Width - hintTextSize.X) * 0.5f;
			}

			if (!focused) return;

			pos.X += /*(innerDimensions.Width - base.TextSize.X) * 0.5f*/ +vector.X - (base.IsLarge ? 8f : 4f) * base.TextScale + 6f;
			if ((this._frameCount %= 40) > 20)
			{
				return;
			}
			if (base.IsLarge)
			{
				Utils.DrawBorderStringBig(spriteBatch, "|", pos, base.TextColor, base.TextScale, 0f, 0f, -1);
				return;
			}
			Utils.DrawBorderString(spriteBatch, "|", pos, base.TextColor, base.TextScale, 0f, 0f, -1);
		}
	}
}