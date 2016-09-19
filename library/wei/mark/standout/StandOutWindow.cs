using System.Threading;
using Java.Util;
using wei.mark.standout;

namespace wei.mark.standout
{


	using StandOutFlags = wei.mark.standout.constants.StandOutFlags;
	using Window = wei.mark.standout.ui.Window;
	using Notification = Android.App.Notification;
	using NotificationManager = Android.App.NotificationManager;
	using PendingIntent = Android.App.PendingIntent;
	using Service = Android.App.Service;
	using Context = Android.Content.Context;
	using Intent = Android.Content.Intent;
	using PixelFormat = Android.Graphics.PixelFormat;
	using Drawable = Android.Graphics.Drawables.Drawable;
	using Uri = Android.Net.Uri;
	using Bundle = Android.OS.Bundle;
	using IBinder = Android.OS.IBinder;
	using Log = Android.Util.Log;
	using Display = Android.Views.Display;
	using Gravity = Android.Views.Gravity;
	using KeyEvent = Android.Views.KeyEvent;
	using LayoutInflater = Android.Views.LayoutInflater;
	using MotionEvent = Android.Views.MotionEvent;
	using View = Android.Views.View;
	using OnClickListener = Android.Views.View.IOnClickListener;
	using ViewGroup = Android.Views.ViewGroup;
	using WindowManager = Android.Views.IWindowManager;
	using Animation = Android.Views.Animations.Animation;
	using AnimationListener = Android.Views.Animations.Animation.IAnimationListener;
	using AnimationUtils = Android.Views.Animations.AnimationUtils;
	using FrameLayout = Android.Widget.FrameLayout;
	using ImageView = Android.Widget.ImageView;
	using LinearLayout = Android.Widget.LinearLayout;
	using PopupWindow = Android.Widget.PopupWindow;
	using TextView = Android.Widget.TextView;
	using Java.Lang;
	using System.Collections.Generic;
	using Android.Views;
	using System;
	using Android;
	using Android.Graphics;
	using Android.Views.Animations;
	using Android.App;

	/// <summary>
	/// Extend this class to easily create and manage floating StandOut windows.
	/// 
	/// @author Mark Wei <markwei@gmail.com>
	/// 
	///         Contributors: Jason <github.com/jasonconnery>
	/// 
	/// </summary>
	public abstract class StandOutWindow : Service
	{
		internal const string TAG = "StandOutWindow";

		/// <summary>
		/// StandOut window id: You may use this sample id for your first window.
		/// </summary>
		public const int DEFAULT_ID = 0;

		/// <summary>
		/// Special StandOut window id: You may NOT use this id for any windows.
		/// </summary>
		public const int ONGOING_NOTIFICATION_ID = -1;

		/// <summary>
		/// StandOut window id: You may use this id when you want it to be
		/// disregarded. The system makes no distinction for this id; it is only used
		/// to improve code readability.
		/// </summary>
		public const int DISREGARD_ID = -2;

		/// <summary>
		/// Intent action: Show a new window corresponding to the id.
		/// </summary>
		public const string ACTION_SHOW = "SHOW";

		/// <summary>
		/// Intent action: Restore a previously hidden window corresponding to the
		/// id. The window should be previously hidden with <seealso cref="#ACTION_HIDE"/>.
		/// </summary>
		public const string ACTION_RESTORE = "RESTORE";

		/// <summary>
		/// Intent action: Close an existing window with an existing id.
		/// </summary>
		public const string ACTION_CLOSE = "CLOSE";

		/// <summary>
		/// Intent action: Close all existing windows.
		/// </summary>
		public const string ACTION_CLOSE_ALL = "CLOSE_ALL";

		/// <summary>
		/// Intent action: Send data to a new or existing window.
		/// </summary>
		public const string ACTION_SEND_DATA = "SEND_DATA";

		/// <summary>
		/// Intent action: Hide an existing window with an existing id. To enable the
		/// ability to restore this window, make sure you implement
		/// <seealso cref="#getHiddenNotification(int)"/>.
		/// </summary>
		public const string ACTION_HIDE = "HIDE";

		/// <summary>
		/// Show a new window corresponding to the id, or restore a previously hidden
		/// window.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that will be used
		///            to create and manage the window. </param>
		/// <param name="id">
		///            The id representing this window. If the id exists, and the
		///            corresponding window was previously hidden, then that window
		///            will be restored.
		/// </param>
		/// <seealso cref= #show(int) </seealso>
		public static void show(Context context, Type cls, int id)
		{
			context.StartService(getShowIntent(context, cls, id));
		}

		/// <summary>
		/// Hide the existing window corresponding to the id. To enable the ability
		/// to restore this window, make sure you implement
		/// <seealso cref="#getHiddenNotification(int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <param name="id">
		///            The id representing this window. The window must previously be
		///            shown. </param>
		/// <seealso cref= #hide(int) </seealso>
		public static void hide(Context context, Type cls, int id)
		{
			context.StartService(getHideIntent(context, cls, id));
		}

		/// <summary>
		/// Close an existing window with an existing id.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <param name="id">
		///            The id representing this window. The window must previously be
		///            shown. </param>
		/// <seealso cref= #close(int) </seealso>
		public static void close(Context context, Type cls, int id)
		{
			context.StartService(getCloseIntent(context, cls, id));
		}

		/// <summary>
		/// Close all existing windows.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <seealso cref= #closeAll() </seealso>
		public static void closeAll(Context context, Type cls)
		{
			context.StartService(getCloseAllIntent(context, cls));
		}

		/// <summary>
		/// This allows windows of different applications to communicate with each
		/// other.
		/// 
		/// <para>
		/// Send <seealso cref="Parceleable"/> data in a <seealso cref="Bundle"/> to a new or existing
		/// windows. The implementation of the recipient window can handle what to do
		/// with the data. To receive a result, provide the class and id of the
		/// sender.
		/// 
		/// </para>
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing the class of
		///            the sending window. </param>
		/// <param name="toCls">
		///            The Service's class extending <seealso cref="StandOutWindow"/> that is
		///            managing the receiving window. </param>
		/// <param name="toId">
		///            The id of the receiving window, or DISREGARD_ID. </param>
		/// <param name="requestCode">
		///            Provide a request code to declare what kind of data is being
		///            sent. </param>
		/// <param name="data">
		///            A bundle of parceleable data to be sent to the receiving
		///            window. </param>
		/// <param name="fromCls">
		///            Provide the class of the sending window if you want a result. </param>
		/// <param name="fromId">
		///            Provide the id of the sending window if you want a result. </param>
		/// <seealso cref= #sendData(int, Class, int, int, Bundle) </seealso>
		public static void sendData(Context context, Type toCls, int toId, int requestCode, Bundle data, Type fromCls, int fromId)
		{
			context.StartService(getSendDataIntent(context, toCls, toId, requestCode, data, fromCls, fromId));
		}

		/// <summary>
		/// See <seealso cref="#show(Context, Class, int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that will be used
		///            to create and manage the window. </param>
		/// <param name="id">
		///            The id representing this window. If the id exists, and the
		///            corresponding window was previously hidden, then that window
		///            will be restored. </param>
		/// <returns> An <seealso cref="Intent"/> to use with
		///         <seealso cref="Context#startService(Intent)"/>. </returns>
		public static Intent getShowIntent(Context context,System.Type cls, int id)
		{
			bool cached = sWindowCache.IsCached(id, cls);
			string action = cached ? ACTION_RESTORE : ACTION_SHOW;
			Uri uri = cached ? Uri.Parse("standout://" + cls + '/' + id) : null;
			return (new Intent(context, cls)).PutExtra("id", id).SetAction(action).SetData(uri);
		}

