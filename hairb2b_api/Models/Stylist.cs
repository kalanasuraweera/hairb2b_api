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
        public string category;
        public decimal costPerSlot;
        public int rating;
        public Stylist_card card;
        public List<TimeSlot> BusySlots = new List<TimeSlot>();

        public Stylist(int id, string name, string category, decimal cost,int rating)
        {
            this.id = id;
            this.name = name;
            this.category = category;
            this.costPerSlot = cost;
            this.rating = rating;
            this.card = new Stylist_card(id, name, category, cost, rating);
        }

        public void addBusySlots(int year,int month, int day,char slot) 
        {
            this.BusySlots.Add(new TimeSlot(year, month, day, slot));
        }

        public List<TimeSlot> getBusySlots(int month)
        {
            return this.BusySlots.FindAll(p => p.month == month);
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
            public Stylist_card(int id,string name,string category,decimal cost,int rating)
            {
                this.id = id;
                this.name = name;
                this.category = category;
                this.costPerSlot = cost;
                this.rating = rating;
            }

            public Stylist_card(Stylist stylist)
            {
                this.id = stylist.id;
                this.name = stylist.name;
                this.category = stylist.category;
                this.costPerSlot = stylist.costPerSlot;
                this.rating = stylist.rating;
            }
        }
    }

   

    
    
}