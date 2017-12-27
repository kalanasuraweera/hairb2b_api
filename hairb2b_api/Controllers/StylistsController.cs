using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using hairb2b_api.Models;
using System.Diagnostics;

namespace hairb2b_api.Controllers
{
    public class StylistsController : ApiController
    {
        List<Stylist> stylists;
        List<string> stylistNames;
        public StylistsController()
        {
            this.stylists = new List<Stylist>();
            this.stylistNames = new List<string>();
            this.stylists.Add(new Stylist(1, "Simon", "educator", 30, 8));
            this.stylists.Add(new Stylist(2, "Sanath", "stylist", 20, 6));
            this.stylists.Add(new Stylist(3, "Joe", "stylist", 25, 6));
            this.stylists.Add(new Stylist(4, "Ramani", "educator", 30, 9));
            this.stylists.ElementAt(0).addBusySlots(2017, 12, 29, 'm');
            this.stylists.ElementAt(0).addBusySlots(2017, 12, 29, 'e');
            this.stylists.ElementAt(1).addBusySlots(2017, 12, 30, 'm');
            this.stylists.ElementAt(1).addBusySlots(2017, 12, 30, 'e');
            foreach(var x in stylists)
            {
                stylistNames.Add(x.name);
            }

        }


        //public IEnumerable<List<DateTime>> getbusyDates(int id, int month)
        //{            
        //    var stylist = this.stylists.FirstOrDefault(p => p.Id == id);
        //    return Ok(stylist.getBusyDates());
        //}
        [HttpGet]
        public IHttpActionResult getBusyDates(int month,int stylistId)
        {
            var stylist = stylists.Find(p => p.id == stylistId);
            
                if (stylist == null)
            {
                return NotFound();
             
            }
            return Ok(stylist.getBusySlots(month));
        }

        [HttpGet]
        public IHttpActionResult getDummyStylistCards()
        {
            List<Stylist.Stylist_card> cards=new List<Stylist.Stylist_card>();
            cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(0)));
            cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(1)));
            cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(2)));
            cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(3)));
            return Ok(cards);
        }

        [HttpGet]
        public IHttpActionResult getStylistNames()
        {
            return Ok(this.stylistNames);
        }

        [HttpGet]
        public IHttpActionResult getBasicSearchResults(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Ok(stylists);
            }
            else
            {
                var results = stylists.FindAll(p => p.name == name);
                //Debug.WriteLine(string.IsNullOrEmpty(name));
                return Ok(results);
            }
            
        }

        [HttpGet]
        public IHttpActionResult getAdvancedSearchResults(string searchBy, string advancedSearchName, int fromDay,int fromMonth,int fromYear, int toDay, int toMonth, int toYear,  int serviceCharge)
        {
            List<Stylist> results = new List<Stylist>(stylists);
            if (searchBy == "name")
            {
                results = results.FindAll(p => p.name == advancedSearchName);
            }
            if (searchBy == "skills")
            {

            }
            if(fromDay!=0 && toDay!=0)
            {
                results = results.FindAll(p => checkAvailability(p,fromDay,fromMonth,fromYear,toDay,toMonth,toYear));
            }
            if(serviceCharge!=0)
            {
                results = results.FindAll(p => p.costPerSlot <= serviceCharge);
            }
            return Ok(results);

        }

        public Boolean checkAvailability(Stylist stylist,int fromDay, int fromMonth, int fromYear, int toDay, int toMonth, int toYear)
        {
            //foreach(Stylist.TimeSlot i in stylist.BusySlots )
            //{
            //    if(i.year>=fromYear && i.year<=toYear)
            //    {
            //        if(i.month>=fromMonth && i.month<=toMonth)
            //        {
            //            if(i.day>=fromDay && i.day <= toDay)
            //            {
            //                return false;
            //            }
            //        }
            //    }
            //}
            DateTime from = new DateTime(fromYear, fromMonth, fromDay);
            DateTime to = new DateTime(toYear, toMonth, toDay);
            foreach(Stylist.TimeSlot i in stylist.BusySlots)
            {
                DateTime current = new DateTime(i.year, i.month, i.day);
                if(DateTime.Compare(current,from)>=0 && DateTime.Compare(current,to)<=0)
                {
                    return false;
                }
            }
            return true;
        }




    }


}
