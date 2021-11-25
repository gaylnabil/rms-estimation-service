using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace RMS_Estimation_Service.AssemblyInformation
{
    public class SimajeAssemblyInfo
    {
        public string Title { get; }
        public string Description { get; }

        public string Company { get; }

        public string Product { get; }

        public string Copyright { get; }

        public string Trademark { get; }

        public string AssemblyVersion { get; }

        public string FileVersion { get; }

        public string Guid { get; }

        public string NeutralLanguage { get; }

        public bool IsComVisible { get; }

        // The assembly information values.

        // Constructors.
        public SimajeAssemblyInfo()
            : this(Assembly.GetExecutingAssembly())
        {
        }

        public SimajeAssemblyInfo(Assembly assembly)
        {
            // Get values from the assembly.
            AssemblyTitleAttribute titleAttr =
                GetAssemblyAttribute<AssemblyTitleAttribute>(assembly);
            if (titleAttr != null) Title = titleAttr.Title;

            AssemblyDescriptionAttribute assemblyAttr =
                GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly);
            if (assemblyAttr != null) Description = assemblyAttr.Description;

            AssemblyCompanyAttribute companyAttr =
                GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly);
            if (companyAttr != null) Company = companyAttr.Company;

            AssemblyProductAttribute productAttr =
                GetAssemblyAttribute<AssemblyProductAttribute>(assembly);
            if (productAttr != null) Product = productAttr.Product;

            AssemblyCopyrightAttribute copyrightAttr =
                GetAssemblyAttribute<AssemblyCopyrightAttribute>(assembly);
            if (copyrightAttr != null) Copyright = copyrightAttr.Copyright;

            AssemblyTrademarkAttribute trademarkAttr =
                GetAssemblyAttribute<AssemblyTrademarkAttribute>(assembly);
            if (trademarkAttr != null) Trademark = trademarkAttr.Trademark;

            AssemblyVersion = assembly.GetName().Version.ToString();

            AssemblyFileVersionAttribute fileVersionAttr =
                GetAssemblyAttribute<AssemblyFileVersionAttribute>(assembly);
            if (fileVersionAttr != null) FileVersion = fileVersionAttr.Version;

            System.Runtime.InteropServices.GuidAttribute guidAttr =
                GetAssemblyAttribute<System.Runtime.InteropServices.GuidAttribute>(assembly);
            if (guidAttr != null) Guid = guidAttr.Value;

            System.Resources.NeutralResourcesLanguageAttribute languageAttr =
                GetAssemblyAttribute<System.Resources.NeutralResourcesLanguageAttribute>(assembly);
            if (languageAttr != null) NeutralLanguage = languageAttr.CultureName;

            System.Runtime.InteropServices.ComVisibleAttribute comAttr =
                GetAssemblyAttribute<System.Runtime.InteropServices.ComVisibleAttribute>(assembly);
            if (comAttr != null) IsComVisible = comAttr.Value;
        }



        // Return a particular assembly attribute value.
        public static T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {
            // Get attributes of this type.
            object[] attributes = assembly.GetCustomAttributes(typeof(T), true);

            // If we didn't get anything, return null.
            if ((attributes == null) || (attributes.Length == 0)) return null;

            // Convert the first attribute value into the desired type and return it.
            return (T) attributes[0];
        }
    }
}
