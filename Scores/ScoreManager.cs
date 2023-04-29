using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

/***************************************************************************************
*  REFERENCES  
*  Title: MonoGame Tutorial 015 - Store Highscores (XML)
*  Author: Oyyou
*  Date: 5/6/2018
*  Code version: N/A
*  URL (source code): https://github.com/Oyyou/MonoGame_Tutorials/blob/master/MonoGame_Tutorials/Tutorial015/Managers/ScoreManager.cs
*  URL (tutorial): https://www.youtube.com/watch?v=JzEwVCgALuY
*  Software License: N/A
*  
***************************************************************************************/


namespace Ascent.Scores
{
    public class ScoreManager
    {
        private static string _fileName = "scores.xml"; // Since we don't give a path, this'll be saved in the "bin" folder
        public Dictionary<int, float> bestTimes { get; private set; }

        public ScoreManager(): this(new Dictionary<int, float>()){}

        public ScoreManager(Dictionary<int, float> times)
        {
            this.bestTimes = times;
        }

        public void Add(int level, float score)
        {
            if (bestTimes.ContainsKey(level)){
                if (score < bestTimes[level]) {
                    bestTimes[level] = score;
                }
            }
            else
            {
                bestTimes[level] = score;
            }
            Save(this);
        }

        public static ScoreManager Load()
        {
            // If there isn't a file to load - create a new instance of "ScoreManager"
            if (!File.Exists(_fileName))
                return new ScoreManager();

            // Otherwise we load the file
            using (var reader = XmlDictionaryReader.CreateTextReader(new FileStream(_fileName, FileMode.Open), new XmlDictionaryReaderQuotas()))
            {
                var serializer = new DataContractSerializer(typeof(Dictionary<int, float>));
                var scores = (Dictionary<int, float>)serializer.ReadObject(reader);
                return new ScoreManager(scores);
            }
        }

        public static void Save(ScoreManager scoreManager)
        {
            // Overrides the file if it already exists
            using (var writer = XmlDictionaryWriter.CreateTextWriter(new FileStream(_fileName, FileMode.Create)))
            {
                var serializer = new DataContractSerializer(typeof(Dictionary<int, float>));
                serializer.WriteObject(writer, scoreManager.bestTimes);
            }
        }
    }
}