		/// <summary>
		/// See <seealso cref="#hide(Context, Class, int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <param name="id">
		///            The id representing this window. If the id exists, and the
		///            corresponding window was previously hidden, then that window
		///            will be restored. </param>
		/// <returns> An <seealso cref="Intent"/> to use with
		///         <seealso cref="Context#startService(Intent)"/>. </returns>
		public static Intent getHideIntent(Context context, System.Type cls, int id)
		{
			return (new Intent(context, cls)).PutExtra("id", id).SetAction(ACTION_HIDE);
		}

		/// <summary>
		/// See <seealso cref="#close(Context, Class, int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <param name="id">
		///            The id representing this window. If the id exists, and the
		///            corresponding window was previously hidden, then that window
		///            will be restored. </param>
		/// <returns> An <seealso cref="Intent"/> to use with
		///         <seealso cref="Context#startService(Intent)"/>. </returns>
		public static Intent getCloseIntent(Context context, System.Type cls, int id)
		{
			return (new Intent(context, cls)).PutExtra("id", id).SetAction(ACTION_CLOSE);
		}

		/// <summary>
		/// See <seealso cref="#closeAll(Context, Class, int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing this class. </param>
		/// <param name="cls">
		///            The Service extending <seealso cref="StandOutWindow"/> that is managing
		///            the window. </param>
		/// <returns> An <seealso cref="Intent"/> to use with
		///         <seealso cref="Context#startService(Intent)"/>. </returns>
		public static Intent getCloseAllIntent(Context context, System.Type cls)
		{
			return (new Intent(context, cls)).SetAction(ACTION_CLOSE_ALL);
		}

		/// <summary>
		/// See <seealso cref="#sendData(Context, Class, int, int, Bundle, Class, int)"/>.
		/// </summary>
		/// <param name="context">
		///            A Context of the application package implementing the class of
		///            the sending window. </param>
		/// <param name="toCls">
		///            The Service's class extending <seealso cref="StandOutWindow"/> that is
		///            managing the receiving window. </param>
		/// <param name="toId">
		///            The id of the receiving window. </param>
		/// <param name="requestCode">
		///            Provide a request code to declare what kind of data is being
		///            sent. </param>
		/// <param name="data">
		///            A bundle of parceleable data to be sent to the receiving
		///            window. </param>
		/// <param name="fromCls">
		///            If the sending window wants a result, provide the class of the
		///            sending window. </param>
		/// <param name="fromId">
		///            If the sending window wants a result, provide the id of the
		///            sending window. </param>
		/// <returns> An <seealso cref="Intnet"/> to use with
		///         <seealso cref="Context#startService(Intent)"/>. </returns>
		public static Intent getSendDataIntent(Context context, System.Type toCls, int toId, int requestCode, Bundle data, Type fromCls, int fromId)
		{
			return (new Intent(context, toCls)).PutExtra("id", toId).PutExtra("requestCode", requestCode).PutExtra("wei.mark.standout.data", data).PutExtra("wei.mark.standout.fromCls",fromCls.ToString()).PutExtra("fromId", fromId).SetAction(ACTION_SEND_DATA);
		}

		// internal map of ids to shown/hidden views
		internal static WindowCache sWindowCache;
		internal static Window sFocusedWindow;

		// static constructors
		static StandOutWindow()
		{
			sWindowCache = new WindowCache();
			sFocusedWindow = null;
		}

		// internal system services
		internal WindowManager mWindowManager;
		private NotificationManager mNotificationManager;
		internal LayoutInflater mLayoutInflater;

		// internal state variables
		private bool startedForeground;

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override void OnCreate()
		{
			base.OnCreate();

			mWindowManager =(IWindowManager) (GetSystemService(WindowService));
			mNotificationManager = (NotificationManager) GetSystemService(Context.NotificationService);
			mLayoutInflater = (LayoutInflater) GetSystemService(Context.LayoutInflaterService);

			startedForeground = false;
		}
		public override Android.App.StartCommandResult OnStartCommand(Intent intent, Android.App.StartCommandFlags flags, int startId)
		{
			base.OnStartCommand(intent, flags, startId);

			// intent should be created with
			// getShowIntent(), getHideIntent(), getCloseIntent()
			if (intent != null)
			{
				string action = intent.Action;
				int id = intent.GetIntExtra("id", DEFAULT_ID);

				// this will interfere with getPersistentNotification()
				if (id == ONGOING_NOTIFICATION_ID)
				{
					throw new Java.Lang.Exception("ID cannot equals StandOutWindow.ONGOING_NOTIFICATION_ID");
				}

				if (ACTION_SHOW.Equals(action) || ACTION_RESTORE.Equals(action))
				{
					show(id);
				}
				else if (ACTION_HIDE.Equals(action))
				{
					hide(id);
				}
				else if (ACTION_CLOSE.Equals(action))
				{
					close(id);
				}
				else if (ACTION_CLOSE_ALL.Equals(action))
				{
					closeAll();
				}
				else if (ACTION_SEND_DATA.Equals(action))
				{
					if (!isExistingId(id) && id != DISREGARD_ID)
					{
						Log.Warn(TAG, "Sending data to non-existant window. If this is not intended, make sure toId is either an existing window's id or DISREGARD_ID.");
					}
					Bundle data = intent.GetBundleExtra("wei.mark.standout.data");
					int requestCode = intent.GetIntExtra("requestCode", 0);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Class fromCls = (Class) intent.getSerializableExtra("wei.mark.standout.fromCls");
					Type fromCls = (Type) intent.GetSerializableExtra("wei.mark.standout.fromCls");
					int fromId = intent.GetIntExtra("fromId", DEFAULT_ID);
					onReceiveData(id, requestCode, data, fromCls, fromId);
				}
			}
			else
			{
				Log.Warn(TAG, "Tried to onStartCommand() with a null intent.");
			}

			// the service is started in foreground in show()
			// so we don't expect Android to kill this service
			return Android.App.StartCommandResult.NotSticky;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			// closes all windows
			closeAll();
		}

		/// <summary>
		/// Return the name of every window in this implementation. The name will
		/// appear in the default implementations of the system window decoration
		/// title and notification titles.
		/// </summary>
		/// <returns> The name. </returns>
		public abstract string AppName {get;}

		/// <summary>
		/// Return the icon resource for every window in this implementation. The
		/// icon will appear in the default implementations of the system window
		/// decoration and notifications.
		/// </summary>
		/// <returns> The icon. </returns>
		public abstract int AppIcon {get;}

		/// <summary>
		/// Create a new <seealso cref="View"/> corresponding to the id, and add it as a child
		/// to the frame. The view will become the contents of this StandOut window.
		/// The view MUST be newly created, and you MUST attach it to the frame.
		/// 
		/// <para>
		/// If you are inflating your view from XML, make sure you use
		/// <seealso cref="LayoutInflater#inflate(int, ViewGroup, boolean)"/> to attach your
		/// view to frame. Set the ViewGroup to be frame, and the boolean to true.
		/// 
		/// </para>
		/// <para>
		/// If you are creating your view programmatically, make sure you use
		/// <seealso cref="FrameLayout#addView(View)"/> to add your view to the frame.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id representing the window. </param>
		/// <param name="frame">
		///            The <seealso cref="FrameLayout"/> to attach your view as a child to. </param>
		public abstract void CreateAndAttachView(int id, FrameLayout frame);

		/// <summary>
		/// Return the <seealso cref="StandOutWindow#LayoutParams"/> for the corresponding id.
		/// The system will set the layout params on the view for this StandOut
		/// window. The layout params may be reused.
		/// 
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <param name="window">
		///            The window corresponding to the id. Given as courtesy, so you
		///            may get the existing layout params. </param>
		/// <returns> The <seealso cref="StandOutWindow#LayoutParams"/> corresponding to the id.
		///         The layout params will be set on the window. The layout params
		///         returned will be reused whenever possible, minimizing the number
		///         of times getParams() will be called. </returns>
		public abstract StandOutLayoutParams getParams(int id, Window window);

