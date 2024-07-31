using Microsoft.EntityFrameworkCore;
using StudentsService.Models;

namespace StudentsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(), isProd);
            }
        }

        private static void SeedData(AppDbContext context, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("--> Not in Production mode so no migrations will be run...");
            }

            if (!context.Students.Any())
            {
                Console.WriteLine("--> Seeding Student data...");

                context.Students.AddRange(
                    new Student()
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        DateOfBirth = new DateTime(2019, 5, 15),
                        Age = 5
                    },
                    new Student()
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        DateOfBirth = new DateTime(2020, 9, 20),
                        Age = 4
                    },
                    new Student()
                    {
                        FirstName = "Alice",
                        LastName = "Johnson",
                        DateOfBirth = new DateTime(2020, 1, 10),
                        Age = 4
                    }
                );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}