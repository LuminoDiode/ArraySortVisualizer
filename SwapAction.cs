using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp3
{
	struct SwapAction
	{
		public int FirstIndex, SecondIndex;
		public SwapAction(int FirstIndex, int SecondIndex)
		{
			this.FirstIndex = FirstIndex;
			this.SecondIndex = SecondIndex;
		}
	}
}
