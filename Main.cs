using System;
using System.Threading;
using System.Collections.Generic;
using YoutubeExtractor;
using System.IO;
using System.Linq;

namespace YouTubeFeast
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("YouTubeFeast version "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			Console.WriteLine("(C) Daniel Kirstenpfad 2012 - http://www.technology-ninja.com");
			Console.WriteLine();
			
			YouTubeFeastConfiguration.ReadConfiguration("YouTubeFeast.configuration");
			
			Console.WriteLine();
			Console.WriteLine("to quit please use control-c.");
			Console.WriteLine();
			
			while (true)
			{
				// we have to decide if there is a job we need to work on
				
				foreach(ChannelJob job in YouTubeFeastConfiguration.DownloadJobs)
				{					
					TimeSpan SinceLastRun = DateTime.Now - job.LastDownload;
					TimeSpan theInterval = new TimeSpan(job.Interval,0,0);
					if ( SinceLastRun  >= theInterval )
					{
						// we should download something... or at least look for new stuff
						List<String> DownloadURLs = YoutubeDownload.GenerateDownloadURLsFromChannel(job.ChannelURL);
						job.LastDownload = DateTime.Now;
						
						// it seems that we got a nice list here, now let's
						if (DownloadURLs.Count > 0)
						{
							// start the downloadings...
							// oh there is a policy: the first file that already exists leads to the abortion of this particular channel download
							// that's because this tool expects the new files to appear first on the channel page and the old ones to be listed later
							// on the page
							
							foreach (String url in DownloadURLs)
							{
								// get all the available video formats for this one...
								IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
								VideoInfo video = videoInfos.First(info => info.VideoFormat == job.DownloadVideoFormat);
								
								String filename = Path.Combine(job.ChannelDownloadDirectory, video.Title + video.VideoExtension);
								
								if (File.Exists(filename))
								{
									Console.WriteLine("File: "+filename+" already exists - we stop this channel job now.");
									break;
								}
								else
									Console.WriteLine("Downloading: "+filename);
								var videoDownloader = new VideoDownloader(video, filename);
								//videoDownloader.ProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);
								 videoDownloader.Execute();
							}
						}
					}	
				}
				
				Thread.Sleep(10000);
			}
		}
	}
}
