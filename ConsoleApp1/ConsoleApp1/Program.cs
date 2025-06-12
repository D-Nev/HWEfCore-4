using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
    }
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=Shopdb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {    
            modelBuilder.Entity<Store>()
                .HasMany(s => s.Customers)
                .WithMany(c => c.Stores)
                .UsingEntity(j => j.ToTable("StoreCustomer"));
        }
    }

    public class Program
    {
        public static void Main()
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            using (var context = new AppDbContext())
            {
                var companies = new List<Company>
            {
                new Company { Name = "Supermarkets 'Food'" },
                new Company { Name = "Electronics 'Gadget'" },
                new Company { Name = "Mega Corporation" } 
            };

                var stores = new List<Store>
            {
                new Store { Name = "Food on Main St", Company = companies[0] },
                new Store { Name = "Food on Oak St", Company = companies[0] },
                new Store { Name = "Gadget Center", Company = companies[1] },
                
                new Store { Name = "Mega Store 1", Company = companies[2] },
                new Store { Name = "Mega Store 2", Company = companies[2] },
                new Store { Name = "Mega Store 3", Company = companies[2] },
                new Store { Name = "Mega Store 4", Company = companies[2] },
                new Store { Name = "Mega Store 5", Company = companies[2] },
                new Store { Name = "Mega Store 6", Company = companies[2] }
            };

                var customers = new List<Customer>
            {
                new Customer { FullName = "John Smith" },
                new Customer { FullName = "Emily Johnson" },
                new Customer { FullName = "Michael Brown" }
            };

                stores[0].Customers.AddRange(new[] { customers[0], customers[1] });
                stores[1].Customers.Add(customers[0]);
                stores[2].Customers.AddRange(new[] { customers[1], customers[2] });

                context.AddRange(companies);
                context.AddRange(stores);
                context.SaveChanges();
            }

            using (var context = new AppDbContext())
            {
                Console.WriteLine("Company information:");
                var companies = context.Companies
                    .Include(c => c.Stores)
                    .ThenInclude(s => s.Customers)
                    .ToList();

                foreach (var company in companies)
                {
                    Console.WriteLine($"\nCompany: {company.Name}");
                    Console.WriteLine("Stores:");
                    foreach (var store in company.Stores)
                    {
                        Console.WriteLine($"- {store.Name}");
                        Console.WriteLine("  Customers: " +
                            string.Join(", ", store.Customers.Select(c => c.FullName)));
                    }
                }

                Console.WriteLine("\nCustomer information:");
                var customers = context.Customers
                    .Include(c => c.Stores)
                    .ThenInclude(s => s.Company)
                    .ToList();

                foreach (var customer in customers)
                {
                    Console.WriteLine($"\nCustomer: {customer.FullName}");
                    Console.WriteLine("Stores:");
                    foreach (var store in customer.Stores)
                    {
                        Console.WriteLine($"- {store.Name} ({store.Company.Name})");
                    }
                }

                Console.WriteLine("\nCompanies with more than 5 stores:");
                var bigCompanies = context.Companies
                    .Include(c => c.Stores)
                    .Where(c => c.Stores.Count > 5)
                    .Select(c => new {
                        c.Name,
                        StoreCount = c.Stores.Count
                    })
                    .ToList();

                if (bigCompanies.Any())
                {
                    foreach (var comp in bigCompanies)
                    {
                        Console.WriteLine($"{comp.Name}: {comp.StoreCount} stores");
                    }
                } 

                Console.WriteLine("\nCustomers registered in multiple stores:");
                var activeCustomers = context.Customers
                    .Include(c => c.Stores)
                    .Where(c => c.Stores.Count > 1)
                    .Select(c => new {
                        c.FullName,
                        StoreCount = c.Stores.Count
                    })
                    .ToList();

                foreach (var cust in activeCustomers)
                {
                    Console.WriteLine($"{cust.FullName}: {cust.StoreCount} stores");
                }
            }
        }
    }
}
