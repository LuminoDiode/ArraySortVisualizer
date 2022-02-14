﻿using System;
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
		public MainWindow()
		{
			InitializeComponent();
			MyAnim = new ArrayMovementAnimator(400, 300, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			ArrayIMAGE.Source = MyAnim.CurrentFrameImage;
			MyAnim.FrameUpdated += OnFrameUpdate;
		}
		private async void but1_Click(object sender, RoutedEventArgs e)
		{
			but1.IsEnabled = false;
			await Task.Run(() =>
			{
				InvokeAll(MyAnim.ToSelectedColorActions(3,10));

				InvokeAll(MyAnim.MoveAnimationActions(3, new System.Drawing.Point(0, 0), 10));

				InvokeAll(MyAnim.ToCommonColorActions(3, 10));

				InvokeAll(MyAnim.SwapAnimationActions(5, 7, 10));

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
			while(Acts.MoveNext())
				Acts.Current.Invoke();
		}
		private void InvokeAll(IEnumerator<Action[]> Acts)
		{
			while (Acts.MoveNext())
				foreach(var act in Acts.Current)
					act.Invoke();
		}
	}
}
