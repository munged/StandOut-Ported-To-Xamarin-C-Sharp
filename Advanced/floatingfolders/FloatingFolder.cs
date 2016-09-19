using System;
using System.Collections.Generic;

namespace wei.mark.floatingfolders
{
	using Android.App;
	using Android.Util;
	using Android.Views;
	using Android.Widget;
	using Java.IO;
	using Java.Lang;
	using Advanced;
	using ActivityInfo = Android.Content.PM.ActivityInfo;
	using AdapterView = Android.Widget.AdapterView;
	using Animation = Android.Views.Animations.Animation;
	using AnimationUtils = Android.Views.Animations.AnimationUtils;
	using Bundle = Android.OS.Bundle;
	using ComponentName = Android.Content.ComponentName;
	using Context = Android.Content.Context;
	using Drawable = Android.Graphics.Drawables.Drawable;
	using FrameLayout = Android.Widget.FrameLayout;
	using ImageView = Android.Widget.ImageView;
	using Intent = Android.Content.Intent;
	using LayoutInflater = Android.Views.LayoutInflater;
	using LinearLayout = Android.Widget.LinearLayout;
	using ListView = Android.Widget.ListView;
	using Log = Android.Util.Log;
	using MotionEvent = Android.Views.MotionEvent;
	using PackageManager = Android.Content.PM.PackageManager;
	using ResolveInfo = Android.Content.PM.ResolveInfo;
	using StandOutFlags = standout.constants.StandOutFlags;
	using StandOutWindow = standout.StandOutWindow;
	using TextView = Android.Widget.TextView;
	using Toast = Android.Widget.Toast;
	using View = Android.Views.View;
	using ViewGroup = Android.Views.ViewGroup;
	using Window = standout.ui.Window;
	using WindowManager = Android.Views.IWindowManager;
	[Service]
	public sealed class FloatingFolder : StandOutWindow
	{
		 const int APP_SELECTOR_ID = -2;

		 const int APP_SELECTOR_CODE = 2;
		 const int APP_SELECTOR_FINISHED_CODE = 3;
		public const int STARTUP_CODE = 4;

		internal PackageManager MPackageManager;
		internal WindowManager MWindowManager;

		internal int IconSize;
		internal int SquareWidth;

		internal SparseArray<FolderModel> MFolders;

		internal Animation MFadeOut, MFadeIn;

		public static void ShowFolders(Context context)
		{
			sendData(context, typeof(FloatingFolder), DISREGARD_ID, STARTUP_CODE, null, null, DISREGARD_ID);
		}

		public override void OnCreate()
		{
			base.OnCreate();

			MPackageManager = PackageManager;
			MWindowManager = (WindowManager)GetSystemService(WindowService);

			IconSize = (int)Resources.GetDimension(Android.Resource.Dimension.AppIconSize);
			SquareWidth = IconSize + 8 * 8;

			MFadeOut = AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeOut);
			MFadeIn = AnimationUtils.LoadAnimation(this, Android.Resource.Animation.FadeIn);

			int duration = 100;
			MFadeOut.Duration = (duration);
			MFadeIn.Duration = (duration);
		}

