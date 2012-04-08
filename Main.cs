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
		public static void Main (string[] args2)
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
						Console.WriteLine("Updating: "+job.ChannelURL);
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
								
								VideoInfo video = null;
                                try
                                {
								    video = videoInfos.First(info => info.VideoFormat == job.DownloadVideoFormat);
                                }
                                catch(Exception)
                                {
                                    Console.WriteLine("\t\tError: Video with the desired resolution is not available.");
                                    //video = videoInfos.First(info => info.VideoFormat == VideoFormat.Standard360);
                                }
								
								if (video != null)
								{
									String filename = Path.Combine(job.ChannelDownloadDirectory, video.Title + video.VideoExtension);
									
									if (File.Exists(filename))
									{
										//Console.WriteLine("File: "+filename+" already exists - we stop this channel job now.");
										Console.WriteLine("\t\tNotice: We are finished with this channel.");
										break;
									}
									else
									{
                                        Console.Write("\t\tDownloading: " + ShortenString.LimitCharacters(video.Title, 40) + "...");
										var videoDownloader = new VideoDownloader(video, filename);

                                        Int32 left = Console.CursorLeft;
                                        Int32 top = Console.CursorTop;

										videoDownloader.ProgressChanged += (sender, args) => DisplayProgress(left,top,args.ProgressPercentage);
										
										videoDownloader.Execute();
                                        Console.WriteLine("done    ");
									}
								}
							}
						}
					}	
				}
				
				Thread.Sleep(60000);
			}
		}
		
		public static void DisplayProgress(Int32 left, Int32 top, double percentage)
		{
            Console.SetCursorPosition(left, top);
			Console.Write (Convert.ToInt32(percentage)+"%");
		}
	}
}
