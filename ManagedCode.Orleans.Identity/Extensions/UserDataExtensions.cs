using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedCode.Orleans.Identity.Extensions
{
    public static class UserDataExtensions
    {
        public static string AsString(this HashSet<string> values, string delimiter = ";")
        {
            return string.Join(delimiter, values);
        }
    }
}
