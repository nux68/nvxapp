using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.service.Helpers
{
    public static class nvxReflection
    {
        public static List<object> GetObjectsOfType<T>(object padre)
        {

            List<object> retVal = new List<object>();

            Type childType = padre.GetType();
            FieldInfo[] fields = childType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);


            foreach (FieldInfo field in fields)
            {
                var o = field.GetValue(padre);
                if (o is T)
                {
                    retVal.Add(o);
                }
            }
            return retVal;

        }
    }
}
