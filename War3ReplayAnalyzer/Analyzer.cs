//#define USE_RATIO
#define USE_BIGRAMS

using System;
using System.Collections.Generic;
using Deerchao.War3Share.W3gParser;
using System.Diagnostics;

namespace War3ReplayAnalyzer
{
	public class Analyzer
	{
		Dictionary<string, int> mPositiveCount;
		Dictionary<string, int> mNegativeCount;
		int mPositiveTotal;
		int mNegativeTotal;

		int mTotalSuccess;
		int mTotalFailures;

		int mErrorTotal;
		int mInvalidTotal;

		char[] mDelimiters = { ' ', ';', ':', ',', '.', '{', '}', '[', ']', '(', ')','?','\\','/' };

		public Analyzer ()
		{
			mPositiveCount = new Dictionary<string, int>();
			mNegativeCount = new Dictionary<string, int>();
			mPositiveTotal = 0;
			mNegativeTotal = 0;
			mTotalSuccess = 0;
			mTotalFailures = 0;
			mErrorTotal = 0;
			mInvalidTotal = 0;
		}

		public double GetAccuracy()
		{
			return (mTotalSuccess / (double)(mTotalSuccess + mTotalFailures));
		}

		public double GetError()
		{
			return (mErrorTotal/(double)(mTotalSuccess + mTotalFailures));
		}

		public double GetInvalid()
		{
			return (mInvalidTotal/(double)(mTotalSuccess + mTotalFailures));
		}

		public int Classify(string filename)
		{
			Replay replay;
			try{
				replay = new Replay (filename);
			}catch(Exception e){
				return -1;
			}
			if (replay.Chats.Count <= 0) {
				mInvalidTotal++;
				Console.WriteLine ("Invalid Replay");
				return -1;
			}
			Dictionary<int, double> teamPositive = new Dictionary<int, double> ();
			Dictionary<int, double> teamNegative = new Dictionary<int, double> ();
			foreach (Team t in replay.Teams) {
				if (t.IsObserver == false) {
					teamPositive.Add (t.TeamNo, 1.0);
					teamNegative.Add (t.TeamNo, 1.0);
				}
			}

			Dictionary<string, bool> wordsFinished = new Dictionary<string, bool> ();
			foreach (ChatInfo ci in replay.Chats) {
				if (teamPositive.ContainsKey (ci.From.TeamNo)) {
					//Console.WriteLine (ci.Message);
					foreach (string s in ci.Message.Split(mDelimiters,1000)) {
						if (!wordsFinished.ContainsKey (s)) {
							int pos = 0;
							int neg = 0;
							if (mPositiveCount.ContainsKey (s)) {
								pos = mPositiveCount [s];
							}
							if (mNegativeCount.ContainsKey (s)) {
								neg = mNegativeCount [s];
							}
							if (pos < 0 || neg < 0) {
								Console.WriteLine ("ERROR");
							}
							//teamPositive [ci.From.TeamNo] *= (pos + 1) / (double)(2 * mPositiveTotal);
							//teamNegative [ci.From.TeamNo] *= (neg + 1) / (double)(2 * mNegativeTotal);
							teamPositive[ci.From.TeamNo] += Math.Log10(1 + pos/(double)mPositiveTotal);
							teamNegative[ci.From.TeamNo] += Math.Log10(1 + neg/(double)mNegativeTotal);
							wordsFinished.Add (s, true);
							//Console.WriteLine ("\tprob pos: " + ci.From.TeamNo + ", " + s + ", " + pos + ", " + teamPositive [ci.From.TeamNo]);
							//Console.WriteLine ("\tprob neg: " + ci.From.TeamNo + ", " + s + ", " + neg + ", " + teamNegative [ci.From.TeamNo]);
						}
					}
				}
			}

			double best = 0.0;
			int bestteam = -1;
			foreach (Team t in replay.Teams) {
				if (t.IsObserver == false) {
					//Console.WriteLine ("Team No: " + t.TeamNo + ", Positive: " + teamPositive [t.TeamNo] + ", Negative: " + teamNegative [t.TeamNo]);
					#if USE_RATIO
					if (teamPositive [t.TeamNo] / teamNegative [t.TeamNo] > best) {
						best = teamPositive [t.TeamNo] / teamNegative [t.TeamNo];
						bestteam = t.TeamNo;
					}
					#else
					if (teamPositive [t.TeamNo] > best && teamNegative[t.TeamNo]<teamPositive[t.TeamNo]) {
						bestteam = t.TeamNo;
						best = teamPositive [t.TeamNo];
					}
					#endif
				}
			}
			if (bestteam == -1) { //again
				foreach (Team t in replay.Teams) {
					if (t.IsObserver == false) {
						//Console.WriteLine ("Team No: " + t.TeamNo + ", Positive: " + teamPositive [t.TeamNo] + ", Negative: " + teamNegative [t.TeamNo]);
						#if USE_RATIO
						if (teamPositive [t.TeamNo] / teamNegative [t.TeamNo] > best) {
							best = teamPositive [t.TeamNo] / teamNegative [t.TeamNo];
							bestteam = t.TeamNo;
						}
						#else
						if (teamPositive [t.TeamNo] > best) {
							bestteam = t.TeamNo;
							best = teamPositive [t.TeamNo];
						}
						#endif
					}
				}
			}

			if (bestteam == -1) {
				Console.WriteLine ("ERROR: all bad teams");
				mErrorTotal++;
				return -1;
			}
				
			int realbest = GetWinningTeam (replay);
			if (bestteam == realbest) {
				Console.WriteLine ("Success - Predicted: "+bestteam+", Actual: "+realbest);
				mTotalSuccess++;
			} else {
				Console.WriteLine ("Failure - Predicted: "+bestteam+", Actual: "+realbest);
				mTotalFailures++;
			}
			return bestteam;
		}