		public override string AppName
		{
			get
			{
				return "Floating Folders";
			}
		}
		public override int AppIcon
		{
			get
			{
				return Resource.Drawable.ic_launcher;
			}
		}

		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: @Override public void createAndAttachView(final int id, Android.widget.FrameLayout frame)
		public override void CreateAndAttachView(int id, FrameLayout frame)
		{
			LayoutInflater inflater = LayoutInflater.From(this);

			// choose which type of window to show
			if (APP_SELECTOR_ID == id)
			{
				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final Android.Views.View view = inflater.inflate(Resource.layout.app_selector, frame, true);
				View view = inflater.Inflate(Resource.Layout.app_selector, frame, true);
				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final Android.widget.ListView listView = (Android.widget.ListView) view.findViewById(Resource.id.list);
				ListView listView = (ListView)view.FindViewById(Resource.Id.list);
				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final java.util.List<Android.Content.pm.ActivityInfo> apps = new java.util.ArrayList<Android.Content.pm.ActivityInfo>();
				IList<ActivityInfo> apps = new List<ActivityInfo>();

				listView.Clickable = (true);

				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final AppAdapter adapter = new AppAdapter(this, Resource.layout.app_row, apps);
				AppAdapter adapter = new AppAdapter(this, Resource.Layout.app_row, apps);
				listView.Adapter = (adapter);

				listView.OnItemClickListener = (new OnItemClickListenerAnonymousInnerClass(this, id, view));

				(new Thread(() =>
				{
					Intent mainIntent = new Intent(Intent.ActionMain, null);
					mainIntent.AddCategory(Intent.CategoryLauncher);
					List<ResolveInfo> resolveApps = (List<ResolveInfo>)PackageManager.QueryIntentActivities(mainIntent, 0);
					resolveApps.Sort(new ComparatorAnonymousInnerClass(this));
					foreach (ResolveInfo resolveApp in resolveApps)
					{
						apps.Add(resolveApp.ActivityInfo);
					}

					Log.Debug("FloatingFolder", "before");
					view.Post(() =>
					{

						Log.Debug("FloatingFolder", "after");
						adapter.NotifyDataSetChanged();
					});
				})).Start();

				View cancel = view.FindViewById(Resource.Id.cancel);
				cancel.SetOnClickListener(new OnClickListenerAnonymousInnerClass(this, id));
			}
			else
			{
				// id is not app selector
				View view = inflater.Inflate(Resource.Layout.folder, frame, true);

				FlowLayout flow = (FlowLayout)view.FindViewById(Resource.Id.flow);

				if (MFolders == null)
				{
					LoadAllFolders();
				}

				FolderModel folder = MFolders.Get(id);
				if (folder != null)
				{
					foreach (ActivityInfo app in folder.Apps)
					{
						AddAppToFolder(id, app, flow);
					}
				}
			}
		}

		private class OnItemClickListenerAnonymousInnerClass : Java.Lang.Object, AdapterView.IOnItemClickListener
		{
			private readonly FloatingFolder OuterInstance;

			private int Id;
			private View View;

			public OnItemClickListenerAnonymousInnerClass(FloatingFolder outerInstance, int id, View view)
			{
				this.OuterInstance = outerInstance;
				this.Id = id;
				this.View = view;
			}


			public void OnItemClick(AdapterView parent, View view, int position, long rowId)
			{
				Window window = (Window)Id;

				// close self
				OuterInstance.close(Id);

				ActivityInfo app = (ActivityInfo)parent.GetItemAtPosition(0);

				// send data back
				if (window.data.ContainsKey("fromId"))
				{
					Bundle data = new Bundle();
					data.PutParcelable("app", app);
					OuterInstance.sendData(Id, typeof(FloatingFolder), window.data.GetInt("fromId"), APP_SELECTOR_FINISHED_CODE, data);
				}
			}
		}

		private class ComparatorAnonymousInnerClass : IComparer<ResolveInfo>
		{
			private readonly FloatingFolder OuterInstance;

			public ComparatorAnonymousInnerClass(FloatingFolder outerInstance)
			{
				this.OuterInstance = outerInstance;
			}


			public virtual int Compare(ResolveInfo app1, ResolveInfo app2)
			{
				string label1 = app1.LoadLabel(OuterInstance.MPackageManager).ToString();
				string label2 = app2.LoadLabel(OuterInstance.MPackageManager).ToString();
				return string.Compare(label1, label2, StringComparison.Ordinal);
			}
		}

		private class OnClickListenerAnonymousInnerClass : Object, View.IOnClickListener
		{
			private readonly FloatingFolder OuterInstance;

			private int Id;

			public OnClickListenerAnonymousInnerClass(FloatingFolder outerInstance, int id)
			{
				this.OuterInstance = outerInstance;
				this.Id = id;
			}


			public void OnClick(View v)
			{
				// close self
				OuterInstance.close(Id);
			}
		}

		public override StandOutLayoutParams getParams(int id, Window window)
		{
			if (APP_SELECTOR_ID == id)
			{
				return new StandOutLayoutParams(this, id, 400, ViewGroup.LayoutParams.MatchParent, StandOutLayoutParams.CENTER, StandOutLayoutParams.TOP);
			}
			else
			{
				FolderModel folder = MFolders.Get(id);
				int width = folder.Width;
				int height = folder.Height;

				if (width == 0)
				{
					width = 400;
				}
				if (height == 0)
				{
					height = 400;
				}
				return new StandOutLayoutParams(this, id, width, height, 50, 50);
			}
		}

