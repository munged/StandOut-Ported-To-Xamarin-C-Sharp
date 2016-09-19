namespace wei.mark.floatingfolders
	{

	using StandOutWindow = wei.mark.standout.StandOutWindow;
	using Activity = Android.App.Activity;
	using Bundle = Android.OS.Bundle;
	using Android.App;

	[Activity(ExcludeFromRecents=true,Theme="@android:style/Theme.NoDisplay",Label = "@string/app_name")]
	[IntentFilter(new[] { "android.intent.action.MAIN" }, Categories = new[]{"android.intent.category.LAUNCHER"})]
	public class FloatingFoldersLauncher : Activity
		{
		/// <summary>
		/// Called when the activity is first created. </summary>
		protected override void OnCreate(Bundle savedInstanceState)
			{
			base.OnCreate(savedInstanceState);
			StandOutWindow.closeAll(this, typeof(FloatingFolder));
			FloatingFolder.ShowFolders(this);

			Finish();
			}
		}

	}