		public int GetWinningTeam(Replay replay)
		{
			int big = 0;
			int winnerteam = -1;
			foreach (Team t in replay.Teams) {
				if (t.IsObserver == false) {
					//Console.WriteLine ("Looking at team no: " + t.TeamNo);
					foreach(Player p in t.Players)
					{
						//Console.WriteLine ("Looking at player no: " + p.Id);
						int ts = p.Time;
						if (ts > big) {
							big = ts;
							winnerteam = t.TeamNo;
						}
						//Console.WriteLine ("\tTime: " + ts);
					}
				}
			}
			return winnerteam;
		}

		public void AddReplay(string filename)
		{
			Replay replay;
			try{
			replay = new Replay (filename);
			}catch(Exception e){
				return;
			}
			if (replay.Chats.Count <= 0) {
				return;
			}
			//Console.WriteLine ("Map: " + replay.Map.GetName());
			//Console.WriteLine ("Players: " + replay.Players.Count);

			int winnerteam = GetWinningTeam (replay);
			//Console.WriteLine ("Winner team: " + winnerteam);

			foreach (ChatInfo ci in replay.Chats) {
				//Console.WriteLine (ci.From.Id+": "+ci.Message);
				if (ci.From.TeamNo == winnerteam) {
					//winner team
					foreach (string s in ci.Message.Split(mDelimiters,1000)) {
						if (mPositiveCount.ContainsKey (s)) {
							mPositiveCount [s]++;
						} else {
							mPositiveCount.Add (s, 1);
						}
						mPositiveTotal++;
						//Console.WriteLine ("\tadded positive: " + s);
					}
					
				} else {
					//loser team
					foreach (string s in ci.Message.Split(mDelimiters,1000)) {
						if (mNegativeCount.ContainsKey (s)) {
							mNegativeCount [s]++;
						} else {
							mNegativeCount.Add (s, 1);
						}
						mNegativeTotal++;
						//Console.WriteLine ("\tadded negative: " + s);
					}
				}
			}
		}
	}
}