		public override int getFlags(int id)
		{
			if (APP_SELECTOR_ID == id)
			{
				return base.getFlags(id);
			}
			else
			{
				return base.getFlags(id) | StandOutFlags.FLAG_BODY_MOVE_ENABLE | StandOutFlags.FLAG_WINDOW_EDGE_LIMITS_ENABLE | StandOutFlags.FLAG_WINDOW_FOCUSABLE_DISABLE;
			}
		}

		public override void onReceiveData(int id, int requestCode, Bundle data, Type fromCls, int fromId)
		{
			switch (requestCode)
			{
				case APP_SELECTOR_CODE:
					if (APP_SELECTOR_ID == id)
					{
						// app selector receives data
						Window window2 = show(APP_SELECTOR_ID);
						window2.data.PutInt("fromId", fromId);
					}
					break;
				case APP_SELECTOR_FINISHED_CODE:
					//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
					//ORIGINAL LINE: final Android.Content.pm.ActivityInfo app = data.getParcelable("app");
					ActivityInfo app = (ActivityInfo)data.GetParcelable("app");
					Log.Debug("FloatingFolder", "Received app: " + app);

					Window window = getWindow(id);
					if (window == null)
					{
						return;
					}

					ViewGroup flow = (ViewGroup)window.FindViewById(Resource.Id.flow);

					AddAppToFolder(id, app, flow);

					OnUserAddApp(id, app);
					break;
				case STARTUP_CODE:
					LoadAllFolders();
					if (MFolders.Size() == 0)
					{
						MFolders.Put(DEFAULT_ID, new FolderModel());
						show(DEFAULT_ID);
					}
					else
					{
						for (int i = 0; i < MFolders.Size(); i++)
						{
							FolderModel folder = MFolders.Get(MFolders.KeyAt(i));
							if (folder.Shown)
							{
								show(folder.Id);
							}
						}
					}
					break;
			}
		}

		private void AddAppToFolder(int id, ActivityInfo app, ViewGroup flow)
		{
			View frame = GetAppView(id, app);

			flow.AddView(frame);
		}

		private void RemoveAppFromView(int id, ActivityInfo app)
		{
			Window window = getWindow(id);
			View frame = window.FindViewWithTag(app);
			ViewGroup flow = (ViewGroup)window.FindViewById(Resource.Id.flow);

			flow.RemoveView(frame);
		}

		private void OnUserAddApp(int id, ActivityInfo app)
		{
			FolderModel folder = MFolders.Get(id);
			folder.Apps.Add(app);

			ResizeToGridAndSave(id, -1);
		}

		private void OnUserRemoveApp(int id, ActivityInfo app)
		{
			RemoveAppFromView(id, app);

			FolderModel folder = MFolders.Get(id);
			folder.Apps.Remove(app);

			ResizeToGridAndSave(id, -1);
		}

		private void SaveFolder(FolderModel folder)
		{
			System.IO.FileStream @out = null;
			try
			{
				@out = (System.IO.FileStream)OpenFileOutput(string.Format("folder{0:D}", folder.Id), Android.Content.FileCreationMode.Private);

				@out.WriteByte(byte.Parse(string.Format("{0:D}\n", folder.Width)));
				@out.WriteByte(byte.Parse(string.Format("{0:D}\n", folder.Height)));

				foreach (ActivityInfo appInFolder in folder.Apps)
				{
					ComponentName name = new ComponentName(appInFolder.PackageName, appInFolder.Name);

					@out.WriteByte(byte.Parse((name.FlattenToString() + "\n")));
				}
			}
			catch (FileNotFoundException e)
			{
				System.Console.WriteLine(e.Message);
				System.Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
				System.Console.WriteLine(e.ToString());
				System.Console.Write(e.StackTrace);
			}
			finally
			{
				if (@out != null)
				{
					try
					{
						@out.Close();
					}
					catch (IOException e)
					{
						System.Console.WriteLine(e.ToString());
						System.Console.Write(e.StackTrace);
					}
				}
			}
		}

