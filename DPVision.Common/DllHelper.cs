using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DPVision.Common
{
    public class DllHelper
    {
        #region 程序集特性访问器

        public static string  GetAssemblyTitle(Assembly assembly)
        {
           
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(assembly.CodeBase);
            
        }

        public static string GetAssemblyVersion(Assembly assembly)
        {
          
                return assembly.GetName().Version.ToString();
            
        }

        public static string GetAssemblyDescription(Assembly assembly)
        {
           
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            
        }

        public static string GetAssemblyProduct(Assembly assembly)
        {
            
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            
        }

        public static string GetAssemblyCopyright(Assembly assembly)
        {
           
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            
        }

        public static string GetAssemblyCompany(Assembly assembly)
        {
           
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            
        }

        public static string GetAssemblyTrademark(Assembly assembly)
        {

            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return ((AssemblyTrademarkAttribute)attributes[0]).Trademark;

        }
        #endregion
    }
}
