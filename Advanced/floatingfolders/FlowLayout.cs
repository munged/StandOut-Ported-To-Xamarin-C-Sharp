using System;

namespace wei.mark.floatingfolders
	{

	using Context = Android.Content.Context;
	using AttributeSet = Android.Util.IAttributeSet;
	using View = Android.Views.View;
	using ViewGroup = Android.Views.ViewGroup;

	/// <summary>
	/// From http://www.superliminal.com/sources/FlowLayout.java.html
	/// 
	/// A view container with layout behavior like that of the Swing FlowLayout.
	/// Originally from
	/// http://nishantvnair.wordpress.com/2010/09/28/flowlayout-in-Android/
	/// 
	/// @author Melinda Green
	/// </summary>
	public class FlowLayout : ViewGroup
		{
		private const int PAD_H = 0, PAD_V = 0; // Space between child views.
		private int MHeight;
		private int MCols;

		public FlowLayout(Context context) : base(context)
			{
			}

		public FlowLayout(Context context, AttributeSet attrs) : base(context, attrs)
			{
			}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
		///	Assert(MeasureSpec.GetMode(widthMeasureSpec) != Android.Views.MeasureSpecMode.Unspecified);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = MeasureSpec.getSize(widthMeasureSpec) - getPaddingLeft() - getPaddingRight();
			int width = MeasureSpec.GetSize(widthMeasureSpec) - PaddingLeft-PaddingRight;
			int height = MeasureSpec.GetSize(heightMeasureSpec) - PaddingTop - PaddingBottom;
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final int count = getChildCount();
			int count = ChildCount;
			int xpos = PaddingLeft;
			int ypos = PaddingTop;
			int childHeightMeasureSpec;
			if (MeasureSpec.GetMode(heightMeasureSpec) == Android.Views.MeasureSpecMode.AtMost)
				{
				childHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(height, Android.Views.MeasureSpecMode.AtMost);
				}
			else
				{
				childHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(0, Android.Views.MeasureSpecMode.Unspecified);
				}
			MHeight = 0;
			int cols = 0;
			MCols = 0;
			for (int i = 0; i < count; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != Android.Views.ViewStates.Gone)
					{
					child.Measure(MeasureSpec.MakeMeasureSpec(width, Android.Views.MeasureSpecMode.AtMost), childHeightMeasureSpec);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childw = child.getMeasuredWidth();
					int childw = child.MeasuredWidth;
					MHeight = Math.Max(MHeight, child.MeasuredHeight + PAD_V);
					if (xpos + childw > width)
						{
						xpos = PaddingLeft;
						ypos += MHeight;
						MCols = Math.Max(MCols, cols);
						cols = 0;
						}
					else
						{
						cols++;
						}
					xpos += childw + PAD_H;
					}
				}

			MCols = Math.Max(MCols, cols);

			if (MeasureSpec.GetMode(heightMeasureSpec) == Android.Views.MeasureSpecMode.Unspecified)
				{
				height = ypos + MHeight;
				}
			else if (MeasureSpec.GetMode(heightMeasureSpec) == Android.Views.MeasureSpecMode.AtMost)
				{
				if (ypos + MHeight < height)
					{
					height = ypos + MHeight;
					}
				}
			height += 5; // Fudge to avoid clipping bottom of last row.
			SetMeasuredDimension(width, height);
			} // end onMeasure()

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = r - l;
			int width = r - l;
			int xpos = PaddingLeft;
			int ypos = PaddingTop;
			for (int i = 0; i < ChildCount; i++)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Android.view.View child = getChildAt(i);
				View child = GetChildAt(i);
				if (child.Visibility != Android.Views.ViewStates.Gone)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childw = child.getMeasuredWidth();
					int childw = child.MeasuredWidth;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int childh = child.getMeasuredHeight();
					int childh = child.MeasuredHeight;
					if (xpos + childw > width)
						{
						xpos = PaddingLeft;
						ypos += MHeight;
						}
					child.Layout(xpos, ypos, xpos + childw, ypos + childh);
					xpos += childw + PAD_H;
					}
				}
			} // end onLayout()

		protected internal virtual int GetCols()
			{
			return MCols;
			}

		protected internal virtual int GetChildHeight()
			{
			return MHeight;
			}
		}
	}