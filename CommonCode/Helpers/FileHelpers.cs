using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonCode.Helpers
{
    public static  class FileHelpers
    {
        public static async Task<string> ReadTextFile(string filePath)
        {
            string? s = string.Empty;
            try
            {
                await using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
                using StreamReader reader = new StreamReader(fileStream);
                s = await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ReadTextFile: {ex.Message} {ex.ToString()}");
            }

            return s;
        }
    }
}
