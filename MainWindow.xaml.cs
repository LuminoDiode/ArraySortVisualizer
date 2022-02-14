using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace WpfApp3
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ArrayMovementAnimator MyAnim;
		int[] AnimatedArr;
		CancellationTokenSource CtSource = new CancellationTokenSource();
		public MainWindow()
		{
			InitializeComponent();
			this.AnimatedArr = new int[5].Select(x=> new Random().Next(1,101)).ToArray();
			MyAnim = new ArrayMovementAnimator(900, 300, AnimatedArr);
			ArrayIMAGE.Source = MyAnim.CurrentFrameImage;
			MyAnim.FrameUpdated += OnFrameUpdate;
			MyAnim.FrameRate = (int)Math.Round(SpeedSlider.Value);

			this.ArraySizeTB.Text = Math.Round(this.ArraySizeSlider.Value).ToString();
			this.SpeedTB.Text = Math.Round(this.SpeedSlider.Value).ToString();
		}

		private void OnFrameUpdate(object sender, EventArgs e)
		{
			MyAnim.RenderCurrentFrame();
			//MyAnim.CurrentFrameImage.Freeze();
			Dispatcher.Invoke(new Action(()=> ArrayIMAGE.Source = MyAnim.CurrentFrameImage));
		}
		private void InvokeAll(IEnumerator<Action>Acts)
		{
			while (Acts.MoveNext())
			{
				Acts.Current.Invoke();
			}
		}
		private void InvokeAll(IEnumerator<Action[]> Acts)
		{
			while (Acts.MoveNext())
				foreach (var act in Acts.Current)
				{
					act.Invoke();
				}
		}

		private void ArraySizeSlider_ValueChanged(object sender, EventArgs e)
		{
			var sld = (Slider)sender;
			this.ArraySizeTB.Text = Math.Round(sld.Value).ToString();
		}

		private void SpeedSlider_ValueChanged(object sender, EventArgs e)
		{
			var sld = (Slider)sender;
			this.SpeedTB.Text = Math.Round(sld.Value).ToString();
			this.MyAnim.FrameRate = (int)Math.Round(sld.Value);
		}

		private void GenerateArrayButton_Click(object sender, RoutedEventArgs e)
		{
			this.IsEnabled= false;
			this.AnimatedArr = new int[(int)this.ArraySizeSlider.Value].Select(x => new Random().Next(1, 101)).ToArray();
			this.MyAnim = new ArrayMovementAnimator(900, 300, this.AnimatedArr);
			MyAnim.FrameRate = (int)Math.Round(SpeedSlider.Value);
			MyAnim.FrameUpdated += OnFrameUpdate;
			OnFrameUpdate(this, EventArgs.Empty);
			this.IsEnabled = true;
		}

		private async void SortArrayButton_Click(object sender, RoutedEventArgs e)
		{
			CtSource.Cancel();
			CtSource = new CancellationTokenSource();

			var FR = (int)(this.SpeedSlider.Value * 3);

			await Task.Run(() =>
			{
				Dispatcher.Invoke(() => ButtonsIsEnabled(false));
				InvokeAll(MyAnim.GetSortAnimationActions(SortingActionsProvider.SortByCombSort(AnimatedArr), null));
				Dispatcher.Invoke(() => ButtonsIsEnabled(true));
			}, CtSource.Token);
		}

		private void ButtonsIsEnabled(bool enabled)
		{
			this.GenerateArrayButton.IsEnabled = enabled;
			this.SortArrayButton.IsEnabled = enabled;
			this.ArraySizeSlider.IsEnabled= enabled;
		}

		private void StopSortingButton_Click(object sender, RoutedEventArgs e)
		{
			this.CtSource.Cancel();
			ButtonsIsEnabled(true);
		}
	}
}
