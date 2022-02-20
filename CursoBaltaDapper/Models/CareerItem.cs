using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursoBaltaDapper.Models
{
    class CareerItem
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public Course Course { get; set; }
    }
}
