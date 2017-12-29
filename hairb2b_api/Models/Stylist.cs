using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace hairb2b_api.Models
{
    public class Stylist
    {
        public int id;
        public string name;
        public string role;
        public decimal costPerSlot;
        public int rating;
        public string address;
        public string city;
        public string state;
        public string country;
        
        public int telephone;
        public string description;
        public Stylist_card card;
        public List<string> skills;
        public List<TimeSlot> BusySlots = new List<TimeSlot>();
       

        public Stylist(int id, string name, string role, decimal cost,int rating)
        {
            this.id = id;
            this.name = name;
            this.role = role;
            this.costPerSlot = cost;
            this.rating = rating;
            //this.card = new Stylist_card(id, name, category, cost, rating);
        }

        public Stylist()
        {

        }

        public void addBusySlots(int year,int month, int day,char slot) 
        {
            this.BusySlots.Add(new TimeSlot(year, month, day, slot));
        }

        public List<TimeSlot> getBusySlots(int month)
        {
            return this.BusySlots.FindAll(p => p.month == month);
        }

        public List<TimeSlot> getBusySlots()
        {
            return this.BusySlots;
        }

        public Stylist_card getCard()
        {
            return this.card;
        }


        //internal classes required by the Stylist class
        public class TimeSlot
        {
            public int day;
            public int month;
            public int year;
            public char slot;

            public TimeSlot(int year, int month, int day, char slot)
            {
                this.day = day;
                this.month = month;
                this.year = year;
                this.slot = slot;
            }

        }

        public class Stylist_card
        {
            public int id;
            public string name;
            public string category;
            public decimal costPerSlot;
            public int rating;
            public List<string> skills;
            public Stylist_card(int id,string name,string category,decimal cost,int rating)
            {
                this.id = id;
                this.name = name;
                this.category = category;
                this.costPerSlot = cost;
                this.rating = rating;
                this.skills = new List<string>();
            }

            public Stylist_card(Stylist stylist)
            {
                this.id = stylist.id;
                this.name = stylist.name;
                this.category = stylist.role;
                this.costPerSlot = stylist.costPerSlot;
                this.rating = stylist.rating;
            }

            public void add_skill(string skill)
            {
                this.skills.Add(skill);
            }
        }
    }

   

    
    
}