using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MultiDialogsBot
{
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Speciality { get; set; }
        public bool AcceptingPatients { get; set; }
        public int Rating { get; set; }
        public string Location { get; set; }
        public string Image { get; set; }
    }
}