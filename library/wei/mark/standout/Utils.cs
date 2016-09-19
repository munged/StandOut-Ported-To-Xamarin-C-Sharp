namespace wei.mark.standout
{

	public class Utils
	{
		public static bool isSet(int flags, int flag)
		{
			return (flags & flag) == flag;
		}
	}

}