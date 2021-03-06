// 
// FlightViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Jeffrey Stedfast
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
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.SQLite;
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightViewController : SQLiteTableViewController<Flight>, IComparer<Flight>
	{
		static readonly NSString FlightTableViewCellKey = new NSString ("Flight");

		readonly NSIndexPath[] selectedRow = new NSIndexPath[2];
		UIBarButtonItem addFlight;
		NSIndexPath updating;
		bool searching;
		bool disposed;
		bool loner;
		
		public FlightViewController ()
		{
			RowHeight = FlightTableViewCell.CellHeight;
			SearchPlaceholder = "Search Flights";
			AutoHideSearch = true;
			Title = "Flights";
			
			LogBook.FlightAdded += OnFlightAdded;
			LogBook.FlightUpdated += OnFlightUpdated;
			LogBook.FlightWillUpdate += OnFlightWillUpdate;
			LogBook.FlightUpdateFailed += OnFlightUpdateFailed;
		}
		
		#region IComparer[Flight] implementation
		int IComparer<Flight>.Compare (Flight x, Flight y)
		{
			// If the Id's are identical, then they are the same flight log entry.
			if (x.Id == y.Id)
				return 0;
			
			// Compare flight dates in descending order.
			int cmp = DateTime.Compare (y.Date, x.Date);
			
			if (cmp == 0)
				return y.Id - x.Id;
			
			return cmp;
		}
		#endregion
		
		UITableView CurrentTableView {
			get {
				return searching ? SearchDisplayController.SearchResultsTableView : TableView;
			}
		}

		public FlightDetailsViewController DetailsViewController {
			get; set;
		}

		public Flight FirstOrSelected {
			get {
				if (DetailsViewController.Flight != null)
					return DetailsViewController.Flight;

				if (IsViewLoaded)
					return ModelForTableView (CurrentTableView).GetItem (0);

				// we haven't been loaded yet, so we'll have to create a new model...
				using (var model = CreateModel (false))
					return model.GetItem (0);
			}
		}
		
		void OnFlightAdded (object sender, FlightEventArgs e)
		{
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			DetailsViewController.Flight = e.Flight;
			model.ReloadData ();
			
			int index = model.IndexOf (e.Flight, this);
			
			if (index == -1) {
				// This suggests that we are probably displaying the search view and the
				// newly added flight log entry does not match the search criteria.
				
				// Just reload the original TableView...
				Model.ReloadData ();
				TableView.ReloadData ();
				return;
			}
			
			int section, row;
			if (!model.IndexToSectionAndRow (index, out section, out row)) {
				// This shouldn't happen...
				model.ReloadData ();
				tableView.ReloadData ();
				return;
			}

			var path = NSIndexPath.FromRowSection (row, section);

			if (model.GetRowCount (section) == 1) {
				// The new flight entry is in a new section...
				var sections = NSIndexSet.FromIndex (section);
				tableView.InsertSections (sections, UITableViewRowAnimation.Automatic);
				sections.Dispose ();
			} else {
				// Add the row for the new flight log entry...
				var rows = new NSIndexPath[1];
				rows[0] = path;
				
				tableView.InsertRows (rows, UITableViewRowAnimation.Automatic);
			}
			
			// Select and scroll to the newly added flight log entry...
			
			// From Apple's documentation:
			//
			// To scroll to the newly selected row with minimum scrolling, select the row using
			// selectRowAtIndexPath:animated:scrollPosition: with UITableViewScrollPositionNone,
			// then call scrollToRowAtIndexPath:atScrollPosition:animated: with
			// UITableViewScrollPositionNone.
			tableView.SelectRow (path, true, UITableViewScrollPosition.None);
			tableView.ScrollToRow (path, UITableViewScrollPosition.None, true);
			path.Dispose ();
		}
		
		void OnFlightWillUpdate (object sender, FlightEventArgs e)
		{
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			updating = PathForVisibleItem (tableView, e.Flight);
			if (updating != null) {
				// We're done.
				loner = model.GetRowCount (updating.Section) == 1;
				return;
			}
			
			// Otherwise we gotta do things the hard way...
			int index = model.IndexOf (e.Flight, this);
			int section, row;
			
			if (index == -1 || !model.IndexToSectionAndRow (index, out section, out row))
				return;
			
			updating = NSIndexPath.FromRowSection (row, section);
			loner = model.GetRowCount (section) == 1;
		}
		
		void OnFlightUpdateFailed (object sender, FlightEventArgs e)
		{
			if (updating != null) {
				updating.Dispose ();
				updating = null;
			}
		}
		
		void OnFlightUpdated (object sender, FlightEventArgs e)
		{
			if (updating == null) {
				// The user probably just saved a flight which doesn't match the search criteria.
				Model.ReloadData ();
				TableView.ReloadData ();
				return;
			}
			
			// No matter what, reload the main model.
			Model.ReloadData ();
			
			// Now update the UITableView that is currently being displayed.
			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);
			
			// The date may have changed, which means the position may have changed.
			model.ReloadData ();
			
			// Find the new position of the flight log entry.
			int index = model.IndexOf (e.Flight, this);
			int section, row;
			
			if (index == -1 || !model.IndexToSectionAndRow (index, out section, out row)) {
				// The flight no longer exists in this model/view...
				if (loner) {
					// The flight log entry was a 'loner', e.g. it was the only flight in its section.
					var sections = NSIndexSet.FromIndex (updating.Section);
					tableView.DeleteSections (sections, UITableViewRowAnimation.Automatic);
					sections.Dispose ();
				} else {
					var rows = new NSIndexPath[1];
					rows[0] = updating;

					tableView.DeleteRows (rows, UITableViewRowAnimation.Automatic);
				}
			} else if (updating.Section != section || updating.Row != row) {
				// The flight changed position in the current table view - need to move it.
				var path = NSIndexPath.FromRowSection (row, section);
				tableView.MoveRow (updating, path);
				path.Dispose ();
			} else {
				// Flight is in the same location, just needs to update its values...
				var rows = new NSIndexPath[1];
				rows[0] = updating;
				
				tableView.ReloadRows (rows, UITableViewRowAnimation.None);
			}
			
			// If the currently displayed UITableView isn't the main view, reset state of the main tableview.
			if (tableView != TableView)
				TableView.ReloadData ();
			
			updating.Dispose ();
			updating = null;
		}
		
		void OnFlightDeleted (UITableView tableView, NSIndexPath indexPath)
		{
			var model = ModelForTableView (tableView);
			
			if (model.GetRowCount (indexPath.Section) > 1) {
				// The section contains more than just this row, so delete only the row.
				var rows = new NSIndexPath[1];
				rows[0] = indexPath;
				
				// Reset the models...
				SearchModel.ReloadData ();
				Model.ReloadData ();
				
				tableView.DeleteRows (rows, UITableViewRowAnimation.Automatic);
			} else {
				// This section only has the row the user is deleting, so remove the entire section.
				using (var sections = NSIndexSet.FromIndex (indexPath.Section)) {
					// Reset the models...
					SearchModel.ReloadData ();
					Model.ReloadData ();

					tableView.DeleteSections (sections, UITableViewRowAnimation.Automatic);
				}
			}
			
			if (tableView != TableView) {
				// We've already deleted the item from the search model, but we
				// have no way of knowing its section/row in the normal model.
				TableView.ReloadData ();
			} else if (Model.SectionCount == 0) {
				OnAddClicked (null, null);
				return;
			}

			// Update the selection...
			int section, row;
			int count;

			if (indexPath.Section >= model.SectionCount) {
				// Select the last row of the last section
				section = model.SectionCount - 1;
				row = model.GetRowCount (section) - 1;
			} else {
				section = indexPath.Section;
				count = model.GetRowCount (section);
				if (indexPath.Row < count)
					row = indexPath.Row;
				else
					row = count - 1;
			}

			NSIndexPath path = NSIndexPath.FromRowSection (row, section);
			tableView.SelectRow (path, true, UITableViewScrollPosition.None);
			RowSelected (tableView, path);
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (DetailsViewController.EditorEngaged)
				return;
			
			DetailsViewController.Edit (new Flight (DateTime.Today), false);
		}
		
		protected override SQLiteTableModel<Flight> CreateModel (bool forSearching)
		{
			var model = new SQLiteTableModel<Flight> (LogBook.SQLiteDB, 16);
			
			model.OrderBy.Add (new SQLiteOrderBy ("Date", SQLiteSortOrder.Descending));
			model.OrderBy.Add (new SQLiteOrderBy ("Id", SQLiteSortOrder.Descending));
			
			model.SectionExpression = "strftime ('%Y', Date)";
			
			return model;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			addFlight = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			NavigationItem.LeftBarButtonItem = addFlight;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			var tableView = CurrentTableView;
			var model = ModelForTableView (tableView);

			if (model.SectionCount == 0) {
				if (!searching) {
					// Add new flight...
					OnAddClicked (null, null);
				}
			} else {
				var path = selectedRow[searching ? 1 : 0];

				if (path == null) {
					// Select first flight in view...
					var visible = tableView.IndexPathsForVisibleRows;
					if (visible == null || visible.Length == 0)
						return;

					path = visible[0];
				}
				
				tableView.SelectRow (path, false, UITableViewScrollPosition.None);
				RowSelected (tableView, path);
			}
		}
		
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath, Flight flight)
		{
			var cell = tableView.DequeueReusableCell (FlightTableViewCellKey) as FlightTableViewCell;
			
			if (cell == null)
				cell = new FlightTableViewCell (FlightTableViewCellKey);
			
			cell.Flight = flight;
			
			return cell;
		}
		
		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}
		
		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return UITableViewCellEditingStyle.Delete;
		}
		
		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;
			
			Flight flight = GetItem (tableView, indexPath);
			
			if (!LogBook.Delete (flight))
				return;
			
			OnFlightDeleted (tableView, indexPath);
		}
		
		protected override void DidBeginSearch (UISearchDisplayController controller)
		{
			searching = true;
		}
		
		protected override void DidEndSearch (UISearchDisplayController controller)
		{
			selectedRow[1] = null;
			searching = false;
		}
		
		static bool PathsEqual (NSIndexPath path0, NSIndexPath path1)
		{
			return path0.Section == path1.Section && path0.Row == path1.Row;
		}
		
		public override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
		{
			var selected = tableView.IndexPathForSelectedRow;
			if (selected != null && !PathsEqual (selected, indexPath))
				tableView.DeselectRow (selected, false);
			
			return indexPath;
		}
		
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			DetailsViewController.Flight = GetItem (tableView, indexPath);
			selectedRow[searching ? 1 : 0] = indexPath;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (updating != null) {
					updating.Dispose ();
					updating = null;
				}

				if (!disposed) {
					LogBook.FlightAdded -= OnFlightAdded;
					LogBook.FlightUpdated -= OnFlightUpdated;
					LogBook.FlightWillUpdate -= OnFlightWillUpdate;
					LogBook.FlightUpdateFailed -= OnFlightUpdateFailed;
					disposed = true;
				}
			}

			base.Dispose (disposing);
		}
	}
}
