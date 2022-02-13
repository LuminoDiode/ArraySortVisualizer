using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace WpfApp3
{
	class ArraySpecialSorts
	{
		private static void Swap<T>(IList<T> Arr, int i1, int i2)
		{
			var temp = Arr[i1];
			Arr[i1] = Arr[i2];
			Arr[i2] = temp;
		}

		// Расческой
		public static void SortByCombSort<T>(IList<T> Arr, out Queue<SwapAction>Actions) where T : IComparable<T>
		{
			Actions = new Queue<SwapAction>();
			const double factor = 1.2473309; // фактор уменьшения
			double Step = Arr.Count - 1; // шаг сортировки

			while (Step > 1)
			{
				for (int i = 0; i + Step < Arr.Count; i++)
				{
					if (Arr[i].CompareTo(Arr[(int)(i + Step)]) > 0)
					{
						Actions.Enqueue(new SwapAction(i, (int)(i + Step)));
						Swap(Arr, i, (int)(i + Step));
					}
				}
				Step /= factor;
			}
		}
	}
}