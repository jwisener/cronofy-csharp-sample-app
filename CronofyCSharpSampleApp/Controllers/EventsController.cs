﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CronofyCSharpSampleApp.Controllers
{
	public class EventsController : ControllerBase
	{
		public ActionResult Show(string id)
		{
			var shownEvent = CronofyHelper.ReadEvents().First(x => x.EventUid == id);

			ViewData["calendarName"] = CronofyHelper.GetCalendars().First(x => x.CalendarId == shownEvent.CalendarId).Name;
            ViewData["google_maps_embed_api_key"] = ConfigurationManager.AppSettings["google_maps_embed_api_key"];

			return View("Show", shownEvent);
		}

		public ActionResult New([Bind(Prefix = "id")] string calendarId)
        {
			var newEvent = new Models.Event
			{
				Calendar = CronofyHelper.GetCalendars().First(x => x.CalendarId == calendarId),
                CalendarId = calendarId,

				EventId = "unique_event_id_" + (new Random().Next(0, 1000000).ToString("D6"))
			};

			return View("New", newEvent);
		}

		public ActionResult Create(Models.Event newEvent)
		{
			if (newEvent.Start > newEvent.End)
			{
				ModelState.AddModelError("End", "End time cannot be before start time");
			}

            if (ModelState.IsValid)
			{
                CronofyHelper.UpsertEvent(newEvent.EventId, newEvent.CalendarId, newEvent.Summary, newEvent.Description, newEvent.Start, newEvent.End, new Cronofy.Location("", newEvent.Latitude, newEvent.Longitude));

                return new RedirectResult($"/calendars/show/{newEvent.CalendarId}");
            }
			
            newEvent.Calendar = CronofyHelper.GetCalendars().First(x => x.CalendarId == newEvent.CalendarId);

			return View("New", newEvent);
		}

		public ActionResult Edit(string id)
		{
			var gotEvent = CronofyHelper.ReadEvents().First(x => x.EventUid == id);

			ViewData["calendarName"] = CronofyHelper.GetCalendars().First(x => x.CalendarId == gotEvent.CalendarId).Name;

			var editEvent = new Models.Event
			{
				CalendarId = gotEvent.CalendarId,
				EventId = gotEvent.EventId,
				Summary = gotEvent.Summary,
				Description = gotEvent.Description,
				Start = (gotEvent.Start.HasTime ? gotEvent.Start.DateTimeOffset.DateTime : gotEvent.Start.Date.DateTime),
				End = (gotEvent.End.HasTime ? gotEvent.End.DateTimeOffset.DateTime : gotEvent.End.Date.DateTime)
			};

			return View("Edit", editEvent);
		}

		public ActionResult Update(Models.Event editEvent)
		{
			if (editEvent.Start > editEvent.End)
			{
				ModelState.AddModelError("End", "End time cannot be before start time");
			}

			if (ModelState.IsValid)
			{
				CronofyHelper.UpsertEvent(editEvent.EventId, editEvent.CalendarId, editEvent.Summary, editEvent.Description, editEvent.Start, editEvent.End, new Cronofy.Location("", editEvent.Latitude, editEvent.Longitude));

				return new RedirectResult($"/calendars/show/{editEvent.CalendarId}");
			}

			ViewData["calendarName"] = CronofyHelper.GetCalendars().First(x => x.CalendarId == editEvent.CalendarId).Name;

			return View("Edit", editEvent);
		}

		public ActionResult Delete(Models.Event deleteEvent)
		{
			CronofyHelper.DeleteEvent(deleteEvent.CalendarId, deleteEvent.EventId);

			return new RedirectResult($"/calendars/show/{deleteEvent.CalendarId}");
		}
    }
}
