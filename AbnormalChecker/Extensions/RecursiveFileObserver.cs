using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Java.IO;

namespace AbnormalChecker.Extensions
{
	public delegate void OnFileObserverEvent(FileObserverEvents ev, string path);

	public class RecursiveFileObserver : FileObserver
	{
		public const FileObserverEvents ChangesOnly = FileObserverEvents.CloseWrite | FileObserverEvents.MoveSelf |
		                                              FileObserverEvents.MovedFrom | FileObserverEvents.Delete |
		                                              FileObserverEvents.Modify | FileObserverEvents.MovedTo |
		                                              FileObserverEvents.DeleteSelf | FileObserverEvents.Create |
		                                              FileObserverEvents.Attrib;

		private List<SingleFileObserver> mObservers;
		private readonly string mPath;
		private readonly FileObserverEvents mMask;
		private readonly OnFileObserverEvent mEvent;

		public RecursiveFileObserver(string path, OnFileObserverEvent ev,
			FileObserverEvents mask = FileObserverEvents.AllEvents) : base(path, mask)
		{
			mPath = path;
			mMask = mask;
			mEvent = ev;
		}

		public override void StartWatching()
		{
			if (mObservers != null) return;
			mObservers = new List<SingleFileObserver>();
			Stack<string> stack = new Stack<string>();
			stack.Push(mPath);
			while (stack.Count != 0)
			{
				string parent = stack.Pop();
				mObservers.Add(new SingleFileObserver(this, parent, mMask));
				File path = new File(parent);
				File[] files = path.ListFiles();
				if (files == null) continue;
				foreach (File file in files)
				{
					if (file.IsDirectory && file.Name != "." && file.Name != "..")
					{
						stack.Push(file.Path);
					}
				}
			}

			foreach (SingleFileObserver observer in mObservers)
				observer.StartWatching();
		}

		public override void StopWatching()
		{
			if (mObservers == null) return;
			foreach (SingleFileObserver observer in mObservers)
				observer.StopWatching();
			mObservers.Clear();
			mObservers = null;
		}

		public override void OnEvent(FileObserverEvents e, string path)
		{
			mEvent?.Invoke(e, path);
		}

		private class SingleFileObserver : FileObserver
		{
			private readonly string mPath;
			private readonly RecursiveFileObserver mRecursiveFileObserver;

			public SingleFileObserver(RecursiveFileObserver parent, string path, FileObserverEvents mask)
				: base(path, mask)
			{
				mRecursiveFileObserver = parent;
				mPath = path;
			}

			public override void OnEvent(FileObserverEvents ev, string path)
			{
				mRecursiveFileObserver.OnEvent(ev, mPath + "/" + path);
			}
		}
	}
}