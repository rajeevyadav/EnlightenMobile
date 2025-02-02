﻿using System;
using Xamarin.Forms;
using Android.Widget;
using Java.IO;
using EnlightenMobile.Services;
using EnlightenMobile;
using Android.Bluetooth;

[assembly: Dependency(typeof(EnlightenMobile.Droid.PlatformUtil))]
namespace EnlightenMobile.Droid
{ 
    public class PlatformUtil : IPlatformUtil
    {
        Logger logger = Logger.getInstance();
        string savePath;

        public PlatformUtil() { }
        
        public bool bluetoothEnabled()
        {
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var enabled = bluetoothAdapter.IsEnabled;
            logger.debug($"PlatformUtil<Android>.bluetoothEnabled = {enabled}");
            return enabled;
        }

        // @see https://stackoverflow.com/a/43754366/11615696
        public bool enableBluetooth(bool flag)
        {
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            var enabled = bluetoothAdapter.IsEnabled;
            if (flag == enabled)
            {
                logger.debug($"enableBluetooth: naught to do! ({flag})");
                return true;
            }

            logger.debug($"enableBluetooth: setting {flag}");
            if (flag)
                bluetoothAdapter.Enable();
            else
                bluetoothAdapter.Disable();

            return true;
        }

        // Make a little pop-up message notification appear (no buttons, it just 
        // fades away after a few seconds).  Currently used when a Measurement 
        // is successfully saved.
        public void toast(string message, View view = null)  
        {  
            var context = Android.App.Application.Context;  
            Toast.MakeText(context, message, ToastLength.Long).Show();  
        }  

        public string getSavePath()
        {
            if (savePath != null)
            {
                logger.debug($"getSavePath: returning previous savePath {savePath}");
                return savePath;
            }

            // create EnlightenSpectra if necessary
            string defaultPath = System.IO.Path.Combine((Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments).AbsolutePath), "EnlightenSpectra");
            //string defaultPath = string.Format("{0}/{1}",Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,"EnlightenSpectra");
            logger.debug($"getSavePath: defaultPath = {defaultPath}");
            if (!writeable(defaultPath))
            {
                logger.error($"getSavePath: unable to write defaultPath {defaultPath}");
                return null;
            }

            // create today if necessary
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string todayPath = string.Format("{0}/{1}", defaultPath, today);
            logger.debug($"getSavePath: todayPath {todayPath}");
            if (!writeable(todayPath))
            {
                logger.error($"getSavePath: unable to write todayPath {todayPath}");
                return null;
            }
                
            logger.debug($"getSavePath: returning writeable todayPath {todayPath}");
            return savePath = todayPath;
        }

        bool writeable(string path)
        {
            File f = new File(path);
            logger.debug($"writeable: testing {path}");
            if (f.Exists())
                logger.debug($"exists: {path}");
            else
            {
                logger.debug($"calling Mkdirs({path})");
                f.Mkdirs();
                if (!f.Exists())
                {
                    logger.error($"writeable: Mkdirs failed to create {path}");
                    return false;
                }
            }

            if (!f.CanWrite())
            {
                logger.error($"writeable: can't write: {path}");
                return false;
            }

            return true;
        }
    }
}
