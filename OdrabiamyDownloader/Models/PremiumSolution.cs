using System;
using System.Collections.Generic;
using System.Text;

namespace OdrabiamyDownloader.Models
{
    public class PremiumSolution
    {
        public int Id { get; set; }
        public int Page { get; set; }
        public string Number { get; set; }
        public string Content { get; set; }
        public string Solution { get; set; }

        public Dictionary<string, object> Book { get; set; }
    }
}
