﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 11/1/2012
 * Time: 6:46 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace UIAutomation
{
	using System;
	using System.Collections.Generic;
	//using System.Windows.Automation;
	using System.Drawing;
	
	/// <summary>
	/// Description of ExecutionPlan.
	/// </summary>
	public static class ExecutionPlan
	{
		static ExecutionPlan()
		{
			Init();
		}
		
		public static Queue<Highlighter> HighlightersQueue { get; set; }
		public static int HighlightersMaxCount { get; set; }
		internal static int HighlighterNumber { get; set; }
        internal static KnownColor[] colorTable = {
        	KnownColor.Red,
        	KnownColor.Green,
        	KnownColor.Blue,
        	KnownColor.Yellow,
        	KnownColor.Pink,
        	KnownColor.SeaGreen,
        	KnownColor.MediumVioletRed,
        	KnownColor.Magenta,
        	KnownColor.YellowGreen,
        	KnownColor.DarkBlue
        };
		
		public static void Init()
		{
			HighlightersQueue =
				new Queue<Highlighter>();
			HighlightersMaxCount = 100;
			HighlighterNumber = 0;
		}
		
		public static void DisposeHighlighers()
		{
			try {
				foreach (Highlighter highLighter in HighlightersQueue) {
					highLighter.Dispose();
				}
				HighlightersQueue.Clear();
				HighlighterNumber = 0;
			}
			catch {
			}
		}
		
		internal static void DecreaseMaxCount(int newCount)
		{
			if (newCount < HighlightersQueue.Count) {
				while (newCount < HighlightersQueue.Count) {
					Highlighter highlighterToBeDisposed = 
						HighlightersQueue.Dequeue();
					highlighterToBeDisposed.Dispose();
				}
			}
			HighlightersMaxCount = newCount;
		}
		
		internal static void DecreaseQueue(int newCount)
		{
		    if (newCount >= HighlightersQueue.Count) return;
		    while (newCount < HighlightersQueue.Count) {
		        Highlighter highlighterToBeDisposed = 
		            HighlightersQueue.Dequeue();
		        highlighterToBeDisposed.Dispose();
		    }

		    /*
			if (newCount < HighlightersQueue.Count) {
				while (newCount < HighlightersQueue.Count) {
					Highlighter highlighterToBeDisposed = 
						HighlightersQueue.Dequeue();
					highlighterToBeDisposed.Dispose();
				}
			}
            */
		}

	    internal static void Enqueue(Highlighter highLighter)
		{
			if (null == highLighter) {
				return;
			}
			
			if (HighlightersMaxCount <= HighlightersQueue.Count) {
				DecreaseQueue(HighlightersMaxCount - 1);
			}
			
			HighlightersQueue.Enqueue(highLighter);
		}
		
		public static void Enqueue(
	        // 20131109
			//AutomationElement elementToHighlight,
			IMySuperWrapper elementToHighlight,
			// 20131204
			// int highlightersGeneration,
		    string highlighterData)
		{
		    // 20131109
		    //if (null == (elementToHighlight as AutomationElement)) return;
		    if (null == elementToHighlight) return;
            /*
            if (null == (elementToHighlight as IMySuperWrapper)) return;
            */
            // 20131204
//		    if (0 >= highlightersGeneration) {
//		        HighlighterNumber++;
//		    } else {
//		        HighlighterNumber = highlightersGeneration;
//		    }
            if (0 == CommonCmdletBase.HighlighterGeneration) {
                CommonCmdletBase.HighlighterGeneration++;
            }
				
		    Highlighter highlighter = new Highlighter(
		        elementToHighlight.Current.BoundingRectangle.Height,
		        elementToHighlight.Current.BoundingRectangle.Width,
		        elementToHighlight.Current.BoundingRectangle.X,
		        elementToHighlight.Current.BoundingRectangle.Y,
		        elementToHighlight.Current.NativeWindowHandle,
		        // 20131204
		        // (Highlighters)(HighlighterNumber % 10),
		        (Highlighters)(CommonCmdletBase.HighlighterGeneration % 10),
		        // 20131204
		        // HighlighterNumber,
		        CommonCmdletBase.HighlighterGeneration,
		        highlighterData);
		    Enqueue(highlighter);

		    /*
            if (null != (elementToHighlight as AutomationElement)) {

				if (0 >= highlightersGeneration) {
					HighlighterNumber++;
				} else {
					HighlighterNumber = highlightersGeneration;
				}
				
				highlighter =
                    new Highlighter(
                        elementToHighlight.Current.BoundingRectangle.Height,
                        elementToHighlight.Current.BoundingRectangle.Width,
                        elementToHighlight.Current.BoundingRectangle.X,
                        elementToHighlight.Current.BoundingRectangle.Y,
                        elementToHighlight.Current.NativeWindowHandle,
                        (Highlighters)(HighlighterNumber % 10),
                        HighlighterNumber,
                        highlighterData);
				ExecutionPlan.Enqueue(highlighter);
			}
            */
        }
	}
}
