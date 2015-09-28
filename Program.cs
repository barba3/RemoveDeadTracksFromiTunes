namespace RemoveDeadFiles
{
    using iTunesLib;
    using System;
    using System.IO;

    public class Program
    {
        public static int Main(string[] args)
        {
            bool delete;
            if (args.Length != 1)
            {
                ShowUsage();
                return 1;
            }

            string arg = args[0];
            if (arg.Equals("/whatif", StringComparison.OrdinalIgnoreCase))
            {
                delete = false;
            }
            else if (arg.Equals("/remove", StringComparison.OrdinalIgnoreCase))
            {
                delete = true;
            }
            else
            {
                ShowUsage();
                return 1;
            }

            FindDeadTracks(delete);
            return 0;
        }

        private static void ShowUsage()
        {
            string exeName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            Console.WriteLine(
                "{0} /whatif{1}\tShows you a list of tracks where it's file doesn't exist",
                exeName,
                Environment.NewLine);
            Console.WriteLine(
                "{0} /remove{1}\tRemoves all tracks where it's file doesn't exist",
                exeName, 
                Environment.NewLine);
        }

        private static void FindDeadTracks(bool delete)
        {
            Console.WriteLine("Connecting to iTunes");
            iTunesApp itunes = new iTunesApp();
            int trackCount = itunes.LibraryPlaylist.Tracks.Count;

            Console.WriteLine("Inspecting {0} tracks", trackCount);
            IITFileOrCDTrack fileOrCdTrack;
            int currentTrackIndex = 0;
            int numberOfDeadTracksFound = 0;
            foreach (IITTrack track in itunes.LibraryPlaylist.Tracks)
            {
                // Show progress
                if (currentTrackIndex % 500 == 0)
                {
                    Console.WriteLine(
                        "[{0:000}%] Inspected {1} tracks so far",
                        currentTrackIndex * 100 / trackCount,
                        currentTrackIndex);
                }

                currentTrackIndex++;

                // Filter out everything which is not a file track
                fileOrCdTrack = track as IITFileOrCDTrack;
                if (fileOrCdTrack == null || fileOrCdTrack.Kind != ITTrackKind.ITTrackKindFile)
                {
                    continue;
                }

                // Filter out tracks with known file location
                if (fileOrCdTrack.Location != null)
                {
                    continue;
                }

                // If we are here, the track has missing file
                numberOfDeadTracksFound++;
                Console.WriteLine(
                    "[{0:000}%] {1} dead track: {2} - {3}",
                    currentTrackIndex * 100 / trackCount,
                    delete ? "Removing" : "Found",
                    fileOrCdTrack.Artist, 
                    fileOrCdTrack.Name);
                if (delete)
                {
                    fileOrCdTrack.Delete();
                }
            }

            Console.WriteLine("[{0:000}%] Inspected {1} tracks", 100, trackCount);
            Console.WriteLine(
                "{0} {1} tracks where it's file doesn't exist",
                delete ? "Removed" : "Found",
                numberOfDeadTracksFound);
        }
    }
}
