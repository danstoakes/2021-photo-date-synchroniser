/*
 * Author: Dan Stoakes
 * Purpose: Performs operations on the Date Created, Date Modified, and Date Taken EXIF metadata.
 * Date: 12/05/2021
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace PhotoDateFixer
{
    /*
     * The main EXIFDateHandler class
     * Contains all the methods for performing operations on EXIF metadata.
     */
    public class EXIFDateHandler
    {
        private const char NullTerminator = '\0';
        private const int DateTakenCode = 0x9003;
        private const int DateDigitisedCode = 0x9004;

        private string filePath;
        private string outputFilePath;

        // Sets the path for the current image file.
        public void SetFilePath(string filePath)
        {
            this.filePath = filePath;
        }

        // Sets the output path for the current image file.
        public void SetOutputFilePath(string outputFilePath)
        {
            this.outputFilePath = outputFilePath;
        }

        // Gets the EXIF metadate property items for the current image file.
        private PropertyItem[] GetPropertyItems(Image image)
        {
            return image.PropertyItems;
        }

        // Gets the output path for the current image file.
        private string GetOutputPath()
        {
            return outputFilePath + "\\" + Path.GetFileName(filePath);
        }

        // Returns whether or not the current image file has
        // the Date Taken or Date Distributed property item.
        private bool HasDateTaken(Image image)
        {
            PropertyItem[] propertyItems = GetPropertyItems(image);
            // Ensure that the file has some properties.
            if (propertyItems.Length == 0)
                return false;
            // Loop through each property item.
            foreach (PropertyItem propertyItem in propertyItems)
            {
                // Check if the property item is for the Date Taken or
                // the Date Digitised value.
                if (propertyItem.Id == DateTakenCode || propertyItem.Id == DateDigitisedCode)
                    return true;
            }

            return false;
        }

        // Returns the earliest of two input DateTime objects.
        public DateTime GetEarliestDate(DateTime date1, DateTime date2)
        {
            DateTime earliest = date1;
            // Check between the input dates and get the earliest.
            if (DateTime.Compare(date1, date2) > 0)
                earliest = date2;

            return earliest;
        }

        // Sets the CreationTime attribute of the current
        // image file to the input DateTime object.
        public bool SetDateCreated(DateTime date)
        {
            Image image = new Bitmap(filePath);

            string newPath = GetOutputPath();
            // Save the file if it doesn't already exist.
            if (!File.Exists(newPath))
                image.Save(newPath);
            // Set the creation time of the file.
            File.SetCreationTime(newPath, date);

            return true;
        }

        // Sets the LastWriteTime attribute of the current
        // image file to the input DateTime object.
        public bool SetDateModified(DateTime date)
        {
            Image image = new Bitmap(filePath);

            string newPath = GetOutputPath();
            // Save the file if it doesn't already exist.
            if (!File.Exists(newPath))
                image.Save(newPath);
            // Set the creation time of the file.
            File.SetLastWriteTime(newPath, date);

            return true;
        }

        // Sets the DateTaken attribute of the current
        // image file to the input DateTime object.
        public bool SetDateTaken(DateTime date)
        {
            Image image = new Bitmap(filePath);
            Encoding encoding = Encoding.UTF8;
            // Ensure that the image has a Date Taken property.
            if (HasDateTaken(image))
            {
                PropertyItem dateTaken = image.GetPropertyItem(DateTakenCode);
                PropertyItem dateDigitised = image.GetPropertyItem(DateDigitisedCode);
                // Convert the date values into string objects and
                // assign them as values to the respective property items.
                dateTaken.Value = encoding.GetBytes(
                    date.ToString("yyyy:MM:dd HH:mm:ss") + NullTerminator);
                dateDigitised.Value = encoding.GetBytes(
                    date.ToString("yyyy:MM:dd HH:mm:ss") + NullTerminator);
                // Set the property items to the image.
                image.SetPropertyItem(dateTaken);
                image.SetPropertyItem(dateDigitised);

                string newPath = GetOutputPath();
                // Save the file if it doesn't already exist.
                if (!File.Exists(newPath))
                    image.Save(newPath);

                return true;
            }
            else
            {
                PropertyItem[] propertyItems = GetPropertyItems(image);
                // Ensure that the file has some properties.
                if (propertyItems.Length != 0)
                {
                    // Extract an existing property item to manipulate and
                    // set as a Date Time/Date Digitalised property. This is
                    // because PropertyItem has no public constructor.
                    PropertyItem propertyItem = propertyItems[0];
                    byte[] value = encoding.GetBytes(
                        date.ToString("yyyy:MM:dd HH:mm:ss") + NullTerminator);
                    // Assign the property item attributes, starting with Date Taken.
                    propertyItem.Id = DateTakenCode;
                    propertyItem.Len = value.Length;
                    propertyItem.Type = 2;
                    propertyItem.Value = value;
                    // Set the property item and repeat for Date Digitalised.
                    image.SetPropertyItem(propertyItem);
                    propertyItem.Id = DateDigitisedCode;
                    image.SetPropertyItem(propertyItem);

                    string newPath = GetOutputPath();
                    // Save the file if it doesn't already exist.
                    if (!File.Exists(newPath))
                        image.Save(newPath);

                    return true;
                }
            }
            image.Dispose();

            return false;
        }

        // Gets the DateTaken EXIF metadate property item
        // for the current image file
        public DateTime? GetDateTaken()
        {
            Image image = new Bitmap(filePath);
            Encoding encoding = Encoding.UTF8;
            // Ensure that the image has a date taken property.
            if (HasDateTaken(image))
            {
                // Get the Date Taken and Date Digitised properties.
                PropertyItem dateTaken = image.GetPropertyItem(DateTakenCode);
                PropertyItem dateDigitised = image.GetPropertyItem(DateDigitisedCode);
                // Get the properties as strings and remove the null terminator.
                string dateTakenString = encoding.GetString(dateTaken.Value);
                dateTakenString = dateTakenString.Remove(dateTakenString.Length - 1);
                string dateDigitisedString = encoding.GetString(dateDigitised.Value);
                dateDigitisedString = dateDigitisedString.Remove(dateDigitisedString.Length - 1);
                // Get Date Taken and Date Digitised properties as DateTime objects.
                DateTime dateTakenDate = DateTime.ParseExact(
                    dateTakenString, "yyyy:MM:dd HH:mm:ss", null);
                DateTime dateDigitisedDate = DateTime.ParseExact(
                    dateDigitisedString, "yyyy:MM:dd HH:mm:ss", null);
                // Return the earliest date of the two.
                return GetEarliestDate(dateTakenDate, dateDigitisedDate);
            }

            return null;
        }
    }
}