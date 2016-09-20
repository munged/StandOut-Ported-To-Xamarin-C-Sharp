using System.Collections.Generic;
using System.Threading;

namespace wei.mark.floatingfolders
{

	using Context = Android.Content.Context;
	using ActivityInfo = Android.Content.PM.ActivityInfo;
	using PackageManager = Android.Content.PM.PackageManager;
	using Drawable = Android.Graphics.Drawables.Drawable;
	using LayoutInflater = Android.Views.LayoutInflater;
	using View = Android.Views.View;
	using ViewGroup = Android.Views.ViewGroup;
	using ImageView = Android.Widget.ImageView;
	using TextView = Android.Widget.TextView;
	using Android.Widget;
	using Android;
	using Java.Lang;
	using System;

	public class AppAdapter : ArrayAdapter<ActivityInfo>
	{
		internal class ViewHolder: Java.Lang.Object
		{
			private readonly AppAdapter OuterInstance;

			public ViewHolder(AppAdapter outerInstance)
			{
				this.OuterInstance = outerInstance;
			}

			internal ImageView Icon;
			internal TextView Name;
			internal int Position;
		}

		internal LayoutInflater MInflater;
		internal PackageManager MPackageManager;
		internal int MTextViewResourceId;

		public AppAdapter(Context context, int textViewResourceId, IList<ActivityInfo> objects) : base(context, textViewResourceId, objects)
		{

			MInflater = LayoutInflater.From(context);
			MPackageManager = context.PackageManager;
			MTextViewResourceId = textViewResourceId;
		}
		void non()
		{
			string s = "I am able to spend 34 nights without x sleep";
			var ch = new[] { 'c', '3', 'x', ' ' };
			foreach(var c in ch )
			{
				s= s.Replace(c.ToString(),"");
			}
			Console.Write(s);
		}
		//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		//ORIGINAL LINE: @Override public android.view.View getView(final int position, android.view.View convertView, android.view.ViewGroup parent)
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final ViewHolder holder;
			ViewHolder holder;
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final android.content.pm.ActivityInfo app = getItem(position);
			ActivityInfo app = GetItem(position);

			if (convertView == null)
			{
				convertView = MInflater.Inflate(MTextViewResourceId, parent, false);
				holder = new ViewHolder(this);

				holder.Icon = (ImageView)convertView.FindViewById(Resource.Id.Icon);
				holder.Name = (TextView)convertView.FindViewById(Advanced.Resource.String.app_name);

				convertView.SetTag(position, holder);
			}
			else
			{
				holder = (ViewHolder)convertView.Tag;
			}
			//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			//ORIGINAL LINE: final android.view.View view = convertView;
			View view = convertView;

			holder.Position = position;

			// don't block the UI thread
			(new Thread(() =>
		{
			var label = app.LoadLabel(MPackageManager);
			Drawable drawable = app.LoadIcon(MPackageManager);
			view.Post(() =>
			{

				if (holder.Position == position)
				{
					holder.Name.Text = (label);
					holder.Icon.SetImageDrawable(drawable);
				}
			});
		})).Start();

			return convertView;
		}
	}

}
