using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;

namespace Dialogue
{
    public class Tools
    {
        /// <summary>
        /// Checks the file to see if it exists and if it's the correct version
        /// </summary>
        /// <returns></returns>
        public bool CheckFile(string path, string version)
        {
            if (File.Exists(path))
            {
                JSONNode Node = JSON.Parse(ArrayToString(File.ReadAllLines(path)));
                if (version == Node["version"].ToString().Trim('"'))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks the file to see if it exists
        /// </summary>
        public bool CheckFile(string path)
        {
            if (File.Exists(path))
            {
                return true;
                
            }
            return false;
        }

        /// <summary>
        /// Converts a string of array into one string, each array value is separated by a line break (\n)
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <returns>A string with all the array values</returns>
        public static string ArrayToString(string[] array)
        {
            string result = string.Join("\n", array);
            return result;
        }
    }
}
