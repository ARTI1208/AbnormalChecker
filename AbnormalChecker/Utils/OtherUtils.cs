using System;
using System.Collections.Generic;
using System.IO;
using Android.Util;
using Java.IO;
using Java.Util.Zip;
using Console = System.Console;
using File = Java.IO.File;

namespace AbnormalChecker.Utils
{
	public class OtherUtils
	{
		private static void DirToZip(File dir, ZipOutputStream outputStream)
		{
			var files = dir.ListFiles();

			if (files.Length == 0)
			{
				var entry = new ZipEntry(dir.Name);

				try
				{
					outputStream.PutNextEntry(entry);
					outputStream.CloseEntry();
				}
				catch (Exception e)
				{
					Log.Error(nameof(CreateZipArchive), "Exception, ex: " + e);
				}
			}

			var dirEntry = new ZipEntry(dir.Name);
			outputStream.PutNextEntry(dirEntry);

			foreach (var t in files)
				if (t.IsFile)
					FileToZip(t, outputStream);
				else
					DirToZip(t, outputStream);
		}

		private static void FileToZip(File file, ZipOutputStream outputStream)
		{
			BufferedInputStream origin = null;
			try
			{
				var buffer = 2048;
				var data = new byte[buffer];
				var fi = new FileStream(file.AbsolutePath, FileMode.Open);
				origin = new BufferedInputStream(fi, buffer);
				var entry = new ZipEntry(file.Name);
				outputStream.PutNextEntry(entry);
				int count;
				while ((count = origin.Read(data, 0, buffer)) != -1) outputStream.Write(data, 0, count);
			}
			finally
			{
				origin?.Close();
			}
		}

		public static bool CreateZipArchive(IList<File> files, string zipFile)
		{
			try
			{
				var dest = new FileStream(zipFile, FileMode.Create);
				var outputStream = new ZipOutputStream(new BufferedStream(dest));
				foreach (var file in files)
					if (file.IsFile)
						FileToZip(file, outputStream);
					else
						DirToZip(file, outputStream);

				outputStream.Finish();
				outputStream.Close();
			}
			catch (Exception e)
			{
				Log.Error(nameof(CreateZipArchive), e.Message);
				return false;
			}

			return true;
		}

		public static bool UnpackZipArchive(string zipPath, File pathToExtract)
		{
			var buffer = 2048;
			FileStream inputStream;
			ZipInputStream zis;
			try
			{
				string filename;
				inputStream = new FileStream(zipPath, FileMode.Open);
				zis = new ZipInputStream(new BufferedStream(inputStream));
				ZipEntry ze;
				var data = new byte[buffer];
				int count;

				while ((ze = zis.NextEntry) != null)
				{
					filename = ze.Name;

					// Need to create directories if not exists, or
					// it will generate an Exception...
					if (ze.IsDirectory)
					{
						var fmd = new File(pathToExtract, filename);
						fmd.Mkdirs();
						continue;
					}

					var current = new File(pathToExtract, filename);

					Log.Debug(nameof(UnpackZipArchive), current.AbsolutePath);

					var fout = new FileOutputStream(current);

					while ((count = zis.Read(data)) != -1) fout.Write(data, 0, count);

					fout.Close();
					zis.CloseEntry();
				}

				zis.Close();
			}
			catch (Exception e)
			{
				Log.Error(nameof(UnpackZipArchive), e.Message);
				return false;
			}

			return true;
		}
	}
}