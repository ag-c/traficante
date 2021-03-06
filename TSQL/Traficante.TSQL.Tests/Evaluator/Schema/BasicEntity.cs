﻿using System;

namespace Traficante.TSQL.Evaluator.Tests.Core.Schema
{
    public class BasicEntity
    {
        public BasicEntity()
        {
        }

        public BasicEntity(string name)
        {
            Name = name;
        }

        public BasicEntity(string country, string city)
        {
            Country = country;
            City = city;
        }

        public BasicEntity(string country, int population)
        {
            Country = country;
            Population = population;
        }

        public BasicEntity(string city, string country, int population)
        {
            City = city;
            Country = country;
            Population = population;
        }

        public BasicEntity(string month, decimal money)
        {
            Month = month;
            Money = money;
        }

        public BasicEntity(string city, string month, decimal money)
        {
            City = city;
            Month = month;
            Money = money;
        }

        public BasicEntity(DateTime time)
        {
            Time = time;
        }

        public string Name { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public decimal Population { get; set; }
        public BasicEntity Self => this;
        public decimal Money { get; set; }
        public string Month { get; set; }
        public DateTime Time { get; set; }
        public int Id { get; set; }
        public int? NullableValue { get; set; }
        public int[] Array => new[] {0, 1, 2};
        
        public override string ToString()
        {
            return "TEST STRING";
        }
    }
}