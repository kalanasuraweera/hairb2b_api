﻿using System;
using System.Configuration;
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
    //this controller handles stylist details
    public class StylistsController : ApiController
    {
        //List<Stylist> stylists;
        List<string> stylistNames = new List<string>();   
        public StylistsController()
        {           
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                Debug.WriteLine(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                //need to get firstname and lastname of all users and put them in the stylistNames list
                SqlCommand command = new SqlCommand("dbo.stylist_namelist"
                                                        , conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
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

        //get the busy dates for a particular stylist in a given month
        [HttpGet]
        public IHttpActionResult getBusyDates(int month,int stylistId)
        {            
            Stylist st = new Stylist();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                //need to add the busy dates to a stylist object
                SqlCommand command = new SqlCommand("dbo.stylist_busydates"
                                                        , conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@stylistId", stylistId));
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
            List<Stylist> stylists = new List<Stylist>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                //need to get id, name, category(role), costperslot and rating
                SqlCommand command = new SqlCommand("dbo.stylist_cards"
                                                        , conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                SqlCommand command2 = new SqlCommand("dbo.stylist_skills",conn);
                command2.CommandType = System.Data.CommandType.StoredProcedure;
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
                        st.profilePic = Convert.ToString(rdr["imagePath"]);
                        stylists.Add(st);
                    }
                }
                using (SqlDataReader rdr = command2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        
                        stylists.Find(p => p.id == Convert.ToInt32(rdr["id"])).skills.Add(Convert.ToString(rdr["description"]));
                    }
                }
            }
            return Ok(stylists);
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
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                //need to get id, name, category(role), costperslot and rating
                SqlCommand command = new SqlCommand("dbo.stylist_cards" ,conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@name",name));
                command.Parameters.Add(new SqlParameter("@requestType", "basic"));

                SqlCommand command2 = new SqlCommand("dbo.stylist_skills", conn);
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                command2.Parameters.Add(new SqlParameter("@name", name));

                conn.Open();
                using (SqlDataReader rdr = command.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Stylist st = new Stylist(Convert.ToInt32(rdr["id"]),
                                Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]),
                                Convert.ToString(rdr["role"]),
                                Convert.ToInt32(rdr["charge"]),
                                5);
                        st.profilePic = Convert.ToString(rdr["imagePath"]);
                        stylistList.Add(st);
                    }
                }
                using (SqlDataReader rdr = command2.ExecuteReader())
                {
                    while (rdr.Read())
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
            
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString) )
            {
                
                SqlCommand command = new SqlCommand("dbo.stylist_cards", conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@requestType", "advanced"));
                if(searchBy=="name")
                {
                    command.Parameters.Add(new SqlParameter("@advSearchName", advancedSearchName));
                }
                if(searchBy=="skills")
                {
                    command.Parameters.Add(new SqlParameter("@skillName", advancedSearchName));
                }
                if(fromDay !=0 && toDay!=0)
                {
                    DateTime from = new DateTime(fromYear, fromMonth, fromDay);
                    DateTime to = new DateTime(toYear, toMonth, toDay);
                    command.Parameters.Add(new SqlParameter("@fromDate", from));
                    command.Parameters.Add(new SqlParameter("@toDate", to));
                }
                if(serviceCharge !=0)
                {
                    command.Parameters.Add(new SqlParameter("@serviceCharge", serviceCharge));
                }
                SqlCommand command2 = new SqlCommand("dbo.stylist_skills", conn);
                command2.CommandType = System.Data.CommandType.StoredProcedure;
                conn.Open();
                using(SqlDataReader rdr= command.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        Stylist st = new Stylist(Convert.ToInt32(rdr["id"]), Convert.ToString(rdr["firstName"]) + " " + Convert.ToString(rdr["lastName"]),Convert.ToString(rdr["role"]),Convert.ToInt32(rdr["charge"]),5);
                        st.profilePic = Convert.ToString(rdr["imagePath"]);
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
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                //need to get stylist details based on the id provided. The details required are name, address,city,state,country,telephone,description,skills
                SqlCommand command = new SqlCommand("dbo.stylist_details"
                                                        , conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@stylistId", stylistId));
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
                    st.profilePic = Convert.ToString(rdr["imagePath"]);
                }
            }
            return Ok(st);
        }

        [HttpGet]
        public IHttpActionResult getSkillNames()
        {
            List<String> skills = new List<string>();
            using(SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                SqlCommand command = new SqlCommand("dbo.skill_description",conn);
                command.CommandType = System.Data.CommandType.StoredProcedure;
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
