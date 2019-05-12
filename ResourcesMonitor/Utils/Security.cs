using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Utils
{
	public class Security
	{
		public static bool ValidateCredentials(string login, string password)
		{
			using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, Constants.DOMAIN_NAME))
			{
                if (login.Contains("@"))
                {
                    var userName = login.Split(new char[]{'@'})[0];
                    login = $"{Constants.DOMAIN_NAME}\\{userName}";
                }
				return pc.ValidateCredentials(login, password);
			}
		}
	}
}