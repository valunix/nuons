using System;
using System.Collections.Generic;

namespace Nuons.CodeInjection.Generators.Tests;

// Enum for testing enum property mapping
public enum SampleOrderStatus
{
    Pending,
    Shipped,
    Delivered
}

// Standard domain type — all properties are supported types
public class SampleOrder
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public SampleOrderStatus Status { get; set; }
    public Guid InternalTrackingId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
}

// Domain type with nullable properties
public class SampleProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int? DiscountPercent { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? LastModified { get; set; }
}

// Domain type with mixed supported/unsupported properties
public class SampleComplexEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();  // unsupported — List<T>
    public SampleOrder? RelatedOrder { get; set; }                // unsupported — nested object
    public SampleOrderStatus Status { get; set; }                 // supported — enum
    public DateTime CreatedAt { get; set; }                       // supported
}

// Domain type with read-only properties
public class SampleReadOnlyEntity
{
    public int Id { get; }
    public string Name { get; } = string.Empty;
}
