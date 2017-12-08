namespace MultiDialogsBot
{
    using System;
    using Microsoft.Bot.Builder.FormFlow;

    [Serializable]
    public class ProvidersQuery
    {
        [Prompt("Please enter your {&}")]
        public string City { get; set; }
        
        [Prompt("Please enter a {&}")]
        public string Speciality { get; set; }

        //[Numeric(1, int.MaxValue)]
        //[Prompt("How many {&} do you want to stay?")]
        //public int Nights { get; set; }
    }
}