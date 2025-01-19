using Civil3DArbitraryCoordinate.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civil3DArbitraryCoordinate.Utils
{
    public static class EnumGenericUtils
    {
        public static T Parse<T>(string input)
        {
            T theEnum = default(T);

            foreach (string name in Enum.GetNames(typeof(T)))
            {
                if (name.Equals(input, StringComparison.CurrentCultureIgnoreCase))
                {
                    theEnum = (T)Enum.Parse(typeof(T), input, true);
                    return theEnum;
                }
            }

            return theEnum;
        }
    }
}
