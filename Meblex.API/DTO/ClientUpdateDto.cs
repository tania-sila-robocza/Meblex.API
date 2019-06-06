﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meblex.API.DTO
{
    public class ClientUpdateDto
    {
        public string Name { get; set; }

        public string NIP { get; set; }

        public string Address { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string PostCode { get; set; }
    }
}
