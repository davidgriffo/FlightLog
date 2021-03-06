// 
// LimitedEntryElement.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Jeffrey Stedfast
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class LimitedEntryElement : EntryElement
	{
		static readonly NSString LimitedEntryElementCellKey = new NSString ("LimitedEntryElement");

		public LimitedEntryElement (string caption, string placeholder)
			: base (caption, placeholder, null)
		{
			MaxLength = -1;
		}
		
		public LimitedEntryElement (string caption, string placeholder, int maxLength)
			: base (caption, placeholder, null)
		{
			MaxLength = maxLength;
		}
		
		public LimitedEntryElement (string caption, string placeholder, string value)
			: base (caption, placeholder, value)
		{
			MaxLength = -1;
		}
		
		public LimitedEntryElement (string caption, string placeholder, string value, bool isPassword)
			: base (caption, placeholder, value, isPassword)
		{
			MaxLength = -1;
		}
		
		public LimitedEntryElement (string caption, string placeholder, string value, int maxLength)
			: base (caption, placeholder, value)
		{
			MaxLength = maxLength;
		}
		
		public LimitedEntryElement (string caption, string placeholder, string value, bool isPassword, int maxLength)
			: base (caption, placeholder, value, isPassword)
		{
			MaxLength = maxLength;
		}

		protected override NSString CellKey {
			get { return LimitedEntryElementCellKey; }
		}
		
		/// <summary>
		/// Gets or sets the maximum allowable input length.
		/// </summary>
		/// <value>
		/// The maximum allowable input length, or <c>-1</c> for infinite.
		/// </value>
		public int MaxLength {
			get; set;
		}
		
		protected override UITextField CreateTextField (RectangleF frame)
		{
			UITextField entry = base.CreateTextField (frame);
			
			entry.ShouldChangeCharacters += OnShouldChangeCharacters;
			entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			entry.AdjustsFontSizeToFitWidth = true;
			entry.Ended += OnEditingCompleted;
			
			return entry;
		}
		
		/// <summary>
		/// Decides whether or not a text change is allowed to be made.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the text change should be allowed to happen or <c>false</c> otherwise.
		/// </returns>
		/// <param name="currentText">
		/// The current text content of the entry.
		/// </param>
		/// <param name="changedRange">
		/// The range of characters in currentText being changed.
		/// </param>
		/// <param name='replacementText'>
		/// The text which will replace the range of text in currentText.
		/// </param>
		/// <param name='result'>
		/// The text that will result in the change.
		/// </param>
		protected virtual bool AllowTextChange (string currentText, NSRange changedRange, string replacementText, string result)
		{
			if (MaxLength < 0)
				return true;

			return result.Length <= MaxLength;
		}
		
		bool OnShouldChangeCharacters (UITextField field, NSRange range, string text)
		{
			string currentText = field.Text;
			string result;
			
			result = currentText.Substring (0, range.Location) + text + currentText.Substring (range.Location + range.Length);
			if (!AllowTextChange (currentText, range, text, result))
				return false;
			
			val = result;
			
			return true;
		}

		void OnEditingCompleted (object sender, EventArgs e)
		{
			if (EditingCompleted != null)
				EditingCompleted (this, EventArgs.Empty);
		}

		public event EventHandler EditingCompleted;
	}
}
