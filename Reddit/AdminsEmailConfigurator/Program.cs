using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdminsEmailConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string correctUsername = "cloudreddit";
            const string correctPassword = "cloudredditforum";

            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            Console.Write("Enter password: ");
            string password = ReadPassword();

            if (username == correctUsername && password == correctPassword)
            {
                Console.WriteLine("Login successful.");

                // Load existing email addresses from file
                var existingEmails = LoadExistingEmails("AdminEmails.txt");

                Console.WriteLine("Enter admin email addresses (type 'done' to finish):");

                List<string> newEmailAddresses = new List<string>();
                while (true)
                {
                    string email = Console.ReadLine();
                    if (email.ToLower() == "done")
                    {
                        break;
                    }

                    if (existingEmails.Contains(email))
                    {
                        Console.WriteLine($"The email address '{email}' already exists.");
                    }
                    else
                    {
                        newEmailAddresses.Add(email);
                        existingEmails.Add(email); // Add to the list to check further entries
                    }
                }

                if (newEmailAddresses.Count > 0)
                {
                    File.AppendAllLines("AdminEmails.txt", newEmailAddresses);
                    Console.WriteLine("Email addresses saved to AdminEmails.txt");
                }
                else
                {
                    Console.WriteLine("No new email addresses to save.");
                }
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
        }

        // Method to read password input without displaying it on the console
        private static string ReadPassword()
        {
            StringBuilder password = new StringBuilder();
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    password.Append(info.KeyChar);
                    Console.Write("*");
                }
                info = Console.ReadKey(true);
            }
            Console.WriteLine();
            return password.ToString();
        }

        // Method to load existing email addresses from file
        private static HashSet<string> LoadExistingEmails(string filePath)
        {
            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    emails.Add(line);
                }
            }
            return emails;
        }
    }
}
