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
            
            

        }

    
        [HttpGet]
        public IHttpActionResult getBusyDates(int month,int stylistId)
        {
            
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
            
            using (SqlConnection conn = new SqlConnection(this.connString))
            {
                //need to get id, name, category(role), costperslot and rating
                SqlCommand command = new SqlCommand(@"SELECT ts.id as id,firstName,lastName,role,charge 
                                                      from trnStylist ts , trnJobRole tj, trnChargePerSlot tc 
                                                      where ts.jobRoleId = tj.id and ts.id = tc.stylistId and  tc.timeSlotId=1 ;"
                                                        , conn);
                SqlCommand command2 = new SqlCommand(@"SELECT ts.id as id,tk.description 
                                                       from trnStylist ts,trnStylistSkillMapping tsm, trnSkill tk 
                                                       WHERE ts.id=tsm.stylistId and tsm.skillId=tk.id;",conn);
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
                using (SqlDataReader rdr = command2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        //int i;
                        this.stylists.Find(p => p.id == Convert.ToInt32(rdr["id"])).skills.Add(Convert.ToString(rdr["description"]));
                        


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
                SqlCommand command2 = new SqlCommand(@"SELECT ts.id as id,tk.description 
                                                       from trnStylist ts,trnStylistSkillMapping tsm, trnSkill tk 
                                                       WHERE ts.id=tsm.stylistId and tsm.skillId=tk.id and (firstName='" + name + "' or lastName='" + name + "') ;",conn);

               
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
                using(SqlDataReader rdr = command2.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        
                        stylistList.Find(p => p.id == Convert.ToInt32(rdr["id"])).skills.Add(Convert.ToString(rdr["description"]));                 
                        

                    }
                }
            }
            return Ok(stylistList);


        }

        [HttpGet]
        public IHttpActionResult getAdvancedSearchResults(string searchBy, string advancedSearchName, int fromDay,int fromMonth,int fromYear, int toDay, int toMonth, int toYear,  int serviceCharge)
        {
            
            
            List<Stylist> stylistList = new List<Stylist>();
            List<int> stylistIds = new List<int>();
            string searchString = @"Select distinct ts.id as id,firstName,lastName,charge,role 
                                    FROM trnStylist ts ,trnChargePerSlot tc, trnJobRole tj  
                                    WHERE tc.stylistId=ts.id and tc.timeSlotId=1 and tj.id=ts.jobRoleId";
            
            if (searchBy=="name")
            {
                searchString += " and (firstName='" + advancedSearchName + "' or lastName='" + advancedSearchName + "' )";
            }
            if(searchBy=="skills")
            {
                searchString += @" and ts.id IN(SELECT DISTINCT ts.id 
                                                FROM trnStylist ts,trnStylistSkillMapping tsm, trnSkill tk
												WHERE ts.id=tsm.stylistId and tsm.skillId=tk.id and tk.description='" + advancedSearchName + "')";
            }
            if(fromDay !=0 && toDay!=0)
            {
                DateTime from = new DateTime(fromYear, fromMonth, fromDay);
                DateTime to = new DateTime(toYear, toMonth, toDay);
                searchString += " and ts.id NOT IN (SELECT DISTINCT ts.id FROM trnStylist ts,trnBusyDates tb WHERE ts.id=tb.stylistId and tb.date>'"+from.Date+"' and tb.date <'"+to.Date+"' )";
            }
            
            if(serviceCharge !=0)
            {
                searchString += " and charge<=" + serviceCharge;
            }
            searchString += ";";
            Debug.WriteLine(searchString);
            using (SqlConnection conn = new SqlConnection(this.connString) )
            {
                SqlCommand command = new SqlCommand(searchString, conn);
                SqlCommand command2 = new SqlCommand(@"SELECT ts.id as id,tk.description as description
                                                       from trnStylist ts,trnStylistSkillMapping tsm, trnSkill tk 
                                                       WHERE ts.id=tsm.stylistId and tsm.skillId=tk.id;", conn);
                conn.Open();
                using(SqlDataReader rdr= command.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Stylist st = new Stylist(Convert.ToInt32(rdr["id"]), Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]),Convert.ToString(rdr["role"]),Convert.ToInt32(rdr["charge"]),5);
                        stylistList.Add(st);
                        stylistIds.Add(Convert.ToInt32(rdr["id"]));
                    }
                }
                using(SqlDataReader rdr=command2.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        int id = Convert.ToInt32(rdr["id"]);
                        if (stylistIds.Contains(id)) {
                            stylistList.Find(p => p.id == id).skills.Add(Convert.ToString(rdr["description"]));
                        }
                    }
                }
            }
            
            return Ok(stylistList);

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

        [HttpGet]
        public IHttpActionResult getSkillNames()
        {
            List<String> skills = new List<string>();
            using(SqlConnection conn = new SqlConnection(this.connString))
            {
                SqlCommand command = new SqlCommand("select description from trnSkill;",conn);
                conn.Open();
                using(SqlDataReader rdr=command.ExecuteReader())
                {
                    while(rdr.Read())
                        skills.Add(Convert.ToString(rdr["description"]));
                }
            }
            return Ok(skills);
        }




    }


}
