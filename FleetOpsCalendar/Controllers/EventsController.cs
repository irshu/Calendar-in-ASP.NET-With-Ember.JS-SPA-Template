﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FleetOpsCalendar.Filters;
using FleetOpsCalendar.Models;
using System.Xml.Linq;

namespace FleetOpsCalendar.Controllers
{
    [Authorize]
    public class EventListController : ApiController
    {
        private TodoItemContext db = new TodoItemContext();

        // GET api/TodoList
        public object GetEvents()
        {
            var query = XElement.Load(System.Web.HttpContext.Current.Server.MapPath("~/Markups/CalendarEvents.xml"))
                     .Elements("event")
                             .Select(p => new Event
                             {
                                 eventId = p.Element("id").Value,
                                 title = p.Element("title").Value,
                                 start = p.Element("start").Value,
                                 end = p.Element("end").Value,
                                 allDay = (bool)p.Element("allDay")
                             }).ToList();
            var result = new { EventList = query };
            return result;
            //return query;

        }

        // GET api/TodoList/5
        public TodoListDto GetTodoList(int id)
        {
            TodoList todoList = db.TodoLists.Find(id);
            if (todoList == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (todoList.UserId != User.Identity.Name)
            {
                // Trying to modify a record that does not belong to the user
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }

            return new TodoListDto(todoList);
        }

        // PUT api/TodoList/5
      
        public HttpResponseMessage PutEvents(int id,Event _Event)
        {
            try
            {
                var XDoc = XDocument.Load(System.Web.HttpContext.Current.Server.MapPath("~/Markups/CalendarEvents.xml"));
                var query = XDoc.Descendants("Events").Elements("event")
                            .Where(p => p.Element("id").Value==id.ToString()).FirstOrDefault();

                if (query!=null) {
                    query.Element("title").Value = _Event.title;
                }
                XDoc.Save(System.Web.HttpContext.Current.Server.MapPath("~/Markups/CalendarEvents.xml"));

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        // POST api/TodoList
        [ValidateHttpAntiForgeryToken]
        public HttpResponseMessage PostTodoList(TodoListDto todoListDto)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            todoListDto.UserId = User.Identity.Name;
            TodoList todoList = todoListDto.ToEntity();
            db.TodoLists.Add(todoList);
            db.SaveChanges();
            todoListDto.TodoListId = todoList.TodoListId;

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, todoListDto);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = todoListDto.TodoListId }));
            return response;
        }

        // DELETE api/TodoList/5
        [ValidateHttpAntiForgeryToken]
        public HttpResponseMessage DeleteTodoList(int id)
        {
            TodoList todoList = db.TodoLists.Find(id);
            if (todoList == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (db.Entry(todoList).Entity.UserId != User.Identity.Name)
            {
                // Trying to delete a record that does not belong to the user
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            TodoListDto todoListDto = new TodoListDto(todoList);
            db.TodoLists.Remove(todoList);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }

            return Request.CreateResponse(HttpStatusCode.OK, todoListDto);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}