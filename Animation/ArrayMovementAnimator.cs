using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;

namespace WpfApp3
{

	class ValueColumn
	{

		//public Bitmap Image;
		public Point LeftUpperCornerLocation;
		public Color CurrentColor;
		public Size Size;
		public int LayerId;
		public ValueColumn(Point Location, Size Size, int LayedId = 0)
		{
			this.LeftUpperCornerLocation = Location;
			this.Size = Size;
			this.LayerId = LayedId;
		}
		public void SetColor(Color Color)
		{
			this.CurrentColor = Color;
		}
	}


	class ArrayMovementAnimator
	{
		const int DefaultNumOfFrames = 60;
		const int SpaceBetweenColumns = 2;

		public event EventHandler FrameUpdated;

		public Bitmap CurrentFrame { get; private set; }
		public BitmapImage CurrentFrameImage => BitmapToImageSource(CurrentFrame);
		public int FrameWidth { get; private set; }
		public int FrameHeight { get; private set; }
		public int[] AnimatedArray { get; private set; }
		public SolidBrush CommonElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.LightBlue);
		public SolidBrush SelectedElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.Blue);
		public SolidBrush InactiveElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.DarkGray);
		public SolidBrush BackgroundBrush { get; set; } = new SolidBrush(System.Drawing.Color.WhiteSmoke);

		private Graphics FrameGraphics;
		private ValueColumn[] ArrayElementsAsGraphicalObjects;
		private int ElementWidth;


		public ArrayMovementAnimator(int FrameWidth, int FrameHeight, int[] AnimatedArray)
		{
			this.FrameWidth = FrameWidth;
			this.FrameHeight = FrameHeight;
			this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
			this.FrameGraphics = Graphics.FromImage(this.CurrentFrame);

			this.AnimatedArray = AnimatedArray;

			this.ElementWidth = (FrameWidth - (AnimatedArray.Length - 1) * SpaceBetweenColumns) / AnimatedArray.Length;

			this.ArrayElementsAsGraphicalObjects =
				ArrayToGraphicalObjects(this.AnimatedArray, ElementWidth, FrameHeight);

			this.RenderCurrentFrame();
		}

		private ValueColumn[] ArrayToGraphicalObjects(int[] Arr, int OneElementWidth, int MaxHeight)
		{
			ValueColumn[] Out = new ValueColumn[Arr.Length];
			int HeightCoeff = MaxHeight / Arr.Max();

			for (int i = 0; i < Arr.Length; i++)
			{
				var CurrElHeight = Arr[i] * HeightCoeff;
				Out[i] = new ValueColumn(
					 new Point(this.ElementWidth * i + SpaceBetweenColumns * i, MaxHeight - CurrElHeight),
					 new Size(ElementWidth, CurrElHeight));
				Out[i].CurrentColor = this.CommonElementBrush.Color;

				var LocationX = this.ElementWidth * i + SpaceBetweenColumns * i;
				var LocationY = this.FrameHeight - CurrElHeight;
				Out[i].LeftUpperCornerLocation = new Point(LocationX, LocationY);
			}

			return Out;
		}


		public void RenderCurrentFrame()
		{
			var GOs = this.ArrayElementsAsGraphicalObjects; // GraphicalObject == GO

			this.FrameGraphics.FillRectangle(this.BackgroundBrush, 0, 0, this.CurrentFrame.Width, this.CurrentFrame.Height);

			foreach (var GO in this.ArrayElementsAsGraphicalObjects.OrderBy(x => x.LayerId))
			{
				this.FrameGraphics.FillRectangle(new SolidBrush(GO.CurrentColor), new Rectangle(GO.LeftUpperCornerLocation, GO.Size));
			}
		}

		public IEnumerator<Action> FillAnimationActions(ValueColumn ArrayElement, Color TargetColor, int FramesNum)
		{
			float RedStart = ArrayElement.CurrentColor.R;
			float GreenStart = ArrayElement.CurrentColor.G;
			float BlueStart = ArrayElement.CurrentColor.B;

			float RedStep = (TargetColor.R - ArrayElement.CurrentColor.R) / (float)(FramesNum);
			float GreenStep = (TargetColor.G - ArrayElement.CurrentColor.G) / (float)(FramesNum);
			float BlueStep = (TargetColor.B - ArrayElement.CurrentColor.B) / (float)(FramesNum);

			int CurrentRed, CurrentGreen, CurrentBlue;

			var stop = (FramesNum - 1);
			for (int i = 0; i < stop; i++)
			{
				CurrentRed = (int)(RedStep * i + RedStart);
				CurrentGreen = (int)(GreenStep * i + GreenStart);
				CurrentBlue = (int)(BlueStep * i + BlueStart);

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
			ValueColumn GO = this.ArrayElementsAsGraphicalObjects[ElementId];

			float StartX = GO.LeftUpperCornerLocation.X;
			float StartY = GO.LeftUpperCornerLocation.Y;

			float StepX = (DestionationPoint.X - GO.LeftUpperCornerLocation.X) / (float)FramesNum;
			float StepY = (DestionationPoint.Y - GO.LeftUpperCornerLocation.Y) / (float)FramesNum;

			for (int i = 0; i < FramesNum; i++)
			{
				yield return new Action(() =>
				{
					GO.LeftUpperCornerLocation.X = (int)(StartX + StepX * i);
					GO.LeftUpperCornerLocation.Y = (int)(StartY + StepY * i);
					FrameUpdated.Invoke(this, EventArgs.Empty);
				});
			}
			yield return new Action(() =>
			{
				GO.LeftUpperCornerLocation = DestionationPoint;
				FrameUpdated.Invoke(this, EventArgs.Empty);
			});
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

			yield return new Action[] {new Action(() =>
			{
				FirstGO.LayerId = 1;
				SecondGO.LayerId = 1;
			}) };

			var FirstSelectActions = ToSelectedColorActions(FirstElementId, FramesNum);
			var SecondSelectActions = ToSelectedColorActions(SecondElementId, FramesNum);

			while (FirstSelectActions.MoveNext() && SecondSelectActions.MoveNext())
			{
				var CurrentFirst = FirstSelectActions.Current;
				var CurrentSecond = SecondSelectActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			var FirstElementDestination = new Point(SecondGO.LeftUpperCornerLocation.X, FirstGO.LeftUpperCornerLocation.Y);
			var SecondElementDestination = new Point(FirstGO.LeftUpperCornerLocation.X, SecondGO.LeftUpperCornerLocation.Y);

			var FirstMoveActions = MoveAnimationActions(FirstElementId, FirstElementDestination, FramesNum);
			var SecondMoveActions = MoveAnimationActions(SecondElementId, SecondElementDestination, FramesNum);

			while (FirstMoveActions.MoveNext() && SecondMoveActions.MoveNext())
			{
				var CurrentFirst = FirstMoveActions.Current;
				var CurrentSecond = SecondMoveActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			var FirstUnselectActions = ToCommonColorActions(FirstElementId, FramesNum);
			var SecondUnselectActions = ToCommonColorActions(SecondElementId, FramesNum);

			while (FirstUnselectActions.MoveNext() && SecondUnselectActions.MoveNext())
			{
				var CurrentFirst = FirstUnselectActions.Current;
				var CurrentSecond = SecondUnselectActions.Current;
				yield return new Action[] { CurrentFirst, CurrentSecond, UpdateInvoker };
			}

			yield return new Action[] {new Action(() =>
			{
				FirstGO.LayerId = 0;
				SecondGO.LayerId = 0;
				SortingActionsProvider.Swap(this.ArrayElementsAsGraphicalObjects,FirstElementId,SecondElementId);
			}) };
		}

		public IEnumerator<Action[]> GetSortAnimationActions(IEnumerator<SwapAction> SwapActionsEnumerator,int FramesNum = DefaultNumOfFrames)
		{
			while (SwapActionsEnumerator.MoveNext())
			{
				var CurrentSwap = SwapAnimationActions(SwapActionsEnumerator.Current.FirstIndex, SwapActionsEnumerator.Current.SecondIndex,FramesNum);
				while (CurrentSwap.MoveNext())
				{
					yield return CurrentSwap.Current;
				}
			}
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
