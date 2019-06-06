﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Meblex.API.Models
{
    public class Color
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ColorId { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [Required]
        [StringLength(7)]
        public string HexCode { get; set; }

        [Required]
        [StringLength(32)]
        public string Slug { get; set; }

        public virtual List<Part> Parts { get; set; }

        public virtual List<PieceOfFurniture> PieceOfFurniture { get; set; }

    }
}
