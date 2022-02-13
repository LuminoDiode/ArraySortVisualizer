using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;

namespace WpfApp3.Animation
{
	class GraphicalObject
	{
		public Bitmap Image;
		public Point LeftUpperCornerLocation;
		public Color CurrentColor;
		public int LayerId;
		private Graphics ImageGraphics;
		private Rectangle FullSizeRect;
		public GraphicalObject(Point Location, Size Size, int LayedId = 0)
		{
			this.LeftUpperCornerLocation = Location;
			this.Image = new Bitmap(Size.Width, Size.Height);
			this.LayerId = LayedId;
			this.ImageGraphics = Graphics.FromImage(this.Image);
			this.FullSizeRect = new Rectangle(new Point(0, 0), Size);
		}

		public void SetColor(Color Color)
			=> ImageGraphics.FillRectangle(new SolidBrush(Color), FullSizeRect);
	}
	class ArrayMovementAnimator
	{
		public Bitmap CurrentStateFrame;
		public int FrameWidth, FrameHeight;
		public int[] AnimatedArray;
		public SolidBrush CommonElementBrush = new SolidBrush(System.Drawing.Color.LightBlue);
		public SolidBrush SelectedElementBrush = new SolidBrush(System.Drawing.Color.DarkGreen);
		public SolidBrush BackgroundBrush = new SolidBrush(System.Drawing.Color.LightGray);
		private GraphicalObject[] ArrayElementsAsGraphicalObjects;
		public int ElementWidth;
		public int SpaceBetweenColumns = 2;

		public ArrayMovementAnimator(int FrameWidth, int FrameHeight, int[] AnimatedArray)
		{
			this.FrameWidth = FrameWidth;
			this.FrameHeight = FrameHeight;
			this.AnimatedArray = AnimatedArray;
			// (FrameWidth-(AnimatedArray.Length-1)*SpaceBetweenColumns) / AnimatedArray.Length  -- оставить место в размере 2 пикселей для пробелов между столбиками
			this.ArrayElementsAsGraphicalObjects = ArrayToGraphicalObjects(this.AnimatedArray, (FrameWidth - (AnimatedArray.Length - 1) * SpaceBetweenColumns) / AnimatedArray.Length, FrameHeight);
			ElementWidth = ArrayElementsAsGraphicalObjects[0].Image.Width;
		}

		private GraphicalObject[] ArrayToGraphicalObjects(int[] Arr, int OneElementWidth, int MaxHeight)
		{
			GraphicalObject[] Out = new GraphicalObject[Arr.Length];
			int HeightCoeff = MaxHeight / Arr.Max();

			for (int i = 0; i < Arr.Length; i++)
			{
				var CurrElHeight = Arr[i] * HeightCoeff;
				Out[i] = new GraphicalObject(
					 new Point(this.ElementWidth * i + this.SpaceBetweenColumns * i, MaxHeight - CurrElHeight),
					 new Size(ElementWidth, CurrElHeight));
			}

			return Out;
		}

		private void CopyBitmapToAnother(Bitmap SourceBitmap, Bitmap DestinationBitmap, Point DestinationLeftUpperCorner)
		{
			for (int SourceX = 0; SourceX < SourceBitmap.Width && (SourceX + DestinationLeftUpperCorner.X) < DestinationBitmap.Width; SourceX++)
			{
				for (int SourceY = 0; SourceY < SourceBitmap.Height && (SourceY + DestinationLeftUpperCorner.Y) < DestinationBitmap.Height; SourceY++)
				{
					DestinationBitmap.SetPixel(SourceX + DestinationLeftUpperCorner.X, SourceY + DestinationLeftUpperCorner.Y, SourceBitmap.GetPixel(SourceY, SourceY));
				}
			}
		}

		public Bitmap RenderCurrentFrame()
		{
			Bitmap Out = new Bitmap(this.FrameWidth, this.FrameHeight);
			var Gr = Graphics.FromImage(Out);

			var GOs = this.ArrayElementsAsGraphicalObjects;

			Gr.FillRectangle(this.BackgroundBrush, 0, 0, Out.Width, Out.Height);

			var id = 0;
			foreach (var GO in this.ArrayElementsAsGraphicalObjects.OrderBy(x => x.LayerId))
				CopyBitmapToAnother(GO.Image, Out, CalculateLeftUpperCornerLocationOfTheElement(id++));

			return Out;
		}

		private Point CalculateLeftUpperCornerLocationOfTheElement(int ElementId)
			=> new Point(this.ElementWidth * ElementId + this.SpaceBetweenColumns * ElementId,this.FrameHeight- this.ArrayElementsAsGraphicalObjects[ElementId].Image.Height);

		public IEnumerator<Action> FillAnimationActions(GraphicalObject ArrayElement, Color TargetColor, int FramesNum)
		{
			double RedStep = (TargetColor.R - ArrayElement.CurrentColor.R) / FramesNum;
			double GreenStep = (TargetColor.G - ArrayElement.CurrentColor.G) / FramesNum;
			double BlueStep = (TargetColor.B - ArrayElement.CurrentColor.B) / FramesNum;

			for (int i = 0; i < FramesNum; i++)
			{
				int CurrentRed = (int)(RedStep * i + ArrayElement.CurrentColor.R);
				int CurrentGreen = (int)(GreenStep * i + ArrayElement.CurrentColor.G);
				int CurrentBlue = (int)(BlueStep * i + ArrayElement.CurrentColor.B);

				yield return new Action(() => ArrayElement.SetColor(Color.FromArgb(CurrentRed, CurrentGreen, CurrentBlue)));
			}
		}
		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для выделения элемента.
		/// </summary>
		public IEnumerator<Action> ToSelectedColorActions(int ElementId, int FramesNum = 60)
			=> FillAnimationActions(ArrayElementsAsGraphicalObjects[ElementId], SelectedElementBrush.Color, FramesNum);
		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для снятия выделения элемента.
		/// </summary>
		public IEnumerator<Action> ToCommonColorActions(int ElementId, int FramesNum = 60)
			=> FillAnimationActions(ArrayElementsAsGraphicalObjects[ElementId], CommonElementBrush.Color, FramesNum);

		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для перемещения объекта.
		/// </summary>
		public IEnumerator<Action> MoveAnimationActions(int ElementId, Point DestionationPoint, int FramesNum = 60)
		{
			GraphicalObject GO = this.ArrayElementsAsGraphicalObjects[ElementId];

			double StepX = (GO.LeftUpperCornerLocation.X - DestionationPoint.X) / FramesNum;
			double StepY = (GO.LeftUpperCornerLocation.Y - DestionationPoint.Y) / FramesNum;

			for (int i = 0; i < FramesNum; i++)
			{
				yield return new Action(() =>
				{
					GO.LeftUpperCornerLocation.X += (int)(StepX * i);
					GO.LeftUpperCornerLocation.Y += (int)(StepY * i);
				});
			}
		}

		/// <summary>
		/// Возвращает наборы делегатов Action, которые должны быть выполнены для показа анимации обмена элементов.
		/// </summary>
		/// <param name="ElementId"></param>
		/// <param name="DestionationPoint"></param>
		/// <param name="FramesNum"></param>
		/// <returns></returns>
		public IEnumerator<Action[]> SwapAnimationActions(int ElementId, Point DestionationPoint, int FramesNum = 60)
		{
			
		}

	}
}