		private void LoadAllFolders()
		{
			MFolders = new SparseArray<FolderModel>();
			string[] folders = FileList();
			foreach (string folderFileName in folders)
			{

				System.IO.FileStream @in = null;
				try
				{
					if (folderFileName.StartsWith("folder", StringComparison.Ordinal))
					{
						FolderModel folder = new FolderModel();
						folder.Id = int.Parse(folderFileName.Substring("folder".Length));

						@in = (System.IO.FileStream)OpenFileInput(folderFileName);
						System.IO.MemoryStream bos = new System.IO.MemoryStream();
						byte[] b = new byte[1024];
						int bytesRead;
						while ((bytesRead = @in.Read(b, 0, b.Length)) != -1)
						{
							bos.Write(b, 0, bytesRead);
						}
						byte[] bytes = bos.ToArray();
						string appNames = StringHelperClass.NewString((sbyte[])(Array)bytes);

						int i = 0;
						foreach (string appName in appNames.Split("\n", true))
						{
							if (i < 2)
							{
								// width and height
								try
								{
									if (i == 0)
									{
										folder.Width = int.Parse(appName);
									}
									else if (i == 1)
									{
										folder.Height = int.Parse(appName);
									}
								}
								catch (System.FormatException)
								{
									string msg = "Please uninstall Floating Folders and reinstall it. The folder format has changed.";
									Log.Debug("FloatingFolder", msg);
									Toast.MakeText(this, msg, ToastLength.Short).Show();
									break;
								}
								i++;
							}
							else
							{
								if (appName.Length > 0)
								{
									ComponentName name = ComponentName.UnflattenFromString(appName);
									try
									{
										ActivityInfo app = MPackageManager.GetActivityInfo(name, 0);
										folder.Apps.Add(app);
										MFolders.Put(folder.Id, folder);
									}
									catch (PackageManager.NameNotFoundException e)
									{
										System.Console.WriteLine(e.ToString());
										System.Console.Write(e.StackTrace);
									}
								}
							}
						}
					}
				}
				catch (FileNotFoundException e)
				{
					System.Console.WriteLine(e.ToString());
					System.Console.Write(e.StackTrace);
				}
				catch (IOException e)
				{
					System.Console.WriteLine(e.ToString());
					System.Console.Write(e.StackTrace);
				}
				finally
				{
					if (@in != null)
					{
						try
						{
							@in.Close();
						}
						catch (IOException e)
						{
							System.Console.WriteLine(e.ToString());
							System.Console.Write(e.StackTrace);
						}
					}
				}
			}
		}

		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: private Android.Views.View getAppView(final int id, final Android.Content.pm.ActivityInfo app)
		private View GetAppView(int id, ActivityInfo app)
		{
			LayoutInflater inflater = LayoutInflater.From(this);
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final Android.Views.View frame = inflater.inflate(Resource.layout.app_square, null);
			View frame = inflater.Inflate(Resource.Layout.app_square, null);

			frame.Tag = (app);

			frame.SetOnClickListener(new OnClickListenerAnonymousInnerClass2(this, app));

			frame.SetOnLongClickListener(new OnLongClickListenerAnonymousInnerClass(this, id, app));

			ImageView icon = (ImageView)frame.FindViewById(Resource.Id.icon);
			icon.SetImageDrawable(app.LoadIcon(MPackageManager));
			LinearLayout.LayoutParams @params = new LinearLayout.LayoutParams(IconSize, IconSize);
			@params.Gravity = Android.Views.GravityFlags.Center;
			icon.LayoutParameters = (@params);

			TextView name = (TextView)frame.FindViewById(Resource.Id.name);
			name.Text = (app.LoadLabel(MPackageManager));

			View square = frame.FindViewById(Resource.Id.square);
			square.LayoutParameters = (new FrameLayout.LayoutParams(SquareWidth, ViewGroup.LayoutParams.WrapContent));

			return frame;
		}

		private class OnClickListenerAnonymousInnerClass2 : Java.Lang.Object, View.IOnClickListener
		{
			private readonly FloatingFolder OuterInstance;

			private ActivityInfo App;

			public OnClickListenerAnonymousInnerClass2(FloatingFolder outerInstance, ActivityInfo app)
			{
				this.OuterInstance = outerInstance;
				this.App = app;
			}


			public void OnClick(View v)
			{
				Intent intent = OuterInstance.MPackageManager.GetLaunchIntentForPackage(App.PackageName);
				OuterInstance.StartActivity(intent);
			}
		}

		private class OnLongClickListenerAnonymousInnerClass : Java.Lang.Object, View.IOnLongClickListener
		{
			private readonly FloatingFolder OuterInstance;

			private int Id;
			private ActivityInfo App;

			public OnLongClickListenerAnonymousInnerClass(FloatingFolder outerInstance, int id, ActivityInfo app)
			{
				this.OuterInstance = outerInstance;
				this.Id = id;
				this.App = app;
			}