		/// <summary>
		/// Implement this method to change modify the behavior and appearance of the
		/// window corresponding to the id.
		/// 
		/// <para>
		/// You may use any of the flags defined in <seealso cref="StandOutFlags"/>. This
		/// method will be called many times, so keep it fast.
		/// 
		/// </para>
		/// <para>
		/// Use bitwise OR (|) to set flags, and bitwise XOR (^) to unset flags. To
		/// test if a flag is set, use <seealso cref="Utils#isSet(int, int)"/>.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> A combination of flags. </returns>
		public virtual int getFlags(int id)
		{
			return 0;
		}

		/// <summary>
		/// Implement this method to set a custom title for the window corresponding
		/// to the id.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The title of the window. </returns>
		public virtual string getTitle(int id)
		{
			return AppName;
		}

		/// <summary>
		/// Implement this method to set a custom icon for the window corresponding
		/// to the id.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The icon of the window. </returns>
		public virtual int getIcon(int id)
		{
			return AppIcon;
		}

		/// <summary>
		/// Return the title for the persistent notification. This is called every
		/// time <seealso cref="#show(int)"/> is called.
		/// </summary>
		/// <param name="id">
		///            The id of the window shown. </param>
		/// <returns> The title for the persistent notification. </returns>
		public virtual string getPersistentNotificationTitle(int id)
		{
			return AppName + " Running";
		}

		/// <summary>
		/// Return the message for the persistent notification. This is called every
		/// time <seealso cref="#show(int)"/> is called.
		/// </summary>
		/// <param name="id">
		///            The id of the window shown. </param>
		/// <returns> The message for the persistent notification. </returns>
		public virtual string getPersistentNotificationMessage(int id)
		{
			return "";
		}

		/// <summary>
		/// Return the intent for the persistent notification. This is called every
		/// time <seealso cref="#show(int)"/> is called.
		/// 
		/// <para>
		/// The returned intent will be packaged into a <seealso cref="PendingIntent"/> to be
		/// invoked when the user clicks the notification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the window shown. </param>
		/// <returns> The intent for the persistent notification. </returns>
		public virtual Intent getPersistentNotificationIntent(int id)
		{
			return null;
		}

		/// <summary>
		/// Return the icon resource for every hidden window in this implementation.
		/// The icon will appear in the default implementations of the hidden
		/// notifications.
		/// </summary>
		/// <returns> The icon. </returns>
		public virtual int HiddenIcon
		{
			get
			{
				return AppIcon;
			}
		}

		/// <summary>
		/// Return the title for the hidden notification corresponding to the window
		/// being hidden.
		/// </summary>
		/// <param name="id">
		///            The id of the hidden window. </param>
		/// <returns> The title for the hidden notification. </returns>
		public virtual string getHiddenNotificationTitle(int id)
		{
			return AppName + " Hidden";
		}

		/// <summary>
		/// Return the message for the hidden notification corresponding to the
		/// window being hidden.
		/// </summary>
		/// <param name="id">
		///            The id of the hidden window. </param>
		/// <returns> The message for the hidden notification. </returns>
		public virtual string getHiddenNotificationMessage(int id)
		{
			return "";
		}

		/// <summary>
		/// Return the intent for the hidden notification corresponding to the window
		/// being hidden.
		/// 
		/// <para>
		/// The returned intent will be packaged into a <seealso cref="PendingIntent"/> to be
		/// invoked when the user clicks the notification.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the hidden window. </param>
		/// <returns> The intent for the hidden notification. </returns>
		public virtual Intent getHiddenNotificationIntent(int id)
		{
			return null;
		}

		/// <summary>
		/// Return a persistent <seealso cref="Notification"/> for the corresponding id. You
		/// must return a notification for AT LEAST the first id to be requested.
		/// Once the persistent notification is shown, further calls to
		/// <seealso cref="#getPersistentNotification(int)"/> may return null. This way Android
		/// can start the StandOut window service in the foreground and will not kill
		/// the service on low memory.
		/// 
		/// <para>
		/// As a courtesy, the system will request a notification for every new id
		/// shown. Your implementation is encouraged to include the
		/// <seealso cref="PendingIntent#FLAG_UPDATE_CURRENT"/> flag in the notification so
		/// that there is only one system-wide persistent notification.
		/// 
		/// </para>
		/// <para>
		/// See the StandOutExample project for an implementation of
		/// <seealso cref="#getPersistentNotification(int)"/> that keeps one system-wide
		/// persistent notification that creates a new window on every click.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The <seealso cref="Notification"/> corresponding to the id, or null if
		///         you've previously returned a notification. </returns>
		public virtual Notification getPersistentNotification(int id)
		{
			// basic notification stuff
			// http://developer.Android.com/guide/topics/ui/notifiers/notifications.html
			int icon = AppIcon;
			long when = DateTimeHelperClass.CurrentUnixTimeMillis();
			Context c = ApplicationContext;
			string contentTitle = getPersistentNotificationTitle(id);
			string contentText = getPersistentNotificationMessage(id);
			string tickerText = string.Format("{0}: {1}", contentTitle, contentText);

			// getPersistentNotification() is called for every new window
			// so we replace the old notification with a new one that has
			// a bigger id
			Intent notificationIntent = getPersistentNotificationIntent(id);

			PendingIntent contentIntent = null;

			if (notificationIntent != null)
			{
				contentIntent = PendingIntent.GetService(this, 0, notificationIntent, Android.App.PendingIntentFlags.UpdateCurrent);
						// flag updates existing persistent notification
			}

			Notification notification = new Notification(icon, tickerText, when);
			notification.SetLatestEventInfo(c, contentTitle, contentText, contentIntent);
			return notification;
		}

		/// <summary>
		/// Return a hidden <seealso cref="Notification"/> for the corresponding id. The system
		/// will request a notification for every id that is hidden.
		/// 
		/// <para>
		/// If null is returned, StandOut will assume you do not wish to support
		/// hiding this window, and will <seealso cref="#close(int)"/> it for you.
		/// 
		/// </para>
		/// <para>
		/// See the StandOutExample project for an implementation of
		/// <seealso cref="#getHiddenNotification(int)"/> that for every hidden window keeps a
		/// notification which restores that window upon user's click.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The <seealso cref="Notification"/> corresponding to the id or null. </returns>
		public virtual Notification getHiddenNotification(int id)
		{
			// same basics as getPersistentNotification()
			int icon = HiddenIcon;
			long when = DateTimeHelperClass.CurrentUnixTimeMillis();
			Context c = ApplicationContext;
			string contentTitle = getHiddenNotificationTitle(id);
			string contentText = getHiddenNotificationMessage(id);
			string tickerText = string.Format("{0}: {1}", contentTitle, contentText);

			// the difference here is we are providing the same id
			Intent notificationIntent = getHiddenNotificationIntent(id);

			PendingIntent contentIntent = null;

			if (notificationIntent != null)
			{
				contentIntent = PendingIntent.GetService(this, 0, notificationIntent, Android.App.PendingIntentFlags.UpdateCurrent);
						// flag updates existing persistent notification
			}

			Notification notification = new Notification(icon, tickerText, when);
			notification.SetLatestEventInfo(c, contentTitle, contentText, contentIntent);
			return notification;
		}

		/// <summary>
		/// Return the animation to play when the window corresponding to the id is
		/// shown.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The animation to play or null. </returns>
		public virtual Animation getShowAnimation(int id)
		{
			return AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeIn);
		}

