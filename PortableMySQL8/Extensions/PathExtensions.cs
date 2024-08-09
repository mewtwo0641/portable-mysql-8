using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableMySQL8
{
    public static class PathExtensions
    {
        public static string FixDirSeperators(this string path)
        {
            string ret = path.Replace("@\\", "/");
            ret = ret.Replace(@"\", "/");
            return ret;
        }
    }
}