			public bool OnLongClick(View v)
			{
				ActivityInfo app = (ActivityInfo)v.Tag;
				Log.Debug("FloatingFolder", "Long clicked: " + app.LoadLabel(OuterInstance.MPackageManager));

				OuterInstance.OnUserRemoveApp(Id, app);
				return true;
			}
		}

		public void OnResize(int id, Window window, View view, MotionEvent @event)
		{
			if (@event.Action == Android.Views.MotionEventActions.Up)
			{
				ResizeToGridAndSave(id, -1);
			}
		}

		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: private void resizeToGridAndSave(final int id, final int cols)
		private void ResizeToGridAndSave(int id, int cols)
		{
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final wei.mark.standout.ui.Window window = getWindow(id);
			Window window = getWindow(id);

			window.Post(() =>
		{

			FlowLayout flow = (FlowLayout)window.FindViewById(Resource.Id.flow);

			FolderModel folder = MFolders.Get(id);

			int count = folder.Apps.Count;
			int columns = cols;

			if (cols == -1)
			{
				columns = flow.GetCols();
			}

			if (columns < 2)
			{
				columns = 2;
			}

			int rows = count / columns;
			if (count % columns > 0)
			{
				rows++;
			}

			if (rows < 1)
			{
				rows = 1;
			}

			int width = flow.Left + (((ViewGroup)flow.Parent).Width - flow.Right) + columns * SquareWidth;
			int height = width;

			if (count > 0)
			{
				height = flow.Top + (((ViewGroup)flow.Parent).Height - flow.Bottom) + rows * flow.GetChildHeight();
			}

			StandOutLayoutParams @params = window.GetLayoutParams();
			@params.Width = width;
			@params.Height = height;
			updateViewLayout(id, @params);

			folder.Width = width;
			folder.Height = height;

			SaveFolder(folder);
		});
		}

		public bool OnFocusChange(int id, Window window, bool focus)
		{
			if (id == APP_SELECTOR_ID && !focus)
			{
				close(APP_SELECTOR_ID);
				return false;
			}
			return OnFocusChange(id, window, focus);
		}

		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: @Override public boolean onTouchBody(final int id, final wei.mark.standout.ui.Window window, final Android.Views.View view, Android.Views.MotionEvent event)
		public bool OnTouchBody(int id, Window window, View view, MotionEvent @event)
		{
			if (id != APP_SELECTOR_ID && @event.Action == Android.Views.MotionEventActions.Move)
			{
				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final StandOutLayoutParams params = (StandOutLayoutParams) window.getLayoutParams();
				StandOutLayoutParams @params = window.GetLayoutParams();

				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final Android.Views.View folderView = window.findViewById(Resource.id.folder);
				View folderView = window.FindViewById(Resource.Id.folder);
				//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
				//ORIGINAL LINE: final Android.widget.ImageView screenshot = (Android.widget.ImageView) window.findViewById(Resource.id.preview);
				ImageView screenshot = (ImageView)window.FindViewById(Resource.Id.preview);

				FolderModel folder = MFolders.Get(id);

				// if touch edge
				if (@params.X <= 0)
				{
					// first time touch edge
					if (folder.FullSize)
					{
						folder.FullSize = false;

						//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
						//ORIGINAL LINE: final Android.graphics.drawable.Drawable drawable = getResources().getDrawable(Resource.drawable.ic_menu_archive);
						Drawable drawable = Resources.GetDrawable(Resource.Drawable.ic_menu_archive);

						screenshot.SetImageDrawable(drawable);

						MFadeOut.SetAnimationListener(new AnimationListenerAnonymousInnerClass(this, id, @params, folderView, screenshot, drawable));

						folderView.StartAnimation(MFadeOut);
					}
				}
				else
				{ // not touch edge

					// first time not touch edge
					if (!folder.FullSize)
					{
						folder.FullSize = true;

						MFadeOut.SetAnimationListener(new AnimationListenerAnonymousInnerClass2(this, id, window, @params, folderView, screenshot));

						screenshot.StartAnimation(MFadeOut);
					}
				}
			}

			return false;
		}

		private class AnimationListenerAnonymousInnerClass : Java.Lang.Object, Animation.IAnimationListener
		{
			private readonly FloatingFolder OuterInstance;

			private int Id;
			private StandOutLayoutParams @params;
			private View FolderView;
			private ImageView Screenshot;
			private Drawable Drawable;

