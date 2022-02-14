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
		public MainWindow()
		{
			InitializeComponent();
			this.AnimatedArr = new int[10].Select(x=> new Random().Next(1,101)).ToArray();
			MyAnim = new ArrayMovementAnimator(400, 300, AnimatedArr);
			ArrayIMAGE.Source = MyAnim.CurrentFrameImage;
			MyAnim.FrameUpdated += OnFrameUpdate;
		}
		private async void but1_Click(object sender, RoutedEventArgs e)
		{
			but1.IsEnabled = false;
			await Task.Run(() =>
			{
				/*
				InvokeAll(MyAnim.ToSelectedColorActions(3,30));

				InvokeAll(MyAnim.MoveAnimationActions(3, new System.Drawing.Point(0, 0), 30));

				InvokeAll(MyAnim.ToCommonColorActions(3, 30));

				InvokeAll(MyAnim.SwapAnimationActions(5, 7, 30));
				*/

				InvokeAll(MyAnim.GetSortAnimationActions(SortingActionsProvider.SortByCombSort(AnimatedArr),10));
			});
			but1.IsEnabled = true;
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
	}
}
