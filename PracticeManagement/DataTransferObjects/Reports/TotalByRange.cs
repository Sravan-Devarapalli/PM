using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTransferObjects.Reports
{
    public class TotalByRange
    {
        public DateTime StartDate { get; set; }

        public int Total { get; set; }

        public int TotalResourcesWithMS { get; set; }

        public int TotalAvailableResourcesWithOutMS { get; set; }

        public decimal TotalRequiredResources { get; set; }
    }
}
