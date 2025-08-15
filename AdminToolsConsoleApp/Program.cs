using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;

namespace AdminToolsConsoleApp
{
    public class Program
    {
        private static IHealthMonitoring proxy;
        public static void Connect()
        {
            var binding = new NetTcpBinding();
            ChannelFactory<IHealthMonitoring> factory = new
                ChannelFactory<IHealthMonitoring>(binding, new
                EndpointAddress("net.tcp://localhost:10100/HMonitor"));
            proxy = factory.CreateChannel();
        }

        private static void DisplayAlertEmails()
        {
            List<AlertEmailEntity> emails = proxy.RetrieveAllAlertEmails();
            int num = 0;
            foreach(AlertEmailEntity email in emails)
            {
                num++;
                Console.WriteLine($"{num}: {email.Email}");
            }
        }

        private static void AddAlertEmail()
        {
            // srediti poslije da provjerava je li validan format unosa
            Console.WriteLine("Enter email: ");
            AlertEmailEntity newEmail = new AlertEmailEntity(Console.ReadLine());
            proxy.AddAlertEmail(newEmail);
        }
        private static void UpdateAlertEmail()
        {
            // i ovdje poslije provjera unosa
            Console.WriteLine("Enter email to update: ");
            string emailToUpdate = Console.ReadLine();
            
            Console.WriteLine("Enter update email: ");
            AlertEmailEntity updateEmail = new AlertEmailEntity(Console.ReadLine());

            proxy.UpdateAlertEmail(emailToUpdate, updateEmail);
        }

        private static void RemoveAlertEmail()
        {
            Console.WriteLine("Enter email to remove: ");
            string emailToRemove = Console.ReadLine();

            proxy.RemoveAlertEmail(emailToRemove);
        }

        static void Main(string[] args)
        {
            AlertEmailDataRepo alertEmailRepo1 = new AlertEmailDataRepo();
            Connect();
            bool exit = true;
            
            while (exit)
            {
                Console.WriteLine("---------- Options:");
                Console.WriteLine("1. Display all alert emails");
                Console.WriteLine("2. Add email");
                Console.WriteLine("3. Update email");
                Console.WriteLine("4. Remove email");
                Console.WriteLine("5. Exit");
                
                Console.Write("Choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplayAlertEmails();
                        break;

                    case "2":
                        AddAlertEmail();
                        break;

                    case "3":
                        UpdateAlertEmail();
                        break;

                    case "4":
                        RemoveAlertEmail();
                        break;

                    case "5":
                        exit = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                Console.WriteLine();
            }
        }
    }
}
