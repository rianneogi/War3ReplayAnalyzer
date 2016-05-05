using System;
using System.Collections.Generic;
using War3ReplayAnalyzer;

namespace War3ReplayAnalyzer
{
	class MainClass
	{
		static void Main(String[] args)
		{
			Analyzer analyzer = new Analyzer ();
			int learnCount = 500;
			int testCount = 500;
			for (int i = 0; i < learnCount; i++) {
				string s = (i + 1).ToString();
				for (int x = s.Length; x < 3; x++) {
					s = '0' + s;
				}
				Console.WriteLine ("Added replay : " + s);
				analyzer.AddReplay (@"data\Replays\Replays - "+s+".w3g");
			}
			for(int i = learnCount+1;i<learnCount+testCount;i++)
			{
				string s = (i + 1).ToString();
				for (int x = s.Length; x < 3; x++) {
					s = '0' + s;
				}
				Console.WriteLine ("Classifying: " + s);
				analyzer.Classify (@"data\Replays\Replays - "+s+".w3g");
			}

			Console.WriteLine ("Accuracy: " + analyzer.GetAccuracy ());
			Console.WriteLine ("Error: " + analyzer.GetError ());
			Console.WriteLine ("Invalid: " + analyzer.GetInvalid ());

			Console.ReadLine();
		}
	}
}
