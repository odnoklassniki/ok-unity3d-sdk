using UnityEngine;

namespace Odnoklassniki
{
	public class WebGLOdnoklassniki : AbstractOdnoklassniki
	{
		public override void Auth(OKAuthCallback callback)
		{
			Debug.Log("WEB GL Auth!");
		}

		protected override string GetAppUrl()
		{
			return "";
		}
	}
}