		/// <summary>
		/// Return the animation to play when the window corresponding to the id is
		/// hidden.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The animation to play or null. </returns>
		public virtual Animation getHideAnimation(int id)
		{
			return AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeOut);
		}

		/// <summary>
		/// Return the animation to play when the window corresponding to the id is
		/// closed.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The animation to play or null. </returns>
		public virtual Animation getCloseAnimation(int id)
		{
			return AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeOut);
		}

		/// <summary>
		/// Implement this method to set a custom theme for all windows in this
		/// implementation.
		/// </summary>
		/// <returns> The theme to set on the window, or 0 for device default. </returns>
		public virtual int ThemeStyle
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// You probably want to leave this method alone and implement
		/// <seealso cref="#getDropDownItems(int)"/> instead. Only implement this method if you
		/// want more control over the drop down menu.
		/// 
		/// <para>
		/// Implement this method to set a custom drop down menu when the user clicks
		/// on the icon of the window corresponding to the id. The icon is only shown
		/// when <seealso cref="StandOutFlags#FLAG_DECORATION_SYSTEM"/> is set.
		/// 
		/// </para>
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The drop down menu to be anchored to the icon, or null to have no
		///         dropdown menu. </returns>
		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
		//ORIGINAL LINE: public Android.widget.PopupWindow getDropDown(final int id)
		public virtual PopupWindow getDropDown(int id)
		{
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final java.util.List<DropDownListItem> items;
			IList<DropDownListItem> items;

			IList<DropDownListItem> dropDownListItems = getDropDownItems(id);
			if (dropDownListItems != null)
			{
				items = dropDownListItems;
			}
			else
			{
				items = new List<DropDownListItem>();
			}

			// add default drop down items
			var ss = new Runnable(delegate {closeAll();});
			items.Add(new DropDownListItem(this, Android.Resource.Drawable.IcMenuCloseClearCancel, "Quit " + AppName, ss));

			// turn item list into views in PopupWindow
			LinearLayout list = new LinearLayout(this);
			list.Orientation = Android.Widget.Orientation.Vertical;

			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final Android.widget.PopupWindow dropDown = new Android.widget.PopupWindow(list, StandOutLayoutParams.WRAP_CONTENT, StandOutLayoutParams.WRAP_CONTENT, true);
			PopupWindow dropDown = new PopupWindow(list, WindowManagerLayoutParams.WrapContent, WindowManagerLayoutParams.WrapContent, true);

			foreach (DropDownListItem item in items)
			{
				ViewGroup listItem = (ViewGroup)mLayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
				list.AddView(listItem);

				ImageView icon = (ImageView)listItem.FindViewById(Resource.Id.Icon);
				Bitmap bm = BitmapFactory.DecodeResource(Resources, Resource.Id.Icon);
				icon.SetImageBitmap(bm);
				TextView description = (TextView)listItem.FindViewById(library.Resource.Id.description);
				description.Text = item.description;
				listItem.SetOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this, dropDown,item));
			}

			Drawable background = Resources.GetDrawable(Resource.Drawable.EditBoxDropDownDarkFrame);
			dropDown.SetBackgroundDrawable(background);
			return dropDown;
		}
		void pp() { }
		class OnClickListenerAnonymousInnerClassHelper :Java.Lang.Object, View.IOnClickListener
		{
			private readonly StandOutWindow outerInstance;

			private PopupWindow dropDown;
			DropDownListItem item;

			public OnClickListenerAnonymousInnerClassHelper(StandOutWindow outerInstance, PopupWindow dropDown,DropDownListItem item)
			{
				this.outerInstance = outerInstance;
				this.dropDown = dropDown;
				this.item = item;
			}


			public void OnClick(View v)
			{
				item.action.Run();
				dropDown.Dismiss();
			}
		}
		class RunnableAnonymousInnerClassHelper :Thread
		{
			private readonly StandOutWindow outerInstance;

			public RunnableAnonymousInnerClassHelper(StandOutWindow outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public void run()
			{
				outerInstance.closeAll();
			}
		}


		/// <summary>
		/// Implement this method to populate the drop down menu when the user clicks
		/// on the icon of the window corresponding to the id. The icon is only shown
		/// when <seealso cref="StandOutFlags#FLAG_DECORATION_SYSTEM"/> is set.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The list of items to show in the drop down menu, or null or empty
		///         to have no dropdown menu. </returns>
		public virtual IList<DropDownListItem> getDropDownItems(int id)
		{
			return null;
		}

		/// <summary>
		/// Implement this method to be alerted to touch events in the body of the
		/// window corresponding to the id.
		/// 
		/// <para>
		/// Note that even if you set <seealso cref="#FLAG_DECORATION_SYSTEM"/>, you will not
		/// receive touch events from the system window decorations.
		/// 
		/// </para>
		/// </summary>
		/// <seealso cref= <seealso cref="View.OnTouchListener#onTouch(View, MotionEvent)"/> </seealso>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="window">
		///            The window corresponding to the id, provided as a courtesy. </param>
		/// <param name="view">
		///            The view where the event originated from. </param>
		/// <param name="event">
		///            See linked method. </param>
		public virtual bool onTouchBody(int id, Window window, View view, MotionEvent @event)
		{
			return false;
		}

		/// <summary>
		/// Implement this method to be alerted to when the window corresponding to
		/// the id is moved.
		/// </summary>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="window">
		///            The window corresponding to the id, provided as a courtesy. </param>
		/// <param name="view">
		///            The view where the event originated from. </param>
		/// <param name="event">
		///            See linked method. </param>
		/// <seealso cref= <seealso cref="#onTouchHandleMove(int, Window, View, MotionEvent)"/> </seealso>
		public virtual void onMove(int id, Window window, View view, MotionEvent @event)
		{
		}

		/// <summary>
		/// Implement this method to be alerted to when the window corresponding to
		/// the id is resized.
		/// </summary>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="window">
		///            The window corresponding to the id, provided as a courtesy. </param>
		/// <param name="view">
		///            The view where the event originated from. </param>
		/// <param name="event">
		///            See linked method. </param>
		/// <seealso cref= <seealso cref="#onTouchHandleResize(int, Window, View, MotionEvent)"/> </seealso>
		public virtual void onResize(int id, Window window, View view, MotionEvent @event)
		{
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to be shown. This callback will occur before the view is
		/// added to the window manager.
		/// </summary>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="view">
		///            The view about to be shown. </param>
		/// <returns> Return true to cancel the view from being shown, or false to
		///         continue. </returns>
		/// <seealso cref= #show(int) </seealso>
		public virtual bool onShow(int id, Window window)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to be hidden. This callback will occur before the view is
		/// removed from the window manager and <seealso cref="#getHiddenNotification(int)"/>
		/// is called.
		/// </summary>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="view">
		///            The view about to be hidden. </param>
		/// <returns> Return true to cancel the view from being hidden, or false to
		///         continue. </returns>
		/// <seealso cref= #hide(int) </seealso>
		public virtual bool onHide(int id, Window window)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to be closed. This callback will occur before the view is
		/// removed from the window manager.
		/// </summary>
		/// <param name="id">
		///            The id of the view, provided as a courtesy. </param>
		/// <param name="view">
		///            The view about to be closed. </param>
		/// <returns> Return true to cancel the view from being closed, or false to
		///         continue. </returns>
		/// <seealso cref= #close(int) </seealso>
		public virtual bool onClose(int id, Window window)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when all windows are about to be
		/// closed. This callback will occur before any views are removed from the
		/// window manager.
		/// </summary>
		/// <returns> Return true to cancel the views from being closed, or false to
		///         continue. </returns>
		/// <seealso cref= #closeAll() </seealso>
		public virtual bool onCloseAll()
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id has received some data. The sender is described by fromCls and fromId
		/// if the sender wants a result. To send a result, use
		/// <seealso cref="#sendData(int, Class, int, int, Bundle)"/>.
		/// </summary>
		/// <param name="id">
		///            The id of your receiving window. </param>
		/// <param name="requestCode">
		///            The sending window provided this request code to declare what
		///            kind of data is being sent. </param>
		/// <param name="data">
		///            A bundle of parceleable data that was sent to your receiving
		///            window. </param>
		/// <param name="fromCls">
		///            The sending window's class. Provided if the sender wants a
		///            result. </param>
		/// <param name="fromId">
		///            The sending window's id. Provided if the sender wants a
		///            result. </param>
		public virtual void onReceiveData(int id, int requestCode, Bundle data, Type fromCls, int fromId)
		{
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to be updated in the layout. This callback will occur before
		/// the view is updated by the window manager.
		/// </summary>
		/// <param name="id">
		///            The id of the window, provided as a courtesy. </param>
		/// <param name="view">
		///            The window about to be updated. </param>
		/// <param name="params">
		///            The updated layout params. </param>
		/// <returns> Return true to cancel the window from being updated, or false to
		///         continue. </returns>
		/// <seealso cref= #updateViewLayout(int, Window, StandOutLayoutParams) </seealso>
		public virtual bool onUpdate(int id, Window window, StandOutLayoutParams @params)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to be bought to the front. This callback will occur before
		/// the window is brought to the front by the window manager.
		/// </summary>
		/// <param name="id">
		///            The id of the window, provided as a courtesy. </param>
		/// <param name="view">
		///            The window about to be brought to the front. </param>
		/// <returns> Return true to cancel the window from being brought to the front,
		///         or false to continue. </returns>
		/// <seealso cref= #bringToFront(int) </seealso>
		public virtual bool onBringToFront(int id, Window window)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id is about to have its focus changed. This callback will occur before
		/// the window's focus is changed.
		/// </summary>
		/// <param name="id">
		///            The id of the window, provided as a courtesy. </param>
		/// <param name="view">
		///            The window about to be brought to the front. </param>
		/// <param name="focus">
		///            Whether the window is gaining or losing focus. </param>
		/// <returns> Return true to cancel the window's focus from being changed, or
		///         false to continue. </returns>
		/// <seealso cref= #focus(int) </seealso>
		public virtual bool onFocusChange(int id, Window window, bool focus)
		{
			return false;
		}

		/// <summary>
		/// Implement this callback to be alerted when a window corresponding to the
		/// id receives a key event. This callback will occur before the window
		/// handles the event with <seealso cref="Window#dispatchKeyEvent(KeyEvent)"/>.
		/// </summary>
		/// <param name="id">
		///            The id of the window, provided as a courtesy. </param>
		/// <param name="view">
		///            The window about to receive the key event. </param>
		/// <param name="event">
		///            The key event. </param>
		/// <returns> Return true to cancel the window from handling the key event, or
		///         false to let the window handle the key event. </returns>
		/// <seealso cref= <seealso cref="Window#dispatchKeyEvent(KeyEvent)"/> </seealso>
		public virtual bool onKeyEvent(int id, Window window, KeyEvent @event)
		{
			return false;
		}

		/// <summary>
		/// Show or restore a window corresponding to the id. Return the window that
		/// was shown/restored.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The window shown. </returns>
		public Window show(int id)
		{
			lock (this)
			{
				// get the window corresponding to the id
				Window cachedWindow = getWindow(id);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final wei.mark.standout.ui.Window window;
				Window window;
        
				// check cache first
				if (cachedWindow != null)
				{
					window = cachedWindow;
				}
				else
				{
					window = new Window(this, id);
				}
        
				// alert callbacks and cancel if instructed
				if (onShow(id, window))
				{
					Log.Debug(TAG, "Window " + id + " show cancelled by implementation.");
					return null;
				}
        
				// focus an already shown window
				if (window.visibility == Window.VISIBILITY_VISIBLE)
				{
					Log.Debug(TAG, "Window " + id + " is already shown.");
					focus(id);
					return window;
				}
        
				window.visibility = Window.VISIBILITY_VISIBLE;
        
				// get animation
				Animation animation = getShowAnimation(id);
        
				// get the params corresponding to the id
				StandOutLayoutParams @params = window.GetLayoutParams();
        
				try
				{
					// add the view to the window manager
					mWindowManager.AddView(window, @params);
        
					// animate
					if (animation != null)
					{
						window.GetChildAt(0).StartAnimation(animation);
					}
				}
				catch (Java.Lang.Exception ex)
				{
					Console.WriteLine(ex.ToString());
					Console.Write(ex.StackTrace);
				}
        
				// add view to internal map
				sWindowCache.PutCache(id, this.GetType(), window);
        
				// get the persistent notification
				Notification notification = getPersistentNotification(id);
        
				// show the notification
				if (notification != null)
				{
					notification.Flags = notification.Flags | Android.App.NotificationFlags.NoClear;
        
					// only show notification if not shown before
					if (!startedForeground)
					{
						// tell Android system to show notification
						StartForeground(this.GetType().GetHashCode() + ONGOING_NOTIFICATION_ID, notification);
						startedForeground = true;
					}
					else
					{
						// update notification if shown before
						mNotificationManager.Notify(this.GetType().GetHashCode() + ONGOING_NOTIFICATION_ID, notification);
					}
				}
				else
				{
					// notification can only be null if it was provided before
					if (!startedForeground)
					{
						throw new Java.Lang.Exception("Your StandOutWindow service must" + "provide a persistent notification." + "The notification prevents Android" + "from killing your service in low" + "memory situations.");
					}
				}
        
				focus(id);
        
				return window;
			}
		}

		/// <summary>
		/// Hide a window corresponding to the id. Show a notification for the hidden
		/// window.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		public void hide(int id)
		{
			lock (this)
			{
				// get the view corresponding to the id
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final wei.mark.standout.ui.Window window = getWindow(id);
				Window window = getWindow(id);
        
				if (window == null)
				{
					throw new System.ArgumentException("Tried to hide(" + id + ") a null window.");
				}
        
				// alert callbacks and cancel if instructed
				if (onHide(id, window))
				{
					Log.Debug(TAG, "Window " + id + " hide cancelled by implementation.");
					return;
				}
        
				// ignore if window is already hidden
				if (window.visibility == Window.VISIBILITY_GONE)
				{
					Log.Debug(TAG, "Window " + id + " is already hidden.");
				}
        
				// check if hide enabled
				if (Utils.isSet(window.flags, StandOutFlags.FLAG_WINDOW_HIDE_ENABLE))
				{
					window.visibility = Window.VISIBILITY_TRANSITION;
        
					// get the hidden notification for this view
					Notification notification = getHiddenNotification(id);
        
					// get animation
					Animation animation = getHideAnimation(id);
        
					try
					{
						// animate
						if (animation != null)
						{
							animation.SetAnimationListener(new AnimationListenerAnonymousInnerClassHelper(this, window, animation));
							window.GetChildAt(0).StartAnimation(animation);
						}
						else
						{
							// remove the window from the window manager
							mWindowManager.RemoveView(window);
						}
					}
					catch (Java.Lang.Exception ex)
					{
						Console.WriteLine(ex.ToString());
						Console.Write(ex.StackTrace);
					}
        
					// display the notification
					notification.Flags = notification.Flags | NotificationFlags.NoClear | NotificationFlags.AutoCancel;
        
					mNotificationManager.Notify(this.GetType().GetHashCode() + id, notification);
        
				}
				else
				{
					// if hide not enabled, close window
					close(id);
				}
			}
		}

		private class AnimationListenerAnonymousInnerClassHelper :Java.Lang.Object,AnimationListener
		{
			private readonly StandOutWindow outerInstance;

			private Window window;
			private Animation animation;

			public AnimationListenerAnonymousInnerClassHelper(StandOutWindow outerInstance, Window window, Animation animation)
			{
				this.outerInstance = outerInstance;
				this.window = window;
				this.animation = animation;
			}


			public void OnAnimationStart(Animation animation)
			{
			}

			public void OnAnimationRepeat(Animation animation)
			{
			}

			public void OnAnimationEnd(Animation animation)
			{
				// remove the window from the window manager
				outerInstance.mWindowManager.RemoveView(window);
				window.visibility = Window.VISIBILITY_GONE;
			}
		
		}

		/// <summary>
		/// Close a window corresponding to the id.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: public final synchronized void close(final int id)
		public void close(int id)
		{
			lock (this)
			{
				// get the view corresponding to the id
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final wei.mark.standout.ui.Window window = getWindow(id);
				Window window = getWindow(id);
        
				if (window == null)
				{
					throw new System.ArgumentException("Tried to close(" + id + ") a null window.");
				}
        
				if (window.visibility == Window.VISIBILITY_TRANSITION)
				{
					return;
				}
        
				// alert callbacks and cancel if instructed
				if (onClose(id, window))
				{
					Log.Warn(TAG, "Window " + id + " close cancelled by implementation.");
					return;
				}
        
				// remove hidden notification
				mNotificationManager.Cancel(this.GetType().GetHashCode() + id);
        
				unfocus(window);
        
				window.visibility = Window.VISIBILITY_TRANSITION;
        
				// get animation
				Animation animation = getCloseAnimation(id);
        
				// remove window
				try
				{
					// animate
					if (animation != null)
					{
						animation.SetAnimationListener(new AnimationListenerAnonymousInnerClassHelper2(this, id, window, animation));
						window.GetChildAt(0).StartAnimation(animation);
					}
					else
					{
						// remove the window from the window manager
						mWindowManager.RemoveView(window);
        
						// remove view from internal map
						sWindowCache.RemoveCache(id, this.GetType());
        
						// if we just released the last window, quit
						if (sWindowCache.GetCacheSize(this.GetType()) == 0)
						{
							// tell Android to remove the persistent notification
							// the Service will be shutdown by the system on low memory
							startedForeground = false;
							StopForeground(true);
						}
					}
				}
				catch (Java.Lang.Exception ex)
				{
					Console.WriteLine(ex.ToString());
					Console.Write(ex.StackTrace);
				}
			}
		}

		private class AnimationListenerAnonymousInnerClassHelper2 :Java.Lang.Object, Animation.IAnimationListener
		{
			private readonly StandOutWindow outerInstance;

			private int id;
			private Window window;
			private Animation animation;


			public AnimationListenerAnonymousInnerClassHelper2(StandOutWindow outerInstance, int id, Window window, Animation animation)
			{
				this.outerInstance = outerInstance;
				this.id = id;
				this.window = window;
				this.animation = animation;
			}
			public  void OnAnimationStart(Animation animation)
			{
			}

			public  void OnAnimationRepeat(Animation animation)
			{
			}

			public  void OnAnimationEnd(Animation animation)
			{
				// remove the window from the window manager
				outerInstance.mWindowManager.RemoveView(window);
				window.visibility = Window.VISIBILITY_GONE;

				// remove view from internal map
				sWindowCache.RemoveCache(id, outerInstance.GetType());

				// if we just released the last window, quit
				if (outerInstance.ExistingIds.Count == 0)
				{
					// tell Android to remove the persistent
					// notification
					// the Service will be shutdown by the system on low
					// memory
					outerInstance.startedForeground = false;
					outerInstance.StopForeground(true);
				}
			}

		}

		/// <summary>
		/// Close all existing windows.
		/// </summary>
		public void closeAll()
		{
			lock (this)
			{
				// alert callbacks and cancel if instructed
				if (onCloseAll())
				{
					Log.Warn(TAG, "Windows close all cancelled by implementation.");
					return;
				}
        
				// add ids to temporary set to avoid concurrent modification
				LinkedList<int?> ids = new LinkedList<int?>();
				foreach (int id in ExistingIds)
				{
					ids.AddLast(id);
				}
        
				// close each window
				foreach (int id in ids)
				{
					close(id);
				}
			}
		}

		/// <summary>
		/// Send <seealso cref="Parceleable"/> data in a <seealso cref="Bundle"/> to a new or existing
		/// windows. The implementation of the recipient window can handle what to do
		/// with the data. To receive a result, provide the id of the sender.
		/// </summary>
		/// <param name="fromId">
		///            Provide the id of the sending window if you want a result. </param>
		/// <param name="toCls">
		///            The Service's class extending <seealso cref="StandOutWindow"/> that is
		///            managing the receiving window. </param>
		/// <param name="toId">
		///            The id of the receiving window. </param>
		/// <param name="requestCode">
		///            Provide a request code to declare what kind of data is being
		///            sent. </param>
		/// <param name="data">
		///            A bundle of parceleable data to be sent to the receiving
		///            window. </param>
		public void sendData(int fromId, Type toCls, int toId, int requestCode, Bundle data)
		{
			StandOutWindow.sendData(this, toCls, toId, requestCode, data, this.GetType(), fromId);
		}

		/// <summary>
		/// Bring the window corresponding to this id in front of all other windows.
		/// The window may flicker as it is removed and restored by the system.
		/// </summary>
		/// <param name="id">
		///            The id of the window to bring to the front. </param>
		public void bringToFront(int id)
		{
			lock (this)
			{
				Window window = getWindow(id);
				if (window == null)
				{
					throw new System.ArgumentException("Tried to bringToFront(" + id + ") a null window.");
				}
        
				if (window.visibility == Window.VISIBILITY_GONE)
				{
					throw new IllegalStateException("Tried to bringToFront(" + id + ") a window that is not shown.");
				}
        
				if (window.visibility == Window.VISIBILITY_TRANSITION)
				{
					return;
				}
        
				// alert callbacks and cancel if instructed
				if (onBringToFront(id, window))
				{
					Log.Warn(TAG, "Window " + id + " bring to front cancelled by implementation.");
					return;
				}
        
				StandOutLayoutParams @params = window.GetLayoutParams();
        
				// remove from window manager then add back
				try
				{
					mWindowManager.RemoveView(window);
				}
				catch (Java.Lang.Exception ex)
				{
					Console.WriteLine(ex.ToString());
					Console.Write(ex.StackTrace);
				}
				try
				{
					mWindowManager.AddView(window, @params);
				}
				catch (Java.Lang.Exception ex)
				{
					Console.WriteLine(ex.ToString());
					Console.Write(ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// Request focus for the window corresponding to this id. A maximum of one
		/// window can have focus, and that window will receive all key events,
		/// including Back and Menu.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> True if focus changed successfully, false if it failed. </returns>
		public bool focus(int id)
		{
			lock (this)
			{
				// check if that window is focusable
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final wei.mark.standout.ui.Window window = getWindow(id);
				Window window = getWindow(id);
				if (window == null)
				{
					throw new System.ArgumentException("Tried to focus(" + id + ") a null window.");
				}
        
				if (!Utils.isSet(window.flags, StandOutFlags.FLAG_WINDOW_FOCUSABLE_DISABLE))
				{
					// remove focus from previously focused window
					if (sFocusedWindow != null)
					{
						unfocus(sFocusedWindow);
					}
        
					return window.onFocus(true);
				}
        
				return false;
			}
		}

		/// <summary>
		/// Remove focus for the window corresponding to this id. Once a window is
		/// unfocused, it will stop receiving key events.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> True if focus changed successfully, false if it failed. </returns>
		public bool unfocus(int id)
		{
			lock (this)
			{
				Window window = getWindow(id);
				return unfocus(window);
			}
		}

		/// <summary>
		/// Courtesy method for your implementation to use if you want to. Gets a
		/// unique id to assign to a new window.
		/// </summary>
		/// <returns> The unique id. </returns>
		public int UniqueId
		{
			get
			{
				int unique = DEFAULT_ID;
				foreach (var id in ExistingIds)
				{
					unique = Java.Lang.Math.Max(unique, id.Value + 1);
				}
				return unique;
			}
		}

		/// <summary>
		/// Return whether the window corresponding to the id exists. This is useful
		/// for testing if the id is being restored (return true) or shown for the
		/// first time (return false).
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> True if the window corresponding to the id is either shown or
		///         hidden, or false if it has never been shown or was previously
		///         closed. </returns>
		public bool isExistingId(int id)
		{
			return sWindowCache.IsCached(id, this.GetType());
		}

		/// <summary>
		/// Return the ids of all shown or hidden windows.
		/// </summary>
		/// <returns> A set of ids, or an empty set. </returns>
		public ISet<int?> ExistingIds
		{
			get
			{
				return sWindowCache.GetCacheIds(this.GetType());
			}
		}

		/// <summary>
		/// Return the window corresponding to the id, if it exists in cache. The
		/// window will not be created with
		/// <seealso cref="#createAndAttachView(int, ViewGroup)"/>. This means the returned
		/// value will be null if the window is not shown or hidden.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <returns> The window if it is shown/hidden, or null if it is closed. </returns>
		public Window getWindow(int id)
		{
			return sWindowCache.GetCache(id, this.GetType());
		}

		/// <summary>
		/// Return the window that currently has focus.
		/// </summary>
		/// <returns> The window that has focus. </returns>
		public Window FocusedWindow
		{
			get
			{
				return sFocusedWindow;
			}
			set
			{
				sFocusedWindow = value;
			}
		}


		/// <summary>
		/// Change the title of the window, if such a title exists. A title exists if
		/// <seealso cref="StandOutFlags#FLAG_DECORATION_SYSTEM"/> is set, or if your own view
		/// contains a TextView with id R.id.title.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <param name="text">
		///            The new title. </param>
		public void setTitle(int id, string text)
		{
			Window window = getWindow(id);
			if (window != null)
			{
				View title = window.FindViewById(library.Resource.Id.title);
				if (title is TextView)
				{
					((TextView) title).Text = text;
				}
			}
		}

		/// <summary>
		/// Change the icon of the window, if such a icon exists. A icon exists if
		/// <seealso cref="StandOutFlags#FLAG_DECORATION_SYSTEM"/> is set, or if your own view
		/// contains a TextView with id R.id.window_icon.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <param name="drawableRes">
		///            The new icon. </param>
		public void setIcon(int id, int drawableRes)
		{
			Window window = getWindow(id);
			if (window != null)
			{
				View icon = new View(this);
				icon = icon.FindViewById(16908294);
				if (icon is ImageView)
				{
					((ImageView) icon).SetImageResource(drawableRes);
				}
			}
		}

		/// <summary>
		/// Internal touch handler for handling moving the window.
		/// </summary>
		/// <seealso cref= <seealso cref="View#onTouchEvent(MotionEvent)"/>
		/// </seealso>
		/// <param name="id"> </param>
		/// <param name="window"> </param>
		/// <param name="view"> </param>
		/// <param name="event">
		/// @return </param>
		public virtual bool onTouchHandleMove(int id, Window window, View view, MotionEvent @event)
		{
			StandOutLayoutParams @params = window.GetLayoutParams();
			// how much you have to move in either direction in order for the
			// gesture to be a move and not tap

			int totalDeltaX = window.touchInfo.lastX - window.touchInfo.firstX;
			int totalDeltaY = window.touchInfo.lastY - window.touchInfo.firstY;

			switch (@event.Action)
			{
				case MotionEventActions.Down:
					window.touchInfo.lastX = (int) @event.RawX;
					window.touchInfo.lastY = (int) @event.RawY;

					window.touchInfo.firstX = window.touchInfo.lastX;
					window.touchInfo.firstY = window.touchInfo.lastY;
					break;
				case MotionEventActions.Move:
					int deltaX = (int) @event.RawX - window.touchInfo.lastX;
					int deltaY = (int) @event.RawY - window.touchInfo.lastY;

					window.touchInfo.lastX = (int) @event.RawX;
					window.touchInfo.lastY = (int) @event.RawY;

					if (window.touchInfo.moving || Java.Lang.Math.Abs(totalDeltaX) >= @params.threshold || Java.Lang.Math.Abs(totalDeltaY) >= @params.threshold)
					{
						window.touchInfo.moving = true;

						// if window is moveable
						if (Utils.isSet(window.flags, StandOutFlags.FLAG_BODY_MOVE_ENABLE))
						{

							// update the position of the window
							if (@event.PointerCount == 1)
							{
								@params.X += deltaX;
								@params.Y += deltaY;
							}

							window.edit().setPosition(@params.X, @params.Y).commit();
						}
					}
					break;
				case MotionEventActions.Up:
					window.touchInfo.moving = false;

					if (@event.PointerCount == 1)
					{

						// bring to front on tap
						bool tap = System.Math.Abs(totalDeltaX) < @params.threshold && System.Math.Abs(totalDeltaY) < @params.threshold;
						if (tap && Utils.isSet(window.flags, StandOutFlags.FLAG_WINDOW_BRING_TO_FRONT_ON_TAP))
						{
							bringToFront(id);
						}
					}

					// bring to front on touch
					else if (Utils.isSet(window.flags, StandOutFlags.FLAG_WINDOW_BRING_TO_FRONT_ON_TOUCH))
					{
					 bringToFront(id);
					}

					break;
			}

			onMove(id, window, view, @event);

			return true;
		}

		/// <summary>
		/// Internal touch handler for handling resizing the window.
		/// </summary>
		/// <seealso cref= <seealso cref="View#onTouchEvent(MotionEvent)"/>
		/// </seealso>
		/// <param name="id"> </param>
		/// <param name="window"> </param>
		/// <param name="view"> </param>
		/// <param name="event">
		/// @return </param>
		public virtual bool onTouchHandleResize(int id, Window window, View view, MotionEvent @event)
		{
			StandOutLayoutParams @params = (StandOutLayoutParams)window.GetLayoutParams();

			switch (@event.Action)
			{
				case MotionEventActions.Down:
					window.touchInfo.lastX = (int) @event.RawX;
					window.touchInfo.lastY = (int) @event.RawY;

					window.touchInfo.firstX = window.touchInfo.lastX;
					window.touchInfo.firstY = window.touchInfo.lastY;
					break;
				case MotionEventActions.Move:
					int deltaX = (int) @event.RawX - window.touchInfo.lastX;
					int deltaY = (int) @event.RawY - window.touchInfo.lastY;

					// update the size of the window
					@params.Width += deltaX;
					@params.Height += deltaY;

					// keep window between min/max width/height
					if (@params.Width >= @params.minWidth && @params.Width <= @params.maxWidth)
					{
						window.touchInfo.lastX = (int) @event.RawX;
					}

					if (@params.Height >= @params.minHeight && @params.Height <= @params.maxHeight)
					{
						window.touchInfo.lastY = (int) @event.RawY;
					}

					window.edit().setSize(@params.Width, @params.Height).commit();
					break;
				case MotionEventActions.Up:
					break;
			}

			onResize(id, window, view, @event);

			return true;
		}

		/// <summary>
		/// Remove focus for the window, which could belong to another application.
		/// Since we don't allow windows from different applications to directly
		/// interact with each other, except for
		/// <seealso cref="#sendData(Context, Class, int, int, Bundle, Class, int)"/>, this
		/// method is private.
		/// </summary>
		/// <param name="window">
		///            The window to unfocus. </param>
		/// <returns> True if focus changed successfully, false if it failed. </returns>
		public virtual bool unfocus(Window window)
		{
			lock (this)
			{
				if (window == null)
				{
					throw new System.ArgumentException("Tried to unfocus a null window.");
				}
				return window.onFocus(false);
			}
		}

		/// <summary>
		/// Update the window corresponding to this id with the given params.
		/// </summary>
		/// <param name="id">
		///            The id of the window. </param>
		/// <param name="params">
		///            The updated layout params to apply. </param>
		public virtual void updateViewLayout(int id, StandOutLayoutParams @params)
		{
			Window window = getWindow(id);

			if (window == null)
			{
				throw new System.ArgumentException("Tried to updateViewLayout(" + id + ") a null window.");
			}

			if (window.visibility == Window.VISIBILITY_GONE)
			{
				return;
			}

			if (window.visibility == Window.VISIBILITY_TRANSITION)
			{
				return;
			}

			// alert callbacks and cancel if instructed
			if (onUpdate(id, window, @params))
			{
				Log.Warn(TAG, "Window " + id + " update cancelled by implementation.");
				return;
			}

			try
			{
				window.SetLayoutParams(@params);
				mWindowManager.UpdateViewLayout(window, @params);
			}
			catch (Java.Lang.Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
		}

		/// <summary>
		/// LayoutParams specific to floating StandOut windows.
		/// 
		/// @author Mark Wei <markwei@gmail.com>
		/// 
		/// </summary>
		public class StandOutLayoutParams : WindowManagerLayoutParams
		{
			private readonly StandOutWindow outerInstance;

			/// <summary>
			/// Special value for x position that represents the left of the screen.
			/// </summary>
			public const int LEFT = 0;
			/// <summary>
			/// Special value for y position that represents the top of the screen.
			/// </summary>
			public const int TOP = 0;
			/// <summary>
			/// Special value for x position that represents the right of the screen.
			/// </summary>
			public static readonly int RIGHT = int.MaxValue;
			/// <summary>
			/// Special value for y position that represents the bottom of the
			/// screen.
			/// </summary>
			public static readonly int BOTTOM = int.MaxValue;
			/// <summary>
			/// Special value for x or y position that represents the center of the
			/// screen.
			/// </summary>
			public static readonly int CENTER = int.MinValue;
			/// <summary>
			/// Special value for x or y position which requests that the system
			/// determine the position.
			/// </summary>
			public static readonly int AUTO_POSITION = int.MinValue + 1;

			/// <summary>
			/// The distance that distinguishes a tap from a drag.
			/// </summary>
			public int threshold;

			/// <summary>
			/// Optional constraints of the window.
			/// </summary>
			public int minWidth, minHeight, maxWidth, maxHeight;

			/// <param name="id">
			///            The id of the window. </param>
			public StandOutLayoutParams(StandOutWindow outerInstance, int id) : base(200, 200, WindowManagerTypes.Phone,WindowManagerFlags.NotTouchModal | WindowManagerFlags.WatchOutsideTouch, Format.Translucent)
			{
				this.outerInstance = outerInstance;

				int windowFlags = outerInstance.getFlags(id);

				FocusFlag = false;

				if (!Utils.isSet(windowFlags, StandOutFlags.FLAG_WINDOW_EDGE_LIMITS_ENABLE))
				{
					// windows may be moved beyond edges
					Flags |= WindowManagerFlags.LayoutNoLimits;
				}

				X = getX(id, Width);
				Y = getY(id, Height);

				Gravity = GravityFlags.Top | GravityFlags.Left;

				threshold = 10;
				minWidth = minHeight = 0;
				maxWidth = maxHeight = int.MaxValue;
			}

			/// <param name="id">
			///            The id of the window. </param>
			/// <param name="w">
			///            The width of the window. </param>
			/// <param name="h">
			///            The height of the window. </param>
			public StandOutLayoutParams(StandOutWindow outerInstance, int id, int w, int h) : this(outerInstance, id)
			{
				this.outerInstance = outerInstance;
				Width = w;
				Height = h;
			}

			/// <param name="id">
			///            The id of the window. </param>
			/// <param name="w">
			///            The width of the window. </param>
			/// <param name="h">
			///            The height of the window. </param>
			/// <param name="xpos">
			///            The x position of the window. </param>
			/// <param name="ypos">
			///            The y position of the window. </param>
			public StandOutLayoutParams(StandOutWindow outerInstance, int id, int w, int h, int xpos, int ypos) : this(outerInstance, id, w, h)
			{
				this.outerInstance = outerInstance;

				if (xpos != AUTO_POSITION)
				{
					X = xpos;
				}
				if (ypos != AUTO_POSITION)
				{
					Y = ypos;
				}

				Display display = outerInstance.mWindowManager.DefaultDisplay;
				int width = display.Width;
				int height = display.Height;

				if (X == RIGHT)
				{
					X = width - w;
				}
				else if (X == CENTER)
				{
					X = (width - w) / 2;
				}

				if (Y == BOTTOM)
				{
					Y = height - h;
				}
				else if (Y == CENTER)
				{
					Y = (height - h) / 2;
				}
			}

			/// <param name="id">
			///            The id of the window. </param>
			/// <param name="w">
			///            The width of the window. </param>
			/// <param name="h">
			///            The height of the window. </param>
			/// <param name="xpos">
			///            The x position of the window. </param>
			/// <param name="ypos">
			///            The y position of the window. </param>
			/// <param name="minWidth">
			///            The minimum width of the window. </param>
			/// <param name="minHeight">
			///            The mininum height of the window. </param>
			public StandOutLayoutParams(StandOutWindow outerInstance, int id, int w, int h, int xpos, int ypos, int minWidth, int minHeight) : this(outerInstance, id, w, h, xpos, ypos)
			{
				this.outerInstance = outerInstance;

				this.minWidth = minWidth;
				this.minHeight = minHeight;
			}

			/// <param name="id">
			///            The id of the window. </param>
			/// <param name="w">
			///            The width of the window. </param>
			/// <param name="h">
			///            The height of the window. </param>
			/// <param name="xpos">
			///            The x position of the window. </param>
			/// <param name="ypos">
			///            The y position of the window. </param>
			/// <param name="minWidth">
			///            The minimum width of the window. </param>
			/// <param name="minHeight">
			///            The mininum height of the window. </param>
			/// <param name="threshold">
			///            The touch distance threshold that distinguishes a tap from
			///            a drag. </param>
			public StandOutLayoutParams(StandOutWindow outerInstance, int id, int w, int h, int xpos, int ypos, int minWidth, int minHeight, int threshold) : this(outerInstance, id, w, h, xpos, ypos, minWidth, minHeight)
			{
				this.outerInstance = outerInstance;

				this.threshold = threshold;
			}

			// helper to create cascading windows
			internal virtual int getX(int id, int width)
			{
				Display display = outerInstance.mWindowManager.DefaultDisplay;
				int displayWidth = display.Width;

				int types = sWindowCache.Size();

				int initialX = 100 * types;
				int variableX = 100 * id;
				int rawX = initialX + variableX;

				return rawX % (displayWidth - width);
			}

			// helper to create cascading windows
			internal virtual int getY(int id, int height)
			{
				Display display = outerInstance.mWindowManager.DefaultDisplay;
				int displayWidth = display.Width;
				int displayHeight = display.Height;

				int types = sWindowCache.Size();

				int initialY = 100 * types;
				int variableY = X + 200 * (100 * id) / (displayWidth - Width);

				int rawY = initialY + variableY;

				return rawY % (displayHeight - height);
			}

			public virtual bool FocusFlag
			{
				set
				{
					if (value)
					{
						Flags = Flags ^ WindowManagerFlags.NotFocusable;
					}
					else
					{
						Flags = Flags | WindowManagerFlags.NotFocusable;
					}
				}
			}
		}

		public class DropDownListItem
		{
			private readonly StandOutWindow outerInstance;

			public int icon;
			public string description;
			public Runnable action;

			public DropDownListItem(StandOutWindow outerInstance, int icon, string description, Runnable action) : base()
			{
				this.outerInstance = outerInstance;
				this.icon = icon;
				this.description = description;
				this.action = action;
			}

			public override string ToString()
			{
				return description;
			}
		}
	}

}