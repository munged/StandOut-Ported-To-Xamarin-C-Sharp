using System;
using System.Collections.Generic;

namespace wei.mark.standout.ui
{


	using StandOutLayoutParams = StandOutWindow.StandOutLayoutParams;
	using StandOutFlags = constants.StandOutFlags;
	using Context = Android.Content.Context;
	using Bundle = Android.OS.Bundle;
	using DisplayMetrics = Android.Util.DisplayMetrics;
	using Log = Android.Util.Log;
	using KeyEvent = Android.Views.KeyEvent;
	using LayoutInflater = Android.Views.LayoutInflater;
	using MotionEvent = Android.Views.MotionEvent;
	using View = Android.Views.View;
	using ViewGroup = Android.Views.ViewGroup;
	using FrameLayout = Android.Widget.FrameLayout;
	using ImageView = Android.Widget.ImageView;
	using PopupWindow = Android.Widget.PopupWindow;
	using TextView = Android.Widget.TextView;
	using R = library.Resource;
	using Android.Views;
	using Java.Lang;

	/// <summary>
	/// Special view that represents a floating window.
	/// 
	/// @author Mark Wei <markwei@gmail.com>
	/// 
	/// </summary>
	public class Window : FrameLayout
	{
		public const int VISIBILITY_GONE = 0;
		public const int VISIBILITY_VISIBLE = 1;
		public const int VISIBILITY_TRANSITION = 2;

		internal const string TAG = "Window";

		/// <summary>
		/// Class of the window, indicating which application the window belongs to.
		/// </summary>
		public Type cls;
		/// <summary>
		/// Id of the window.
		/// </summary>
		public int id;

		/// <summary>
		/// Whether the window is shown, hidden/closed, or in transition.
		/// </summary>
		public int visibility;

		/// <summary>
		/// Whether the window is focused.
		/// </summary>
		public bool focused;

		/// <summary>
		/// Original params from <seealso cref="StandOutWindow#getParams(int, Window)"/>.
		/// </summary>
		public StandOutWindow.StandOutLayoutParams originalParams;
		/// <summary>
		/// Original flags from <seealso cref="StandOutWindow#getFlags(int)"/>.
		/// </summary>
		public int flags;

		/// <summary>
		/// Touch information of the window.
		/// </summary>
		public TouchInfo touchInfo;

		/// <summary>
		/// Data attached to the window.
		/// </summary>
		public Bundle data;

		/// <summary>
		/// Width and height of the screen.
		/// </summary>
		internal int displayWidth, displayHeight;

		/// <summary>
		/// Context of the window.
		/// </summary>
		private readonly StandOutWindow mContext;
		private LayoutInflater mLayoutInflater;

		public Window(Context context) : base(context)
		{
			mContext = null;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: public Window(final wei.mark.standout.StandOutWindow context, final int id)
		public Window(StandOutWindow context, int id) : base(context)
		{
			context.SetTheme(context.ThemeStyle);

			mContext = context;
			mLayoutInflater = LayoutInflater.From(context);

			this.cls = context.GetType();
			this.id = id;
			this.originalParams = context.getParams(id, this);
			this.flags = context.getFlags(id);
			this.touchInfo = new TouchInfo();
			touchInfo.ratio = (float) originalParams.Width / originalParams.Height;
			this.data = new Bundle();
			DisplayMetrics metrics = mContext.Resources.DisplayMetrics;
			displayWidth = metrics.WidthPixels;
			displayHeight = (int)(metrics.HeightPixels - 25 * metrics.Density);

			// create the window contents
			View content;
			FrameLayout body;

			if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_SYSTEM))
			{
				// requested system window decorations
				content = SystemDecorations;
				body = (FrameLayout) content.FindViewById(R.Id.body);
			}
			else
			{
				// did not request decorations. will provide own implementation
				content = new FrameLayout(context);
				content.Id = R.Id.content;
				body = (FrameLayout) content;
			}

			AddView(content);

			//TODO
			body.SetOnTouchListener(new OnTouchListenerAnonymousInnerClassHelper(this, context, id));

			// attach the view corresponding to the id from the
			// implementation
			context.CreateAndAttachView(id, body);

