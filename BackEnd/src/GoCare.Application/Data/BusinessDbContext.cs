using Microsoft.EntityFrameworkCore;

namespace GoCare.Application.Data;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options) : DbContext(options)
{
        
}

