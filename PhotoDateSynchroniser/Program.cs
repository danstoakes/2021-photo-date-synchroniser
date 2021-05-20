/*
 * Author: Dan Stoakes
 * Purpose: Synchronises the file date values across the Date Created, Date Modified,
 *          and Date Taken EXIF metadata within the file.
 * Date: 12/05/2021
 */

using System;
using System.IO;

namespace PhotoDateFixer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Ensure that the input and output directories are specified.
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: PhotoDateSynchroniser.cs input_directory output_directory");
                return 1;
            }
            // Get the command-line arguments as input and output path variables.
            string path = args[0];
            string outputFilePath = args[1];
            // Ensure that the input and output directories are different.
            if (path.Equals(outputFilePath))
            {
                Console.WriteLine("The input and output directories need to be different.");
                return 1;
            }

            try
            {
                // Check if the output directory exists and create it if it does not.
                if (!Directory.Exists(outputFilePath))
                    Directory.CreateDirectory(outputFilePath);
            } catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return 1;
            }
            // Assign the files to an array depending on if its a single file or directory.
            string[] files = null;
            if (File.Exists(path))
            {
                files = new string[1];
                files[0] = path;
            }
            else if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path);
            }
            // Ensure that there is at least one file to use.
            if (files == null)
            {
                Console.WriteLine("The input argument does not point to a valid file/folder.");
                return 1;
            }
            // Instantiate the EXIFDateHandler class.
            EXIFDateHandler exifDateHandler = new EXIFDateHandler();
            foreach (string filePath in files)
            {
                // Ensure that the file has a valid extension and is one of the allowed types.
                if (Path.HasExtension(filePath) && IsValidExtension(Path.GetExtension(filePath)))
                {
                    exifDateHandler.SetFilePath(filePath);
                    exifDateHandler.SetOutputFilePath(outputFilePath);
                    // Assign the date created, modified, and taken values to variables.
                    DateTime created = File.GetCreationTime(filePath);
                    DateTime modified = File.GetLastWriteTime(filePath);
                    DateTime? taken = exifDateHandler.GetDateTaken();
                    // Ensure that the created and modified dates are not null or the default.
                    if (created != null && created.Year != 1601 && modified != null && modified.Year != 1601)
                    {
                        DateTime earliest = exifDateHandler.GetEarliestDate(created, modified);
                        // Ensure that the Date Taken value is assigned and not null.
                        if (taken != null)
                            earliest = exifDateHandler.GetEarliestDate(earliest, (DateTime)taken);
                        // Pass the earliest date into the EXIFDateHandler object, whereby order matters.
                        bool setTaken = exifDateHandler.SetDateTaken(earliest);
                        bool setCreated = exifDateHandler.SetDateCreated(earliest);
                        bool setModified = exifDateHandler.SetDateModified(earliest);

                        if (!(setTaken && setCreated && setModified))
                            Console.WriteLine("Some values were incorrectly set for: " + filePath);
                    }
                }
            }
            return 0;
        }

        // Returns whether the current image file extension is valid
        private static bool IsValidExtension(string extension)
        {
            string[] validExtensions = { ".jpg", ".png", ".gif" };
            // Loop through each of the valid extensions.
            foreach (string validExtension in validExtensions)
            {
                // Return true if the extension is valid.
                if (extension.ToLower().Contains(validExtension))
                    return true;
            }
            return false;
        }
    }
}