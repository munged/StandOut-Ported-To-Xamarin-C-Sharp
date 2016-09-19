using Java.Util;

namespace wei.mark.standout.ui
{

	/// <summary>
	/// This class holds temporal touch and gesture information. Mainly used to hold
	/// temporary data for onTouchEvent(MotionEvent).
	/// 
	/// @author Mark Wei <markwei@gmail.com>
	/// 
	/// </summary>
	public class TouchInfo
	{
		/// <summary>
		/// The state of the window.
		/// </summary>
		public int firstX, firstY, lastX, lastY;
		public double dist, scale, firstWidth, firstHeight;
		public float ratio;

		/// <summary>
		/// Whether we're past the move threshold already.
		/// </summary>
		public bool moving;

		public override string ToString()
		{
			return string.Format((string)Locale.Us, "WindowTouchInfo { firstX=%d, firstY=%d,lastX=%d, lastY=%d, firstWidth=%d, firstHeight=%d }", firstX, firstY, lastX, lastY, firstWidth, firstHeight);
		}
	}

}