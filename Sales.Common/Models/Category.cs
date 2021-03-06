﻿namespace Sales.Common.Models
{
    using Newtonsoft.Json;
    using SQLite;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        [Display(Name = "Image")]
        public string ImagePath { get; set; }
        [JsonIgnore]
        [Ignore]
        public virtual ICollection<Product> Products { get; set; }

        public string ImageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(this.ImagePath))
                {
                    return "noproduct";
                }

                return $"http://10.0.0.22{this.ImagePath.Substring(1)}";
            }
        }
    }
}