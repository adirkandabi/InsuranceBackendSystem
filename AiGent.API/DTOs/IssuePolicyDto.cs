using System;
using System.ComponentModel.DataAnnotations;
using AiGent.Core.Enums;

namespace AiGent.API.DTOs;

public class IssuePolicyDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    [EnumDataType(typeof(PolicyType), ErrorMessage = "Invalid Policy Type. Allowed values are: 1 (Car), 2 (Health), 3 (Life)")]
    public PolicyType Type { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Premium must be greater than zero.")]
    public decimal Premium { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Coverage Amount must be greater than zero.")]
    public decimal CoverageAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}