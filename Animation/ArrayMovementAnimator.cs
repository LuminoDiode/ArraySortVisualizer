using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;

namespace WpfApp3
{
	class GraphicalObject
	{
		public Bitmap Image;
		public Point LeftUpperCornerLocation;
		public Color CurrentColor => Image.GetPixel(0, 0);
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
		const int DefaultNumOfFrames = 60;
		const int SpaceBetweenColumns = 2;

		public event EventHandler FrameUpdated;


		public Bitmap CurrentFrame;
		public BitmapImage CurrentFrameImage => BitmapToImageSource(CurrentFrame);
		public int FrameWidth { get; private set; }
		public int FrameHeight { get; private set; }
		public int[] AnimatedArray { get; private set; }
		public SolidBrush CommonElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.LightBlue);
		public SolidBrush SelectedElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.DarkGreen);
		public SolidBrush BackgroundBrush { get; set; } = new SolidBrush(System.Drawing.Color.WhiteSmoke);
		private GraphicalObject[] ArrayElementsAsGraphicalObjects;
		private int ElementWidth => FrameWidth / AnimatedArray.Length - ((AnimatedArray.Length - 1) * SpaceBetweenColumns);


		public ArrayMovementAnimator(int FrameWidth, int FrameHeight, int[] AnimatedArray)
		{
			this.FrameWidth = FrameWidth;
			this.FrameHeight = FrameHeight;
			this.AnimatedArray = AnimatedArray;
			// (FrameWidth-(AnimatedArray.Length-1)*SpaceBetweenColumns) / AnimatedArray.Length  -- оставить место в размере 2 пикселей для пробелов между столбиками
			this.ArrayElementsAsGraphicalObjects = ArrayToGraphicalObjects(this.AnimatedArray, (FrameWidth - (AnimatedArray.Length - 1) * SpaceBetweenColumns) / AnimatedArray.Length, FrameHeight);
			this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
			this.RenderCurrentFrame();
			//this.CurrentFrameImage = BitmapToImageSource(CurrentFrame);
		}

		private GraphicalObject[] ArrayToGraphicalObjects(int[] Arr, int OneElementWidth, int MaxHeight)
		{
			GraphicalObject[] Out = new GraphicalObject[Arr.Length];
			int HeightCoeff = MaxHeight / Arr.Max();

			for (int i = 0; i < Arr.Length; i++)
			{
				var CurrElHeight = Arr[i] * HeightCoeff;
				Out[i] = new GraphicalObject(
					 new Point(this.ElementWidth * i + SpaceBetweenColumns * i, MaxHeight - CurrElHeight),
					 new Size(ElementWidth, CurrElHeight));
				Out[i].SetColor(this.CommonElementBrush.Color);
			}

			return Out;
		}

		private void CopyBitmapToAnother(Bitmap SourceBitmap, Bitmap DestinationBitmap, Point DestinationLeftUpperCorner)
		{
			for (int SourceX = 0; SourceX < SourceBitmap.Width && (SourceX + DestinationLeftUpperCorner.X) < DestinationBitmap.Width; SourceX++)
			{
				for (int SourceY = 0; SourceY < SourceBitmap.Height && (SourceY + DestinationLeftUpperCorner.Y) < DestinationBitmap.Height; SourceY++)
				{
					var SetX = SourceX + DestinationLeftUpperCorner.X;
					var SetY = SourceY + DestinationLeftUpperCorner.Y;
					var SourcePixel = SourceBitmap.GetPixel(SourceX, SourceY);
					DestinationBitmap.SetPixel(SetX, SetY, SourcePixel);
				}
			}
		}

		public void RenderCurrentFrame()
		{
			var Gr = Graphics.FromImage(this.CurrentFrame);

			var GOs = this.ArrayElementsAsGraphicalObjects;

			Gr.FillRectangle(this.BackgroundBrush, 0, 0, this.CurrentFrame.Width, this.CurrentFrame.Height);

			var id = 0;
			foreach (var GO in this.ArrayElementsAsGraphicalObjects.OrderBy(x => x.LayerId))
				CopyBitmapToAnother(GO.Image, this.CurrentFrame, CalculateLeftUpperCornerLocationOfTheElement(id++));
		}

		private Point CalculateLeftUpperCornerLocationOfTheElement(int ElementId)
			=> new Point(this.ElementWidth * ElementId + SpaceBetweenColumns * ElementId, this.FrameHeight - this.ArrayElementsAsGraphicalObjects[ElementId].Image.Height);

		public IEnumerator<Action> FillAnimationActions(GraphicalObject ArrayElement, Color TargetColor, int FramesNum)
		{
			double RedStep = (TargetColor.R - ArrayElement.CurrentColor.R) / (double)(FramesNum);
			double GreenStep = (TargetColor.G - ArrayElement.CurrentColor.G) / (double)(FramesNum);
			double BlueStep = (TargetColor.B - ArrayElement.CurrentColor.B) / (double)(FramesNum);

			for (int i = 0; i < FramesNum-1; i++)
			{
				int CurrentRed = (int)(RedStep + ArrayElement.CurrentColor.R);
				int CurrentGreen = (int)(GreenStep + ArrayElement.CurrentColor.G);
				int CurrentBlue = (int)(BlueStep + ArrayElement.CurrentColor.B);

				if (CurrentRed < 0) CurrentRed = 0;
				if (CurrentGreen < 0) CurrentGreen = 0;
				if (CurrentBlue < 0) CurrentBlue = 0;

				yield return new Action(() =>
				{
					ArrayElement.SetColor(Color.FromArgb(CurrentRed, CurrentGreen, CurrentBlue));
					FrameUpdated.Invoke(this, EventArgs.Empty);
				});
			}
			yield return new Action(() =>
			{
				ArrayElement.SetColor(TargetColor);
				FrameUpdated.Invoke(this, EventArgs.Empty);
			});
		}
		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для выделения элемента.
		/// </summary>
		public IEnumerator<Action> ToSelectedColorActions(int ElementId, int FramesNum = DefaultNumOfFrames)
			=> FillAnimationActions(ArrayElementsAsGraphicalObjects[ElementId], SelectedElementBrush.Color, FramesNum);
		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для снятия выделения элемента.
		/// </summary>
		public IEnumerator<Action> ToCommonColorActions(int ElementId, int FramesNum = DefaultNumOfFrames)
			=> FillAnimationActions(ArrayElementsAsGraphicalObjects[ElementId], CommonElementBrush.Color, FramesNum);

		/// <summary>
		/// Возвращает делегаты Action, которые должны быть выполнены для перемещения объекта.
		/// </summary>
		public IEnumerator<Action> MoveAnimationActions(int ElementId, Point DestionationPoint, int FramesNum = DefaultNumOfFrames)
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
					FrameUpdated.Invoke(this, EventArgs.Empty);
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
		public IEnumerator<Action[]> SwapAnimationActions(int FirstElementId, int SecondElementId, int FramesNum = DefaultNumOfFrames)
		{
			var UpdateInvoker = new Action(() => FrameUpdated.Invoke(this, EventArgs.Empty));

			var FirstGO = this.ArrayElementsAsGraphicalObjects[FirstElementId];
			var SecondGO = this.ArrayElementsAsGraphicalObjects[SecondElementId];

			FirstGO.LayerId = 1;
			SecondGO.LayerId = 1;

			var FirstSelectActions = ToSelectedColorActions(FirstElementId);
			var SecondSelectActions = ToSelectedColorActions(SecondElementId);

			while (FirstSelectActions.MoveNext() && SecondSelectActions.MoveNext())
			{
				var CurrentFirst = FirstSelectActions.Current;
				var CurrentSecond = SecondSelectActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			var FirstElementDestination = new Point(SecondGO.LeftUpperCornerLocation.X, FirstGO.LeftUpperCornerLocation.Y);
			var SecondElementDestination = new Point(FirstGO.LeftUpperCornerLocation.X, SecondGO.LeftUpperCornerLocation.Y);

			var FirstMoveActions = MoveAnimationActions(FirstElementId, FirstElementDestination);
			var SecondMoveActions = MoveAnimationActions(SecondElementId, SecondElementDestination);

			while (FirstMoveActions.MoveNext() && SecondMoveActions.MoveNext())
			{
				var CurrentFirst = FirstMoveActions.Current;
				var CurrentSecond = SecondMoveActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			var FirstUnselectActions = ToCommonColorActions(FirstElementId);
			var SecondUnselectActions = ToCommonColorActions(SecondElementId);

			while (FirstUnselectActions.MoveNext() && SecondUnselectActions.MoveNext())
			{
				var CurrentFirst = FirstUnselectActions.Current;
				var CurrentSecond = SecondUnselectActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			FirstGO.LayerId = 0;
			SecondGO.LayerId = 0;

			(FirstGO, SecondGO) = (SecondGO, FirstGO);
		}

		public void PerformSwapInSourceArrays(int i1, int i2)
		{
			(AnimatedArray[i1], AnimatedArray[i2]) = (AnimatedArray[i2], AnimatedArray[i1]);
		}




		// https://stackoverflow.com/a/22501616/11325184
		private BitmapImage BitmapToImageSource(Bitmap bitmap)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
				memory.Position = 0;
				BitmapImage bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();

				return bitmapimage;
			}
		}
	}
}
