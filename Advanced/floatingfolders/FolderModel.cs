using System.Collections.Generic;

namespace wei.mark.floatingfolders
	{


	using ActivityInfo = Android.Content.PM.ActivityInfo;

	public class FolderModel
		{
		public int Id;
		public string Name;
		public IList<ActivityInfo> Apps;
		public bool Shown;
		public bool FullSize;
		public int Width;
		public int Height;

		public FolderModel()
			{
			Apps = new List<ActivityInfo>();
			Shown = true;
			FullSize = true;
			}
		}

	}