			// make sure the implementation attached the view
			if (body.ChildCount == 0)
			{
				throw new Exception("You must attach your view to the given frame in createAndAttachView()");
			}

			// implement StandOut specific workarounds
			if (!Utils.isSet(flags, StandOutFlags.FLAG_FIX_COMPATIBILITY_ALL_DISABLE))
			{
				fixCompatibility(body);
			}
			// implement StandOut specific additional functionality
			if (!Utils.isSet(flags, StandOutFlags.FLAG_ADD_FUNCTIONALITY_ALL_DISABLE))
			{
				addFunctionality(body);
			}

			// attach the existing tag from the frame to the window
			Tag = body.Tag;
		}

		private class OnTouchListenerAnonymousInnerClassHelper :Java.Lang.Object, IOnTouchListener
		{
			private readonly Window outerInstance;

			private StandOutWindow context;
			private int id;

			public OnTouchListenerAnonymousInnerClassHelper(Window outerInstance, StandOutWindow context, int id)
			{
				this.outerInstance = outerInstance;
				this.context = context;
				this.id = id;
			}


			public  bool OnTouch(View v, MotionEvent @event)
			{
				// pass all touch events to the implementation
				bool consumed = false;

				// handle move and bring to front
				consumed = context.onTouchHandleMove(id, outerInstance, v, @event) || consumed;

				// alert implementation
				consumed = context.onTouchBody(id, outerInstance, v, @event) || consumed;

				return consumed;
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent @event)
		{
			StandOutLayoutParams @params = GetLayoutParams();

			// focus window
			if (@event.Action == MotionEventActions.Down)
			{
				if (mContext.FocusedWindow != this)
				{
					mContext.focus(id);
				}
			}

			// multitouch
			if (@event.PointerCount >= 2 && Utils.isSet(flags, StandOutFlags.FLAG_WINDOW_PINCH_RESIZE_ENABLE) && (@event.Action & MotionEventActions.Mask) == MotionEventActions.PointerDown)
			{
				touchInfo.scale = 1;
				touchInfo.dist = -1;
				touchInfo.firstWidth = @params.Width;
				touchInfo.firstHeight = @params.Height;
				return true;
			}

			return false;
		}

		public override bool OnTouchEvent(MotionEvent @event)
		{
			// handle touching outside
			switch (@event.Action)
			{
				case MotionEventActions.Outside:
					// unfocus window
					if (mContext.FocusedWindow == this)
					{
						mContext.unfocus(this);
					}

					// notify implementation that ACTION_OUTSIDE occurred
					mContext.onTouchBody(id, this, this, @event);
					break;
			}

			// handle multitouch
			if (@event.PointerCount >= 2 && Utils.isSet(flags, StandOutFlags.FLAG_WINDOW_PINCH_RESIZE_ENABLE))
			{
				// 2 fingers or more

				float x0 = @event.GetX(0);
				float y0 = @event.GetY(0);
				float x1 = @event.GetX(1);
				float y1 = @event.GetY(1);

				double dist = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));

				switch (@event.Action & MotionEventActions.Mask)
				{
					case MotionEventActions.Move:
						if (touchInfo.dist == -1)
						{
							touchInfo.dist = dist;
						}
						touchInfo.scale *= dist / touchInfo.dist;
						touchInfo.dist = dist;

						// scale the window with anchor point set to middle
						edit().setAnchorPoint(.5f,.5f).setSize((int)(touchInfo.firstWidth * touchInfo.scale), (int)(touchInfo.firstHeight * touchInfo.scale)).commit();
						break;
				}
				mContext.onResize(id, this, this, @event);
			}

