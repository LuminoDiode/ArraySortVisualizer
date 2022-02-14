using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp3
{
	internal class SortingActionsProvider
	{

		// https://stackoverflow.com/questions/36906/what-is-the-fastest-way-to-swap-values-in-c/615995#615995
		public static void Swap<T>(IList<T> Arr, int i1, int i2)
		{
			var temp = Arr[i1];
			Arr[i1] = Arr[i2];
			Arr[i2] = temp;
		}

		// Расческой
		public static IEnumerator<SwapAction> SortByCombSort(int[] Arr) 
		{
			const double factor = 1.2473309; // фактор уменьшения
			double Step = Arr.Length - 1; // шаг сортировки
			
			while (Step > 1)
			{
				for (int i = 0; i + Step < Arr.Length; i++)
				{
					if (Arr[i].CompareTo(Arr[(int)(i + Step)]) > 0)
					{
						Swap(Arr, i, (int)(i + Step));
						yield return new SwapAction(i, (int)(i + Step));
					}
				}
				Step /= factor;
			}
			/*
			for(int i =0; i < Arr.Length; i++)
			{
				if (Arr[i] > Arr[i + 1])
				{
					yield return new SwapAction(i, i + 1);
					Swap(Arr, i, i + 1);
					i = 0;
				}
			}*/
			Console.WriteLine();
		}
	}
}
