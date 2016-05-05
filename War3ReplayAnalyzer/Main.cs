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
			int learnCount = 20;
			int testCount = 80;
			for (int i = 0; i < learnCount; i++) {
				Console.WriteLine ("Added replay : " + (i+1));
				analyzer.AddReplay (@"data\Replays\Replate - "+(i+1)+".w3g");
			}
			for(int i = learnCount+1;i<learnCount+testCount;i++)
			{
				Console.WriteLine ("Classifying: " + (i+1));
				analyzer.Classify (@"data\Replays\Replate - "+(i+1)+".w3g");
			}

			Console.WriteLine ("Accuracy: " + analyzer.GetAccuracy ());
			Console.WriteLine ("Error: " + analyzer.GetError ());
			Console.WriteLine ("Invalid: " + analyzer.GetInvalid ());

			Console.ReadLine();
		}
	}
}