			return true;
		}

		public override bool DispatchKeyEvent(KeyEvent @event)
		{
			if (mContext.onKeyEvent(id, this, @event))
			{
				Log.Debug(TAG, "Window " + id + " key event " + @event + " cancelled by implementation.");
				return false;
			}

			if (@event.Action == KeyEventActions.Up)
			{
				switch (@event.KeyCode)
				{
					case Keycode.Back:
						mContext.unfocus(this);
						return true;
				}
			}

			return base.DispatchKeyEvent(@event);
		}

		/// <summary>
		/// Request or remove the focus from this window.
		/// </summary>
		/// <param name="focus">
		///            Whether we want to gain or lose focus. </param>
		/// <returns> True if focus changed successfully, false if it failed. </returns>
		public virtual bool onFocus(bool focus)
		{
			if (!Utils.isSet(flags, StandOutFlags.FLAG_WINDOW_FOCUSABLE_DISABLE))
			{
				// window is focusable

				if (focus == focused)
				{
					// window already focused/unfocused
					return false;
				}

				focused = focus;

				// alert callbacks and cancel if instructed
				if (mContext.onFocusChange(id, this, focus))
				{
					Log.Debug(TAG, "Window " + id + " focus change " + (focus ? "(true)" : "(false)") + " cancelled by implementation.");
					focused = !focus;
					return false;
				}

				if (!Utils.isSet(flags, StandOutFlags.FLAG_WINDOW_FOCUS_INDICATOR_DISABLE))
				{
					// change visual state
					View content = FindViewById(R.Id.content);
					if (focus)
					{
						// gaining focus
						content.SetBackgroundResource(R.Drawable.border_focused);
					}
					else
					{
						// losing focus
						if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_SYSTEM))
						{
							// system decorations
							content.SetBackgroundResource(R.Drawable.border);
						}
						else
						{
							// no decorations
							content.SetBackgroundResource(0);
						}
					}
				}

				// set window manager params
				StandOutWindow.StandOutLayoutParams @params = GetLayoutParams();
				@params.FocusFlag = focus;
				mContext.updateViewLayout(id, @params);

				if (focus)
				{
					mContext.FocusedWindow = this;
				}
				else
				{
					if (mContext.FocusedWindow == this)
					{
						mContext.FocusedWindow = null;
					}
				}

				return true;
			}
			return false;
		}

		public void SetLayoutParams(ViewGroup.LayoutParams @params)
		{
			if (@params is StandOutWindow.StandOutLayoutParams)
			{
				base.LayoutParameters = @params;
			}
			else
			{
				throw new System.ArgumentException("Window" + id + ": LayoutParams must be an instance of StandOutLayoutParams.");
			}
		}

		/// <summary>
		/// Convenience method to start editting the size and position of this
		/// window. Make sure you call <seealso cref="Editor#commit()"/> when you are done to
		/// update the window.
		/// </summary>
		/// <returns> The Editor associated with this window. </returns>
		public virtual Editor edit()
		{
			return new Editor(this);
		}

		public StandOutWindow.StandOutLayoutParams GetLayoutParams()
		{
			StandOutWindow.StandOutLayoutParams @params = (StandOutWindow.StandOutLayoutParams) base.LayoutParameters;
			if (@params == null)
			{
				@params = originalParams;
			}
			return @params;
		}

		/// <summary>
		/// Returns the system window decorations if the implementation sets
		/// <seealso cref="#FLAG_DECORATION_SYSTEM"/>.
		/// 
		/// <para>
		/// The system window decorations support hiding, closing, moving, and
		/// resizing.
		/// 
		/// </para>
		/// </summary>
		/// <returns> The frame view containing the system window decorations. </returns>
		private View SystemDecorations
		{
			get
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final Android.view.View decorations = mLayoutInflater.inflate(wei.mark.standout.R.layout.system_window_decorators, null);
				View decorations = mLayoutInflater.Inflate(R.Layout.system_window_decorators, null);
    
				// icon
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final Android.widget.ImageView icon = (Android.widget.ImageView) decorations.FindViewById(wei.mark.standout.R.Id.window_icon);
				ImageView icon = (ImageView) decorations.FindViewById(R.Id.window_icon);
				icon.SetImageResource(mContext.AppIcon);
				icon.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this, icon));
    
				// title
				TextView title = (TextView) decorations.FindViewById(R.Id.title);
				title.Text = mContext.getTitle(id);
    
				// hide
				View hide = decorations.FindViewById(R.Id.hide);
				hide.SetOnClickListener( new OnClickListenerAnonymousInnerClassHelper2(this));
				hide.Visibility = ViewStates.Gone;
    
				// maximize
				View maximize = decorations.FindViewById(R.Id.maximize);
				maximize.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper3(this));
    
				// close
				View close = decorations.FindViewById(R.Id.close);
				close.SetOnClickListener (new OnClickListenerAnonymousInnerClassHelper4(this));
    
				// move
				View titlebar = decorations.FindViewById(R.Id.titlebar);
				titlebar.SetOnTouchListener(new OnTouchListenerAnonymousInnerClassHelper2(this));
    
				// resize
				View corner = decorations.FindViewById(R.Id.corner);
				corner.SetOnTouchListener(new OnTouchListenerAnonymousInnerClassHelper3(this));
    
				// set window appearance and behavior based on flags
				if (Utils.isSet(flags, StandOutFlags.FLAG_WINDOW_HIDE_ENABLE))
				{
					hide.Visibility = ViewStates.Visible;
				}
				if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_MAXIMIZE_DISABLE))
				{
					maximize.Visibility = ViewStates.Gone;
				}
				if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_CLOSE_DISABLE))
				{
					close.Visibility = ViewStates.Gone;
				}
				if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_MOVE_DISABLE))
				{
					titlebar.SetOnTouchListener(null);
				}
				if (Utils.isSet(flags, StandOutFlags.FLAG_DECORATION_RESIZE_DISABLE))
				{
					corner.Visibility = ViewStates.Gone;
				}
    
				return decorations;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper :Java.Lang.Object, IOnClickListener
		{
			private readonly Window outerInstance;

			private ImageView icon;

			public OnClickListenerAnonymousInnerClassHelper(Window outerInstance, ImageView icon)
			{
				this.outerInstance = outerInstance;
				this.icon = icon;
			}


			public void OnClick(View v)
			{
				PopupWindow dropDown = outerInstance.mContext.getDropDown(outerInstance.id);
				if (dropDown != null)
				{
					dropDown.ShowAsDropDown(icon);
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 :Java.Lang.Object, IOnClickListener
		{
			private readonly Window outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public  void OnClick(View v)
			{
				outerInstance.mContext.hide(outerInstance.id);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 :Java.Lang.Object, IOnClickListener
		{
			private readonly Window outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}
			public void OnClick(View v)
			{
				StandOutWindow.StandOutLayoutParams @params = outerInstance.GetLayoutParams();
				bool isMaximized = outerInstance.data.GetBoolean(WindowDataKeys.IS_MAXIMIZED);
				if (isMaximized && @params.Width == outerInstance.displayWidth && @params.Height == outerInstance.displayHeight && @params.X == 0 && @params.Y == 0)
				{
					outerInstance.data.PutBoolean(WindowDataKeys.IS_MAXIMIZED, false);
					int oldWidth = outerInstance.data.GetInt(WindowDataKeys.WIDTH_BEFORE_MAXIMIZE, -1);
					int oldHeight = outerInstance.data.GetInt(WindowDataKeys.HEIGHT_BEFORE_MAXIMIZE, -1);
					int oldX = outerInstance.data.GetInt(WindowDataKeys.X_BEFORE_MAXIMIZE, -1);
					int oldY = outerInstance.data.GetInt(WindowDataKeys.Y_BEFORE_MAXIMIZE, -1);
					outerInstance.edit().setSize(oldWidth, oldHeight).setPosition(oldX, oldY).commit();
				}
				else
				{
					outerInstance.data.PutBoolean(WindowDataKeys.IS_MAXIMIZED, true);
					outerInstance.data.PutInt(WindowDataKeys.WIDTH_BEFORE_MAXIMIZE, @params.Width);
					outerInstance.data.PutInt(WindowDataKeys.HEIGHT_BEFORE_MAXIMIZE, @params.Height);
					outerInstance.data.PutInt(WindowDataKeys.X_BEFORE_MAXIMIZE, @params.X);
					outerInstance.data.PutInt(WindowDataKeys.Y_BEFORE_MAXIMIZE, @params.Y);
					outerInstance.edit().setSize(1f, 1f).setPosition(0, 0).commit();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 :Java.Lang.Object, IOnClickListener
		{
			private readonly Window outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void OnClick(View v)
			{
				outerInstance.mContext.close(outerInstance.id);
			}
		}

		private class OnTouchListenerAnonymousInnerClassHelper2 :Java.Lang.Object, IOnTouchListener
		{
			private readonly Window outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper2(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public  bool OnTouch(View v, MotionEvent @event)
			{
				// handle dragging to move
				bool consumed = outerInstance.mContext.onTouchHandleMove(outerInstance.id, outerInstance, v, @event);
				return consumed;
			}
		}

		private class OnTouchListenerAnonymousInnerClassHelper3 :Java.Lang.Object, IOnTouchListener
		{
			private readonly Window outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper3(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}
			public bool OnTouch(View v, MotionEvent @event)
			{
				// handle dragging to move
				bool consumed = outerInstance.mContext.onTouchHandleResize(outerInstance.id, outerInstance, v, @event);
				return consumed;
			}
		}

		/// <summary>
		/// Implement StandOut specific additional functionalities.
		/// 
		/// <para>
		/// Currently, this method does the following:
		/// 
		/// </para>
		/// <para>
		/// Attach resize handles: For every View found to have id R.Id.corner,
		/// attach an OnTouchListener that implements resizing the window.
		/// 
		/// </para>
		/// </summary>
		/// <param name="root">
		///            The view hierarchy that is part of the window. </param>
		internal virtual void addFunctionality(View root)
		{
			// corner for resize
			if (!Utils.isSet(flags, StandOutFlags.FLAG_ADD_FUNCTIONALITY_RESIZE_DISABLE))
			{
				View corner = root.FindViewById(R.Id.corner);
				if (corner != null)
				{
					corner.SetOnTouchListener(new OnTouchListenerAnonymousInnerClassHelper4(this));
				}
			}

			// window_icon for drop down
			if (!Utils.isSet(flags, StandOutFlags.FLAG_ADD_FUNCTIONALITY_DROP_DOWN_DISABLE))
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Android.view.View icon = root.FindViewById(wei.mark.standout.R.Id.window_icon);
				View icon = root.FindViewById(R.Id.window_icon);
				if (icon != null)
				{
					icon.SetOnClickListener ( new OnClickListenerAnonymousInnerClassHelper5(this, icon));
				}
			}
		}

		private class OnTouchListenerAnonymousInnerClassHelper4 :Java.Lang.Object, IOnTouchListener
		{
			private readonly Window outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper4(Window outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public bool OnTouch(View v, MotionEvent @event)
			{
				// handle dragging to move
				bool consumed = outerInstance.mContext.onTouchHandleResize(outerInstance.id, outerInstance, v, @event);

				return consumed;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : Java.Lang.Object, IOnClickListener
		{
			private readonly Window outerInstance;

			private View icon;

			public OnClickListenerAnonymousInnerClassHelper5(Window outerInstance, View icon)
			{
				this.outerInstance = outerInstance;
				this.icon = icon;
			}


			public void OnClick(View v)
			{
				PopupWindow dropDown = outerInstance.mContext.getDropDown(outerInstance.id);
				if (dropDown != null)
				{
					dropDown.ShowAsDropDown(icon);
				}
			}
		
		}

		/// <summary>
		/// Iterate through each View in the view hiearchy and implement StandOut
		/// specific compatibility workarounds.
		/// 
		/// <para>
		/// Currently, this method does the following:
		/// 
		/// </para>
		/// <para>
		/// Nothing yet.
		/// 
		/// </para>
		/// </summary>
		/// <param name="root">
		///            The root view hierarchy to iterate through and check. </param>
		internal virtual void fixCompatibility(View root)
		{
			Java.Util.IQueue queue = (Java.Util.IQueue)new LinkedList<View>();
			queue.Add(root);

			View view = null;
			while ((view = (Android.Views.View)queue.Poll()) != null)
			{
				// do nothing yet

				// iterate through children
				if (view is ViewGroup)
				{
					ViewGroup group = (ViewGroup) view;
					for (int i = 0; i < group.ChildCount; i++)
					{
						queue.Add(group.GetChildAt(i));
					}
				}
			}
		}

		/// <summary>
		/// Convenient way to resize or reposition a Window. The Editor allows you to
		/// easily resize and reposition the window around anchor points.
		/// 
		/// @author Mark Wei <markwei@gmail.com>
		/// 
		/// </summary>
		public class Editor
		{
			private readonly Window outerInstance;

			/// <summary>
			/// Special value for width, height, x, or y positions that represents
			/// that the value should not be changed.
			/// </summary>
			public static readonly int UNCHANGED = int.MinValue;

			/// <summary>
			/// Layout params of the window associated with this Editor.
			/// </summary>
			internal StandOutWindow.StandOutLayoutParams mParams;

			/// <summary>
			/// The position of the anchor point as a percentage of the window's
			/// width/height. The anchor point is only used by the <seealso cref="Editor"/>.
			/// 
			/// <para>
			/// The anchor point effects the following methods:
			/// 
			/// </para>
			/// <para>
			/// <seealso cref="#setSize(float, float)"/>, <seealso cref="#setSize(int, int)"/>,
			/// <seealso cref="#setPosition(int, int)"/>, <seealso cref="#setPosition(int, int)"/>.
			/// 
			/// The window will move, expand, or shrink around the anchor point.
			/// 
			/// </para>
			/// <para>
			/// Values must be between 0 and 1, inclusive. 0 means the left/top, 0.5
			/// is the center, 1 is the right/bottom.
			/// </para>
			/// </summary>
			internal float anchorX, anchorY;

			public Editor(Window outerInstance)
			{
				this.outerInstance = outerInstance;
				mParams = outerInstance.GetLayoutParams();
				anchorX = anchorY = 0;
			}

			public virtual Editor setAnchorPoint(float x, float y)
			{
				if (x < 0 || x > 1 || y < 0 || y > 1)
				{
					throw new System.ArgumentException("Anchor point must be between 0 and 1, inclusive.");
				}

				anchorX = x;
				anchorY = y;

				return this;
			}

			/// <summary>
			/// Set the size of this window as percentages of max screen size. The
			/// window will expand and shrink around the top-left corner, unless
			/// you've set a different anchor point with
			/// <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="percentWidth"> </param>
			/// <param name="percentHeight"> </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			public virtual Editor setSize(float percentWidth, float percentHeight)
			{
				return setSize((int)(outerInstance.displayWidth * percentWidth), (int)(outerInstance.displayHeight * percentHeight));
			}

			/// <summary>
			/// Set the size of this window in absolute pixels. The window will
			/// expand and shrink around the top-left corner, unless you've set a
			/// different anchor point with <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="width"> </param>
			/// <param name="height"> </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			public virtual Editor setSize(int width, int height)
			{
				return setSize(width, height, false);
			}

			/// <summary>
			/// Set the size of this window in absolute pixels. The window will
			/// expand and shrink around the top-left corner, unless you've set a
			/// different anchor point with <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="width"> </param>
			/// <param name="height"> </param>
			/// <param name="skip">
			///            Don't call <seealso cref="#setPosition(int, int)"/> to avoid stack
			///            overflow. </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			internal virtual Editor setSize(int width, int height, bool skip)
			{
				if (mParams != null)
				{
					if (anchorX < 0 || anchorX > 1 || anchorY < 0 || anchorY > 1)
					{
						throw new IllegalStateException("Anchor point must be between 0 and 1, inclusive.");
					}

					int lastWidth = mParams.Width;
					int lastHeight = mParams.Height;

					if (width != UNCHANGED)
					{
						mParams.Width = width;
					}
					if (height != UNCHANGED)
					{
						mParams.Height = height;
					}

					// set max width/height
					int maxWidth = mParams.maxWidth;
					int maxHeight = mParams.maxHeight;

					if (Utils.isSet(outerInstance.flags, StandOutFlags.FLAG_WINDOW_EDGE_LIMITS_ENABLE))
					{
						maxWidth = (int) Math.Min(maxWidth, outerInstance.displayWidth);
						maxHeight = (int) Math.Min(maxHeight, outerInstance.displayHeight);
					}

					// keep window between min and max
					mParams.Width = Math.Min(Math.Max(mParams.Width, mParams.minWidth), maxWidth);
					mParams.Height = Math.Min(Math.Max(mParams.Height, mParams.minHeight), maxHeight);

					// keep window in aspect ratio
					if (Utils.isSet(outerInstance.flags, StandOutFlags.FLAG_WINDOW_ASPECT_RATIO_ENABLE))
					{
						int ratioWidth = (int)(mParams.Height * outerInstance.touchInfo.ratio);
						int ratioHeight = (int)(mParams.Width / outerInstance.touchInfo.ratio);
						if (ratioHeight >= mParams.minHeight && ratioHeight <= mParams.maxHeight)
						{
							// width good adjust height
							mParams.Height = ratioHeight;
						}
						else
						{
							// height good adjust width
							mParams.Width = ratioWidth;
						}
					}

					if (!skip)
					{
						// set position based on anchor point
						setPosition((int)(mParams.X + lastWidth * anchorX), (int)(mParams. Y+ lastHeight * anchorY));
					}
				}

				return this;
			}

			/// <summary>
			/// Set the position of this window as percentages of max screen size.
			/// The window's top-left corner will be positioned at the given x and y,
			/// unless you've set a different anchor point with
			/// <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="percentWidth"> </param>
			/// <param name="percentHeight"> </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			public virtual Editor setPosition(float percentWidth, float percentHeight)
			{
				return setPosition((int)(outerInstance.displayWidth * percentWidth), (int)(outerInstance.displayHeight * percentHeight));
			}

			/// <summary>
			/// Set the position of this window in absolute pixels. The window's
			/// top-left corner will be positioned at the given x and y, unless
			/// you've set a different anchor point with
			/// <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="x"> </param>
			/// <param name="y"> </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			public virtual Editor setPosition(int x, int y)
			{
				return setPosition(x, y, false);
			}

			/// <summary>
			/// Set the position of this window in absolute pixels. The window's
			/// top-left corner will be positioned at the given x and y, unless
			/// you've set a different anchor point with
			/// <seealso cref="#setAnchorPoint(float, float)"/>.
			/// 
			/// Changes will not applied until you <seealso cref="#commit()"/>.
			/// </summary>
			/// <param name="x"> </param>
			/// <param name="y"> </param>
			/// <param name="skip">
			///            Don't call <seealso cref="#setPosition(int, int)"/> and
			///            <seealso cref="#setSize(int, int)"/> to avoid stack overflow. </param>
			/// <returns> The same Editor, useful for method chaining. </returns>
			internal virtual Editor setPosition(int x, int y, bool skip)
			{
				if (mParams != null)
				{
					if (anchorX < 0 || anchorX > 1 || anchorY < 0 || anchorY > 1)
					{
						throw new IllegalStateException("Anchor point must be between 0 and 1, inclusive.");
					}

					// sets the x and y correctly according to anchorX and
					// anchorY
					if (x != UNCHANGED)
					{
						mParams.X = (int)(x - mParams.Width * anchorX);
					}
					if (y != UNCHANGED)
					{
						mParams.Y = (int)(y - mParams.Height * anchorY);
					}

					if (Utils.isSet(outerInstance.flags, StandOutFlags.FLAG_WINDOW_EDGE_LIMITS_ENABLE))
					{
						// if gravity is not TOP|LEFT throw exception
						if (mParams.Gravity != (GravityFlags.Top | GravityFlags.Left))
						{
							throw new IllegalStateException("The window " + outerInstance.id + " gravity must be TOP|LEFT if FLAG_WINDOW_EDGE_LIMITS_ENABLE or FLAG_WINDOW_EDGE_TILE_ENABLE is set.");
						}

						// keep window inside edges
						mParams.X = Math.Min(Math.Max(mParams.X, 0), outerInstance.displayWidth - mParams.Width);
						mParams.Y = Math.Min(Math.Max(mParams.Y, 0), outerInstance.displayHeight - mParams.Height);
					}
				}

				return this;
			}

			/// <summary>
			/// Commit the changes to this window. Updates the layout. This Editor
			/// cannot be used after you commit.
			/// </summary>
			public virtual void commit()
			{
				if (mParams != null)
				{
					outerInstance.mContext.updateViewLayout(outerInstance.id, mParams);
					mParams = null;
				}
			}
		}

		public class WindowDataKeys
		{
			public const string IS_MAXIMIZED = "isMaximized";
			public const string WIDTH_BEFORE_MAXIMIZE = "widthBeforeMaximize";
			public const string HEIGHT_BEFORE_MAXIMIZE = "heightBeforeMaximize";
			public const string X_BEFORE_MAXIMIZE = "xBeforeMaximize";
			public const string Y_BEFORE_MAXIMIZE = "yBeforeMaximize";
		}
	}
}