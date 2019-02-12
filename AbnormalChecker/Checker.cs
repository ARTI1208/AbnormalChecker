using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Java.IO;

namespace AbnormalChecker
{
    public static class Checker
    {

        private static bool HasRootApp(Context context)
        {
            PackageManager pm = context.PackageManager;
            IList<PackageInfo> packages = pm.GetInstalledPackages(0);
            IList<string> pk = Array.ConvertAll(packages.ToArray(), input => input.PackageName);
            return pk.Contains("com.topjohnwu.magisk") || pk.Contains("eu.chainfire.supersu");
        }

        static readonly string[] SuBinaryPlaces =
        {
            "/sbin/", "/system/bin/", "/system/xbin/"
        };
        
        private static bool HasSuBinary()
        {
            return SuBinaryPlaces.Any(where => new File(where + "su").Exists());
        }

        public static string GetSuBinaryPath()
        {
            return Array.Find(SuBinaryPlaces, s => new File(s + "su").Exists());
        } 
            
        public static bool IsRooted(Context context)
        {
            return HasRootApp(context) && HasSuBinary();
        }
    }
}