			public AnimationListenerAnonymousInnerClass(FloatingFolder outerInstance, int id, StandOutLayoutParams @params, View folderView, ImageView screenshot, Drawable drawable)
			{
				this.OuterInstance = outerInstance;
				this.Id = id;
				this.@params = @params;
				this.FolderView = folderView;
				this.Screenshot = screenshot;
				this.Drawable = drawable;
			}


			public void OnAnimationStart(Animation animation)
			{
			}

			public void OnAnimationRepeat(Animation animation)
			{
			}

			public void OnAnimationEnd(Animation animation)
			{
				FolderView.Visibility = (ViewStates.Gone);

				// post so that the folder is invisible
				// before
				// anything else happens
				Screenshot.Post(() =>
			{

				// preview should be centered
				// vertically
				@params.Y = @params.Y + @params.Height / 2 - Drawable.IntrinsicHeight / 2;

				@params.Width = Drawable.IntrinsicWidth;
				@params.Height = Drawable.IntrinsicHeight;

				OuterInstance.updateViewLayout(Id, @params);

				Screenshot.Visibility = (ViewStates.Visible);
				Screenshot.StartAnimation(OuterInstance.MFadeIn);
			});
			}
		}

		private class AnimationListenerAnonymousInnerClass2 : Java.Lang.Object, Animation.IAnimationListener
		{
			private readonly FloatingFolder OuterInstance;

			private int Id;
			private Window Window;
			private StandOutLayoutParams @params;
			private View FolderView;
			private ImageView Screenshot;

			public AnimationListenerAnonymousInnerClass2(FloatingFolder outerInstance, int id, Window window, StandOutLayoutParams @params, View folderView, ImageView screenshot)
			{
				this.OuterInstance = outerInstance;
				this.Id = id;
				this.Window = window;
				this.@params = @params;
				this.FolderView = folderView;
				this.Screenshot = screenshot;
			}


			public void OnAnimationStart(Animation animation)
			{
				Log.Debug("FloatingFolder", "Animation started");
			}

			public void OnAnimationRepeat(Animation animation)
			{
			}

			public void OnAnimationEnd(Animation animation)
			{
				Log.Debug("FloatingFolder", "Animation ended");
				Screenshot.Visibility = (ViewStates.Gone);

				// post so that screenshot is invisible
				// before anything else happens
				Screenshot.Post(() =>
			{

				StandOutLayoutParams originalParams = OuterInstance.getParams(Id, Window);

				Drawable drawable = Screenshot.Drawable;
				Screenshot.SetImageDrawable(null);

				@params.Y = @params.Y - originalParams.Height / 2 + drawable.IntrinsicHeight / 2;

				@params.Width = originalParams.Width;
				@params.Height = originalParams.Height;

				OuterInstance.updateViewLayout(Id, @params);

				FolderView.Visibility = (ViewStates.Visible);

				FolderView.StartAnimation(OuterInstance.MFadeIn);
			});
			}
		}

		public string GetPersistentNotificationMessage(int id)
		{
			return "Click to close all windows.";
		}

		public Intent GetPersistentNotificationIntent(int id)
		{
			return StandOutWindow.getCloseAllIntent(this, typeof(FloatingFolder));
		}

		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: @Override public java.util.List<DropDownListItem> getDropDownItems(final int id)
		public override IList<DropDownListItem> getDropDownItems(int id)
		{
			IList<DropDownListItem> items = new List<DropDownListItem>();
			FolderModel folder = MFolders.Get(id);

			// add
			var xg = new Action(() =>
				{
					sendData(id, typeof(FloatingFolder), APP_SELECTOR_ID, APP_SELECTOR_CODE, null);

				});
			Runnable rex = new Runnable(xg);
			items.Add(new DropDownListItem(this, Android.Resource.Drawable.IcMenuAdd, "Add Application", rex));
			if (folder.Apps.Count > 0)
			{
				// clear all
				var g = new Action(() =>
				{
					FolderModel folder2 = MFolders.Get(id);

					// copy to new array so we don't remove items while
					// we
					// iterate
					IList<ActivityInfo> apps = new List<ActivityInfo>(folder2.Apps);

					foreach (ActivityInfo app in apps)
					{
						OnUserRemoveApp(id, app);
					}
				});
				Runnable r = new Runnable(g);
				items.Add(new DropDownListItem(this, Android.Resource.Drawable.IcMenuDelete, "Clear All", r));
			}
			return items;
		}
	}
}