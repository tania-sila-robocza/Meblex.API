﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meblex.API.FormsDto.Response
{
    public class ShoppingCartPartResponse
    {
        public CategoryResponse Category { get; set; }
        public int PieceOfFurnitureId { get; set; }
        public int PartId { get; set; }
        public string Name { get; set; }
    }
}
