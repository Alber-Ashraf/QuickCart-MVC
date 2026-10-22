using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuickCart.Data;
using QuickCart.Models;
using QuickCart.Utility;

namespace QuickCart.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly QuickCartDbContext _db;
        private readonly IConfiguration _configuration;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            QuickCartDbContext db,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
            _configuration = configuration;
        }

        public void Initialize()
        {
            // 1. Apply any pending database migrations
            try
            {
                if (_db.Database.IsRelational() && _db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Database migration failed: {ex.Message}", ex);
            }

            // 2. Seed Identity Roles & Default Administrator
            SeedIdentity();

            // 3. Seed Literary Categories
            SeedCategories();

            // 4. Seed Real Authentic Books & Covers
            SeedBooks();
        }

        private void SeedIdentity()
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                var adminEmail = _configuration["AdminUser:Email"] ?? "admin@quickcart.com";
                var adminPassword = _configuration["AdminUser:Password"] ?? "AdminPassword123!";

                var existingAdmin = _userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Name = "QuickCart Administrator",
                        PhoneNumber = "555-0100",
                        Address = "100 Innovation Way",
                        City = "Seattle",
                        State = "WA",
                        PostalCode = "98101",
                        EmailConfirmed = true
                    };

                    var createResult = _userManager.CreateAsync(adminUser, adminPassword).GetAwaiter().GetResult();
                    if (createResult.Succeeded)
                    {
                        _userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
                    }
                }
            }
        }

        private void SeedCategories()
        {
            var desiredCategories = new[]
            {
                new Category { Name = "Classics", DisplayOrder = 1 },
                new Category { Name = "Sci-Fi & Fantasy", DisplayOrder = 2 },
                new Category { Name = "History & Thought", DisplayOrder = 3 },
                new Category { Name = "Technology", DisplayOrder = 4 },
                new Category { Name = "Self-Development", DisplayOrder = 5 }
            };

            foreach (var cat in desiredCategories)
            {
                if (!_db.Categories.Any(c => c.Name == cat.Name))
                {
                    _db.Categories.Add(cat);
                }
            }
            _db.SaveChanges();
        }

        private void SeedBooks()
        {
            // Remove old placeholder demo books if they are not attached to active orders or carts
            var placeholderTitles = new[]
            {
                "Fortune of Time",
                "Dark Skies",
                "Vanish in the Sunset",
                "Cotton Candy",
                "Rock in the Ocean",
                "Leaves and Wonders"
            };

            var unusedPlaceholders = _db.Products
                .Where(p => placeholderTitles.Contains(p.Title))
                .ToList();

            foreach (var placeholder in unusedPlaceholders)
            {
                bool inOrder = _db.OrderDetails.Any(od => od.ProductId == placeholder.Id);
                bool inCart = _db.ShoppingCarts.Any(sc => sc.ProductId == placeholder.Id);
                if (!inOrder && !inCart)
                {
                    var existingImgs = _db.ProductImages.Where(pi => pi.ProductId == placeholder.Id).ToList();
                    if (existingImgs.Any())
                    {
                        _db.ProductImages.RemoveRange(existingImgs);
                    }
                    _db.Products.Remove(placeholder);
                }
            }
            _db.SaveChanges();

            // Authentic, real literary books with real ISBNs, rich descriptions, and local book covers
            var realBooks = new[]
            {
                new BookSeedModel
                {
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    CategoryName = "Classics",
                    ISBN = "978-0743273565",
                    ListPrice = 18.00,
                    Price = 15.00,
                    Price50 = 13.50,
                    Price100 = 12.00,
                    ImageUrl = "/images/books/the-great-gatsby.jpg",
                    Description = "Set in the Jazz Age on Long Island near New York City, the novel depicts first-person narrator Nick Carraway's interactions with mysterious millionaire Jay Gatsby and Gatsby's obsession to reunite with his former lover, Daisy Buchanan. A quintessential exploration of the American Dream, wealth, excess, and disillusionment."
                },
                new BookSeedModel
                {
                    Title = "1984",
                    Author = "George Orwell",
                    CategoryName = "Sci-Fi & Fantasy",
                    ISBN = "978-0451524935",
                    ListPrice = 17.00,
                    Price = 14.50,
                    Price50 = 13.00,
                    Price100 = 11.50,
                    ImageUrl = "/images/books/1984.jpg",
                    Description = "Winston Smith is a low-ranking member of the ruling Party in London, in the nation of Oceania. Everywhere Winston goes, the Party watches through telescreens and the ominous gaze of Big Brother. Orwell's chilling prophecy about totalitarianism, mass surveillance, and the manipulation of truth remains one of the most powerful and influential novels of the twentieth century."
                },
                new BookSeedModel
                {
                    Title = "To Kill a Mockingbird",
                    Author = "Harper Lee",
                    CategoryName = "Classics",
                    ISBN = "978-0060935467",
                    ListPrice = 19.99,
                    Price = 16.99,
                    Price50 = 15.00,
                    Price100 = 13.50,
                    ImageUrl = "/images/books/to-kill-a-mockingbird.jpg",
                    Description = "Told through the observant eyes of young Scout Finch, the novel explores racial injustice, moral courage, and compassion in the fictional town of Maycomb, Alabama. When her father, attorney Atticus Finch, agrees to defend a Black man falsely accused of a horrific crime, Scout and her brother Jem learn profound lessons about empathy and conscience."
                },
                new BookSeedModel
                {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    CategoryName = "Sci-Fi & Fantasy",
                    ISBN = "978-0547928227",
                    ListPrice = 16.00,
                    Price = 14.00,
                    Price50 = 12.50,
                    Price100 = 11.00,
                    ImageUrl = "/images/books/the-hobbit.jpg",
                    Description = "Bilbo Baggins enjoys a comfortable, unambitious life in his hobbit-hole at Bag End until the wizard Gandalf and a company of thirteen dwarves whisk him away on a journey to the Lonely Mountain. There, they seek to reclaim the ancient Dwarf kingdom of Erebor and its treasure hoard from the deadly dragon Smaug."
                },
                new BookSeedModel
                {
                    Title = "Atomic Habits",
                    Author = "James Clear",
                    CategoryName = "Self-Development",
                    ISBN = "978-0735211292",
                    ListPrice = 27.00,
                    Price = 22.00,
                    Price50 = 19.50,
                    Price100 = 17.00,
                    ImageUrl = "/images/books/atomic-habits.jpg",
                    Description = "A comprehensive, practical guide on how to change your habits and get 1% better every day. Drawing from biology, psychology, and neuroscience, James Clear reveals an easy framework that proves tiny changes can lead to remarkable results. Learn how to design an environment where good habits are easy and bad habits are nearly impossible."
                },
                new BookSeedModel
                {
                    Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                    Author = "Robert C. Martin",
                    CategoryName = "Technology",
                    ISBN = "978-0132350884",
                    ListPrice = 54.99,
                    Price = 46.50,
                    Price50 = 42.00,
                    Price100 = 38.00,
                    ImageUrl = "/images/books/clean-code.jpg",
                    Description = "Even bad code can function, but if code isn't clean, it can bring a development organization to its knees. Software engineering legend Robert C. Martin presents a revolutionary paradigm with best practices of cleaning code on the fly. Packed with practical case studies, principles of craftsmanship, unit testing patterns, and refactoring techniques."
                },
                new BookSeedModel
                {
                    Title = "Dune",
                    Author = "Frank Herbert",
                    CategoryName = "Sci-Fi & Fantasy",
                    ISBN = "978-0441013593",
                    ListPrice = 20.00,
                    Price = 17.00,
                    Price50 = 15.00,
                    Price100 = 13.00,
                    ImageUrl = "/images/books/dune.jpg",
                    Description = "Set on the harsh desert planet Arrakis, Dune tells the story of Paul Atreides, heir to a noble family tasked with ruling an inhospitable world whose only valuable commodity is 'melange', the mystical spice that extends human consciousness and enables interstellar travel. A monumental triumph of worldbuilding, politics, religion, and ecology."
                },
                new BookSeedModel
                {
                    Title = "Pride and Prejudice",
                    Author = "Jane Austen",
                    CategoryName = "Classics",
                    ISBN = "978-0141439518",
                    ListPrice = 15.00,
                    Price = 12.50,
                    Price50 = 11.00,
                    Price100 = 9.50,
                    ImageUrl = "/images/books/pride-and-prejudice.jpg",
                    Description = "Jane Austen's timeless romantic comedy charts the emotional development of Elizabeth Bennet, who learns the error of making hasty judgements and comes to appreciate the difference between the superficial and the essential. Her turbulent relationship with the proud, wealthy aristocrat Fitzwilliam Darcy is a masterpiece of wit and social satire."
                },
                new BookSeedModel
                {
                    Title = "Sapiens: A Brief History of Humankind",
                    Author = "Yuval Noah Harari",
                    CategoryName = "History & Thought",
                    ISBN = "978-0062316097",
                    ListPrice = 24.99,
                    Price = 20.99,
                    Price50 = 18.50,
                    Price100 = 16.00,
                    ImageUrl = "/images/books/sapiens.jpg",
                    Description = "One hundred thousand years ago, at least six different species of humans inhabited Earth. Yet today there is only one: Homo sapiens. What happened to the others? And what may happen to us? Renowned historian Yuval Noah Harari spans the whole of human history, from the very first humans to walk the earth to the radical revolutions of modern biotechnology."
                },
                new BookSeedModel
                {
                    Title = "The Alchemist",
                    Author = "Paulo Coelho",
                    CategoryName = "Classics",
                    ISBN = "978-0062315007",
                    ListPrice = 16.99,
                    Price = 13.99,
                    Price50 = 12.50,
                    Price100 = 11.00,
                    ImageUrl = "/images/books/the-alchemist.jpg",
                    Description = "Paulo Coelho's enchanting novel has inspired a devoted following around the world. This magical story follows Santiago, an Andalusian shepherd boy who yearns to travel in search of a worldly treasure as extravagant as any ever found. The story of the treasures Santiago finds along the way teaches us the essential wisdom of listening to our hearts."
                },
                new BookSeedModel
                {
                    Title = "Crime and Punishment",
                    Author = "Fyodor Dostoevsky",
                    CategoryName = "Classics",
                    ISBN = "978-0140449136",
                    ListPrice = 18.00,
                    Price = 15.00,
                    Price50 = 13.50,
                    Price100 = 12.00,
                    ImageUrl = "/images/books/crime-and-punishment.jpg",
                    Description = "Raskolnikov, a destitute and desperate former student in Saint Petersburg, commits a murder believing himself an extraordinary man who transcends moral law. What follows is a profound psychological drama of guilt, terror, philosophical introspection, and the painful path toward redemption and spiritual rebirth."
                },
                new BookSeedModel
                {
                    Title = "The Catcher in the Rye",
                    Author = "J.D. Salinger",
                    CategoryName = "Classics",
                    ISBN = "978-0316769488",
                    ListPrice = 16.00,
                    Price = 13.50,
                    Price50 = 12.00,
                    Price100 = 10.50,
                    ImageUrl = "/images/books/the-catcher-in-the-rye.jpg",
                    Description = "First published in 1951, J.D. Salinger's novel follows sixteen-year-old Holden Caulfield after his expulsion from Pencey Prep school. Wandering through New York City, Holden grapples with alienating conformity, phoniness, grief over his late brother Allie, and the elusive desire to protect innocence from the corrupting adult world."
                }
            };

            var categoryDict = _db.Categories.ToDictionary(c => c.Name, c => c.Id);
            var defaultCategoryId = categoryDict.Values.FirstOrDefault();

            var existingProducts = _db.Products
                .Include(p => p.ProductImages)
                .ToList();

            foreach (var seed in realBooks)
            {
                var categoryId = categoryDict.TryGetValue(seed.CategoryName, out var catId)
                    ? catId
                    : defaultCategoryId;

                var existingProduct = existingProducts
                    .FirstOrDefault(p => p.ISBN == seed.ISBN || p.Title == seed.Title);

                if (existingProduct == null)
                {
                    var newProduct = new Product
                    {
                        Title = seed.Title,
                        Author = seed.Author,
                        Description = seed.Description,
                        ISBN = seed.ISBN,
                        ListPrice = seed.ListPrice,
                        Price = seed.Price,
                        Price50 = seed.Price50,
                        Price100 = seed.Price100,
                        CategoryId = categoryId
                    };
                    _db.Products.Add(newProduct);

                    _db.ProductImages.Add(new ProductImage
                    {
                        Product = newProduct,
                        ImageUrl = seed.ImageUrl
                    });
                }
                else
                {
                    existingProduct.Author = seed.Author;
                    existingProduct.Description = seed.Description;
                    existingProduct.ListPrice = seed.ListPrice;
                    existingProduct.Price = seed.Price;
                    existingProduct.Price50 = seed.Price50;
                    existingProduct.Price100 = seed.Price100;
                    existingProduct.CategoryId = categoryId;

                    if (!existingProduct.ProductImages.Any(pi => pi.ImageUrl == seed.ImageUrl))
                    {
                        _db.ProductImages.Add(new ProductImage
                        {
                            ProductId = existingProduct.Id,
                            ImageUrl = seed.ImageUrl
                        });
                    }
                }
            }
            _db.SaveChanges();
        }

        private class BookSeedModel
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string ISBN { get; set; } = string.Empty;
            public double ListPrice { get; set; }
            public double Price { get; set; }
            public double Price50 { get; set; }
            public double Price100 { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
