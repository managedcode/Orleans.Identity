using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedCode.Orleans.Identity.Extensions
{
    public static class UserDataExtensions
    {
        public static string AsString(this HashSet<string> values, string delimiter = ",")
        {
            if(values.Count >= 1)
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (string value in values)
                {
                    stringBuilder.Append(delimiter + value);
                    stringBuilder.Append(delimiter);
                }

                return stringBuilder.ToString();
            }
            else
                return values.First();
        }
    }
}
