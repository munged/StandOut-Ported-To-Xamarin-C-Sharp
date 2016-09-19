namespace wei.mark.standout.constants
{
	/// <summary>
	/// Flags to be returned from <seealso cref="StandOutWindow#getFlags(int)"/>.
	/// 
	/// @author Mark Wei markwei@gmail.com
	/// 
	/// </summary>
	public static class StandOutFlags
	{
		// This counter keeps track of which primary bit to set for each flag
		private static int flag_bit = 0;

		/// <summary>
		/// Setting this flag indicates that the window wants the system provided
		/// window decorations (titlebar, hide/close buttons, resize handle, etc).
		/// </summary>
		public static readonly int FLAG_DECORATION_SYSTEM = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window decorator should NOT provide
		/// a close button.
		/// 
		/// <para>
		/// This flag also sets <seealso cref="#FLAG_DECORATION_SYSTEM"/>.
		/// </para>
		/// </summary>
		public static readonly int FLAG_DECORATION_CLOSE_DISABLE = FLAG_DECORATION_SYSTEM | 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window decorator should NOT provide
		/// a resize handle.
		/// 
		/// <para>
		/// This flag also sets <seealso cref="#FLAG_DECORATION_SYSTEM"/>.
		/// </para>
		/// </summary>
		public static readonly int FLAG_DECORATION_RESIZE_DISABLE = FLAG_DECORATION_SYSTEM | 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window decorator should NOT provide
		/// a resize handle.
		/// 
		/// <para>
		/// This flag also sets <seealso cref="#FLAG_DECORATION_SYSTEM"/>.
		/// </para>
		/// </summary>
		public static readonly int FLAG_DECORATION_MAXIMIZE_DISABLE = FLAG_DECORATION_SYSTEM | 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window decorator should NOT provide
		/// a resize handle.
		/// 
		/// <para>
		/// This flag also sets <seealso cref="#FLAG_DECORATION_SYSTEM"/>.
		/// </para>
		/// </summary>
		public static readonly int FLAG_DECORATION_MOVE_DISABLE = FLAG_DECORATION_SYSTEM | 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window can be moved by dragging the
		/// body.
		/// 
		/// <para>
		/// Note that if <seealso cref="#FLAG_DECORATION_SYSTEM"/> is set, the window can
		/// always be moved by dragging the titlebar regardless of this flag.
		/// </para>
		/// </summary>
		public static readonly int FLAG_BODY_MOVE_ENABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that windows are able to be hidden, that
		/// <seealso cref="StandOutWindow#getHiddenIcon(int)"/>,
		/// <seealso cref="StandOutWindow#getHiddenTitle(int)"/>, and
		/// <seealso cref="StandOutWindow#getHiddenMessage(int)"/> are implemented, and that
		/// the system window decorator should provide a hide button if
		/// <seealso cref="#FLAG_DECORATION_SYSTEM"/> is set.
		/// </summary>
		public static readonly int FLAG_WINDOW_HIDE_ENABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window should be brought to the
		/// front upon user interaction.
		/// 
		/// <para>
		/// Note that if you set this flag, there is a noticeable flashing of the
		// window during <seealso cref="MotionEvent#ACTION_UP"/>. This the hack that allows
		/// the system to bring the window to the front.
		/// </para>
		/// </summary>
		public static readonly int FLAG_WINDOW_BRING_TO_FRONT_ON_TOUCH = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window should be brought to the
		/// front upon user tap.
		/// 
		/// <para>
		/// Note that if you set this flag, there is a noticeable flashing of the
		// window during <seealso cref="MotionEvent#ACTION_UP"/>. This the hack that allows
		/// the system to bring the window to the front.
		/// </para>
		/// </summary>
		public static readonly int FLAG_WINDOW_BRING_TO_FRONT_ON_TAP = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should keep the window's
		/// position within the edges of the screen. If this flag is not set, the
		/// window will be able to be dragged off of the screen.
		/// 
		/// <para>
		// If this flag is set, the window's <seealso cref="Gravity"/> is recommended to be
		// <seealso cref="Gravity#TOP"/> | <seealso cref="Gravity#LEFT"/>. If the gravity is anything
		/// other than TOP|LEFT, then even though the window will be displayed within
		/// the edges, it will behave as if the user can drag it off the screen.
		/// 
		/// </para>
		/// </summary>
		public static readonly int FLAG_WINDOW_EDGE_LIMITS_ENABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should keep the window's
		/// aspect ratio constant when resizing.
		/// 
		/// <para>
		/// The aspect ratio will only be enforced in
		/// <seealso cref="StandOutWindow#onTouchHandleResize(int, Window, View, MotionEvent)"/>
		/// . The aspect ratio will not be enforced if you set the width or height of
		/// the window's LayoutParams manually.
		/// 
		/// </para>
		/// </summary>
		//  StandOutWindow#onTouchHandleResize(int, Window, View, MotionEvent) 
		public static readonly int FLAG_WINDOW_ASPECT_RATIO_ENABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should resize the window when
		/// it detects a pinch-to-zoom gesture.
		/// </summary>
		// <seealso cref= Window#onInterceptTouchEvent(MotionEvent) </seealso>
		public static readonly int FLAG_WINDOW_PINCH_RESIZE_ENABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the window does not need focus. If this
		/// flag is set, the system will not take care of setting and unsetting the
		/// focus of windows based on user touch and key events.
		/// 
		/// <para>
		/// You will most likely need focus if your window contains any of the
		/// following: Button, ListView, EditText.
		/// 
		/// </para>
		/// <para>
		/// The benefit of disabling focus is that your window will not consume any
		/// key events. Normally, focused windows will consume the Back and Menu
		/// keys.
		/// 
		/// </para>
		/// </summary>
		// <seealso cref= <seealso cref="StandOutWindow#focus(int)"/> </seealso>
		// <seealso cref= <seealso cref="StandOutWindow#unfocus(int)"/>
		//  </seealso>
		public static readonly int FLAG_WINDOW_FOCUSABLE_DISABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should not change the
		/// window's visual state when focus is changed. If this flag is set, the
		/// implementation can choose to change the visual state in
		/// <seealso cref="StandOutWindow#onFocusChange(int, Window, boolean)"/>.
		/// </summary>
		// <seealso cref= <seealso cref="Window#onFocus(boolean)"/>
		//  </seealso>
		public static readonly int FLAG_WINDOW_FOCUS_INDICATOR_DISABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should disable all
		/// compatibility workarounds. The default behavior is to run
		// <seealso cref="Window#fixCompatibility(View, int)"/> on the view returned by the
		/// implementation.
		/// </summary>
		// <seealso cref= <seealso cref="Window#fixCompatibility(View, int)"/> </seealso>
		public static readonly int FLAG_FIX_COMPATIBILITY_ALL_DISABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should disable all additional
		/// functionality. The default behavior is to run
		// <seealso cref="Window#addFunctionality(View, int)"/> on the view returned by the
		/// implementation.
		/// </summary>
		// <seealso cref= <seealso cref="StandOutWindow#addFunctionality(View, int)"/> </seealso>
		public static readonly int FLAG_ADD_FUNCTIONALITY_ALL_DISABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should disable adding the
		/// resize handle additional functionality to a custom View R.id.corner.
		/// 
		/// <para>
		/// If <seealso cref="#FLAG_DECORATION_SYSTEM"/> is set, the user will always be able
		/// to resize the window with the default corner.
		/// 
		/// </para>
		/// </summary>
		// <seealso cref= <seealso cref="Window#addFunctionality(View, int)"/> </seealso>
		public static readonly int FLAG_ADD_FUNCTIONALITY_RESIZE_DISABLE = 1 << flag_bit++;

		/// <summary>
		/// Setting this flag indicates that the system should disable adding the
		/// drop down menu additional functionality to a custom View
		/// R.id.window_icon.
		/// 
		/// <para>
		/// If <seealso cref="#FLAG_DECORATION_SYSTEM"/> is set, the user will always be able
		/// to show the drop down menu with the default window icon.
		/// 
		/// </para>
		/// </summary>
		// <seealso cref= <seealso cref="Window#addFunctionality(View, int)"/> </seealso>
		public static readonly int FLAG_ADD_FUNCTIONALITY_DROP_DOWN_DISABLE = 1 << flag_bit++;
	}
}