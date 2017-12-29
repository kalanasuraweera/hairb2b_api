using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using hairb2b_api.Models;
using System.Diagnostics;
using System.Data.SqlClient;


namespace hairb2b_api.Controllers
{
    public class StylistsController : ApiController
    {
        List<Stylist> stylists;
        List<string> stylistNames = new List<string>();      
        private string connString = "Data Source=eyepaxws-51\\eyepaxws51;Initial Catalog=hairb2b;Integrated Security=True";


        public StylistsController()
        {
            //this.stylists = new List<Stylist>();
            //this.stylistNames = new List<string>();
            //this.stylists.Add(new Stylist(1, "Simon", "educator", 30, 8));
            //this.stylists.Add(new Stylist(2, "Sanath", "stylist", 20, 6));
            //this.stylists.Add(new Stylist(3, "Joe", "stylist", 25, 6));
            //this.stylists.Add(new Stylist(4, "Ramani", "educator", 30, 9));
            //this.stylists.ElementAt(0).addBusySlots(2017, 12, 29, 'm');
            //this.stylists.ElementAt(0).addBusySlots(2017, 12, 29, 'e');
            //this.stylists.ElementAt(1).addBusySlots(2017, 12, 30, 'm');
            //this.stylists.ElementAt(1).addBusySlots(2017, 12, 30, 'e');
            //foreach(var x in stylists)
            //{
            //    stylistNames.Add(x.name);
            //}
            
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to get firstname and lastname of all users and put them in the stylistNames list
                SqlCommand command = new SqlCommand(@"SELECT firstName,lastName
                                                      from trnStylist;"
                                                        , conn);
                
                conn.Open();
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        this.stylistNames.Add(Convert.ToString(rdr["firstName"]));
                        this.stylistNames.Add(Convert.ToString(rdr["lastName"]));
                    }
                }
            }
            
            //try
            //{
            //    connection = new SqlConnection(connString);
            //    connection.Open();
            //    Debug.WriteLine("successfully connected to the database");
            //}
            //catch(Exception e)
            //{
            //    Debug.WriteLine(e);
            //}

        }

    
        [HttpGet]
        public IHttpActionResult getBusyDates(int month,int stylistId)
        {
            //var stylist = stylists.Find(p => p.id == stylistId);

            //    if (stylist == null)
            //{
            //    return NotFound();

            //}
            //return Ok(stylist.getBusySlots(month));
            Stylist st = new Stylist();
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to add the busy dates to a stylist object
                SqlCommand command = new SqlCommand(@"SELECT firstName,tb.date as date,tt.slot as slot 
                                                      from trnStylist ts,trnBusyDates tb,trnTimeSlot tt 
                                                      WHERE ts.id ="+stylistId + " and ts.id=tb.stylistId and tb.timeSlotId=tt.id;"                                                     
                                                        , conn);

                conn.Open();
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string slot = Convert.ToString(rdr["slot"]);
                        DateTime date = Convert.ToDateTime(rdr["date"]);
                        if(date.Month==month)
                            st.addBusySlots(date.Year, date.Month, date.Day, (slot == "morning" ? 'm' : 'e'));
                    }
                }
            }
            return Ok(st.getBusySlots());

        }

        [HttpGet]
        public IHttpActionResult getDummyStylistCards()
        {
            //List<Stylist.Stylist_card> cards = new List<Stylist.Stylist_card>();
            //cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(0)));
            //cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(1)));
            //cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(2)));
            //cards.Add(new Stylist.Stylist_card(this.stylists.ElementAt(3)));
            //return Ok(cards);
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to get id, name, category(role), costperslot and rating
                SqlCommand command = new SqlCommand(@"SELECT ts.id as id,firstName,lastName,role,charge 
                                                      from trnStylist ts , trnJobRole tj, trnChargePerSlot tc 
                                                      where ts.jobRoleId = tj.id and ts.id = tc.stylistId and  tc.timeSlotId=1 ;"
                                                        , conn);
                this.stylists = new List<Stylist>();
                conn.Open();
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Stylist st = new Stylist(Convert.ToInt32(rdr["id"]),
                                Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]),
                                Convert.ToString(rdr["role"]),
                                Convert.ToInt32(rdr["charge"]),
                                5);
                        this.stylists.Add(st);
                    }
                }
            }
            return Ok(this.stylists);
        }

        [HttpGet]
        public IHttpActionResult getStylistNames()
        {
            return Ok(this.stylistNames);
        }

        //[HttpGet]
        //public IHttpActionResult getBasicSearchResults(string name)
        //{
        //    if (string.IsNullOrEmpty(name))
        //    {
        //        return Ok(stylists);
        //    }
        //    else
        //    {
        //        var results = stylists.FindAll(p => p.name == name);
        //        //Debug.WriteLine(string.IsNullOrEmpty(name));
        //        return Ok(results);
        //    }

        //}

        [HttpGet]
        public IHttpActionResult getBasicSearchResults(string name)
        {
            List<Stylist> stylistList = new List<Stylist>();
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to get id, name, category(role), costperslot and rating
                SqlCommand command = new SqlCommand(@"SELECT ts.id as id,firstName,lastName,role,charge 
                                                      from trnStylist ts , trnJobRole tj, trnChargePerSlot tc 
                                                      where ts.jobRoleId = tj.id and ts.id = tc.stylistId and  tc.timeSlotId=1 and (firstName='"+name+"' or lastName='"+name +"') ;"
                                                        ,conn);
                Debug.WriteLine(@"SELECT ts.id as id,firstName,lastName,role,charge 
                                                      from trnStylist ts , trnJobRole tj, trnChargePerSlot tc 
                                                      where ts.jobRoleId = tj.id and ts.id = tc.stylistId and  tc.timeSlotId=1 and (firstName='" + name + "' or lastName='" + name + "' )");
                
                conn.Open();
                using(SqlDataReader rdr = command.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Stylist st = new Stylist(Convert.ToInt32(rdr["id"]),
                                Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]),
                                Convert.ToString(rdr["role"]),
                                Convert.ToInt32(rdr["charge"]),
                                5);
                        stylistList.Add(st);
                    }
                }
            }
            return Ok(stylistList);


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

        [HttpGet]
        public IHttpActionResult getStylistDetails(int stylistId)
        {
            Stylist st = new Stylist();
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to get stylist details based on the id provided. The details required are name, address,city,state,country,telephone,description,skills
                SqlCommand command = new SqlCommand(@"select firstName,lastName,addressLine1,addressLine2,city,state,country,telephone,description,charge,role 
                                                      from trnStylist ts,trnChargePerSlot tc,trnJobRole tj  
                                                      where ts.id=" + stylistId+ " and ts.id=tc.stylistId and tc.timeSlotId=1 and tj.id=ts.jobRoleId;"
                                                        , conn);

                conn.Open();
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    rdr.Read();
                    st.name = Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]);
                    st.address = Convert.ToString(rdr["addressLine1"] + ", " + rdr["addressLine2"]);
                    st.city = Convert.ToString(rdr["city"]);
                    st.state = Convert.ToString(rdr["state"]);
                    st.country = Convert.ToString(rdr["country"]);
                    st.telephone = Convert.ToInt32(rdr["telephone"]);
                    st.description = Convert.ToString(rdr["description"]);
                    st.costPerSlot = Convert.ToInt32(rdr["charge"]);
                    st.role = Convert.ToString(rdr["role"]);
                }
            }
            return Ok(st);
        }




    }


}
