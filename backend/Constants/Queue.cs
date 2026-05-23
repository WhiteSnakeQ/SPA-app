namespace SPA_app.Constants
{
	public static class Queue
	{
		public static class Comment
		{
			public const string ExchangeName = "comment.created";
			public const string CacheClean = "comment.created.cache";
			public const int CacheCleanCount = 8;
			public const string SignalR = "comment.created.signalr";
            public const int SignalRCount = 2;
            public const string IndexSearch = "comment.created.search";
            public const int IndexSearchCount = 4;
			public const int IndexSearchInsertAtOnce = 200;
        }
		public static class Files
		{
			public const string ExchangeName = "files.created";
			public const string ResizeImage = "files.created.resize";
            public const int ResizeImageCount = 14;
            public const string FileCreated = "files.created.db";
            public const int FileCreatedCount = 14;
        }
	}
}
