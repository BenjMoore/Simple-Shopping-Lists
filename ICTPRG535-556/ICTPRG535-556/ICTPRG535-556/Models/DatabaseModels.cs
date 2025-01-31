﻿using System.ComponentModel.DataAnnotations;

namespace ICTPRG535_556.Models
{
    public class UserDTO
    {
        public int UserID { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }
        public int Lists { get; set; }

    }

    public class ListDTO
    {
        public int ListID { get; set; }
        public string ListName { get; set; }
        public int UserID { get; set; }
        public string? Unit { get; set; } 
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public DateTime? FinalisedDate { get; set; }
        public int ListIndex { get; set; }

    }
    public class UserListsDTO
    {
        public int ListID { get; set; }
        public int UserID { get; set; }
    }
    public class ProduceDTO
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int? Quantity { get; set; }

    }

}
