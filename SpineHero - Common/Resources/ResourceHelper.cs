using System;
using System.IO;
using System.Reflection;

namespace SpineHero.Common.Resources
{
    public static class ResourceHelper
    {
        public static string GetAssemblyCompanyAndProductNameDirectory(Assembly assembly)
        {
            return Path.Combine(assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company,
                assembly.GetCustomAttribute<AssemblyProductAttribute>().Product);
        }

        /// <summary>
        /// Returns path to Application data directory (AppData/Local) for the current user.
        /// </summary>
        /// <returns>Path to Application data (Local) directory for the current user.</returns>
        public static string GetAppDataLocalDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        /// <summary>
        /// Returns path to Application data directory (AppData/Roaming) for the current roaming user.
        /// </summary>
        /// <returns>Path to Application data (Roaming) directory for the current roaming user.</returns>
        public static string GetAppDataRoamingDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary>
        /// Returns path to given resource (e.g. @"Icons\Image.png") that is included in provided or calling assembly.
        /// </summary>
        /// <param name="resource">Directory and name of the resource</param>
        /// <param name="assembly"></param>
        /// <returns>String containing path to given resource.</returns>
        public static string GetResourcePath(string resource, Assembly assembly = null)
        {
            return @"pack://application:,,,/"
                + (assembly?.GetName().Name ?? Assembly.GetCallingAssembly().GetName().Name)
                + ";component/"
                + resource;
        }

        /// <summary>
        /// Returns path to given local resource of provided or calling assembly (e.g. @"Resources\Text.txt").
        /// </summary>
        /// <param name="resource">Directory and name of the resource</param>
        /// <param name="assembly"></param>
        /// <returns>String containing path to given resource.</returns>
        public static string GetLocalResourcePath(string resource, Assembly assembly = null)
        {
            return GetAssemblyDirectory(assembly) + @"\" + resource;
        }

        /// <summary>
        /// Gets the directory of provided or calling assembly.
        /// </summary>
        private static string GetAssemblyDirectory(Assembly assembly)
        {
            string codeBase = assembly?.CodeBase ?? Assembly.GetCallingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Returns path to given resource (e.g. @"Resources\Text.txt") which is in user's AppData/Local folder (AssemblyCompany/AssemblyProduct) based on provided or calling assembly.
        /// </summary>
        /// <param name="resource">Directory and name of the resource</param>
        /// <param name="assembly"></param>
        /// <returns>String containing path to given resource.</returns>
        public static string GetAppDataLocalResourcePath(string resource, Assembly assembly = null)
        {
            return Path.Combine(GetAppDataLocalDirectory(), GetAssemblyCompanyAndProductNameDirectory(assembly ?? Assembly.GetCallingAssembly()), resource);
        }
    }
}