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
		public MainWindow()
		{
			InitializeComponent();
			MyAnim = new ArrayMovementAnimator(400, 300, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			ArrayIMAGE.Source = MyAnim.CurrentFrameImage;
			MyAnim.FrameUpdated += OnFrameUpdate;
		}
		private async void but1_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(() =>
			{
				var Acts = MyAnim.ToSelectedColorActions(1,5);
				Acts.MoveNext();


				while (Acts.MoveNext())
				{
					Acts.Current.Invoke();
				}


				Acts = MyAnim.ToSelectedColorActions(2, 5);
				Acts.MoveNext();

				while (Acts.MoveNext())
				{
					Acts.Current.Invoke();
				}

				Acts = MyAnim.ToCommonColorActions(1, 5);
				Acts.MoveNext();

				while (Acts.MoveNext())
				{
					Acts.Current.Invoke();
				}
				Acts = MyAnim.ToCommonColorActions(2, 5);
				Acts.MoveNext();

				while (Acts.MoveNext())
				{
					Acts.Current.Invoke();
				}
			});
		}

		private void OnFrameUpdate(object sender, EventArgs e)
		{
			MyAnim.RenderCurrentFrame();
			MyAnim.CurrentFrameImage.Freeze();
			Dispatcher.Invoke(new Action(()=> ArrayIMAGE.Source = MyAnim.CurrentFrameImage));
		}
	}
}
