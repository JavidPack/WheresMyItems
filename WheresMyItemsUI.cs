using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ID;

namespace WheresMyItems
{
	class WheresMyItemsUI : UIState
	{
		public UIPanel searchBarPanel;
		public static bool visible = false;
		public static NewUITextBox box;

		public static string SearchTerm
		{
			get { return box.Text; }
		}

		public override void OnInitialize()
		{
			searchBarPanel = new UIPanel();
			searchBarPanel.SetPadding(0);
			//searchBarPanel.Left.Set(0, .5f);
			//searchBarPanel.Top.Set(0, .5f);
			searchBarPanel.Top.Set(50, 0f);
			searchBarPanel.HAlign = 0.5f;
			searchBarPanel.VAlign = 0.5f;
			searchBarPanel.Width.Set(170f, 0f);
			searchBarPanel.Height.Set(30f, 0f);
			searchBarPanel.BackgroundColor = new Color(73, 94, 171);
			searchBarPanel.OnMouseDown += DragStart;
			searchBarPanel.OnMouseUp += DragEnd;

			Texture2D buttonPlayTexture = ModLoader.GetTexture("Terraria/UI/Cursor_2");
			UIImage playButton = new UIImage(buttonPlayTexture);
			playButton.Left.Set(5, 0f);
			playButton.Top.Set(5, 0f);
			searchBarPanel.Append(playButton);

			box = new NewUITextBox("Type here to search", 0.78f);
			box.BackgroundColor = Color.Transparent;
			box.BorderColor = Color.Transparent;
			box.Left.Pixels = 15;
			box.Top.Pixels = -5;
			box.MinWidth.Pixels = 120;
			box.OnUnfocus += () => visible = false;
			searchBarPanel.Append(box);


			Append(searchBarPanel);
		}

		private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			visible = false;
		}

		// Drag support
		Vector2 offset;
		public static bool dragging = false;
		private void DragStart(UIMouseEvent evt, UIElement listeningElement)
		{
			offset = new Vector2(evt.MousePosition.X - searchBarPanel.Left.Pixels, evt.MousePosition.Y - searchBarPanel.Top.Pixels);
			dragging = true;
		}

		private void DragEnd(UIMouseEvent evt, UIElement listeningElement)
		{
			Vector2 end = evt.MousePosition;
			dragging = false;

			searchBarPanel.Left.Set(end.X - offset.X, 0f);
			searchBarPanel.Top.Set(end.Y - offset.Y, 0f);

			Recalculate();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			Vector2 MousePosition = new Vector2((float)Main.mouseX, (float)Main.mouseY);
			if (searchBarPanel.ContainsPoint(MousePosition))
			{
				Main.LocalPlayer.mouseInterface = true;
			}
			if (dragging)
			{
				searchBarPanel.Left.Set(MousePosition.X - offset.X, 0f);
				searchBarPanel.Top.Set(MousePosition.Y - offset.Y, 0f);
				Recalculate();
			}
		}
	}
}
