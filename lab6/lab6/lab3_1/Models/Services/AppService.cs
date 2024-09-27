using lab3_1.Models.Database;
using lab3_1.Models.Services.AzureServices;
using lab3_1.Models.Services.DatabaseServices;
using lab3_1.Models.Services.LoadServices;
using lab3_1.Models.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace lab3_1.Models.Services
{
    public class AppService
    {
        private AuthorizationService AuthorizationService { get; set; }
        private DatabaseService DatabaseService { get; set; }
        private LoadService LoadService { get; set; }
        private int UserId { get; set; }

        public AppService(DatabaseService db, int userId)
        {
            AuthorizationService = new AuthorizationService();
            DatabaseService = db;
            LoadService = new LoadService(userId, DatabaseService);
            UserId = userId;
        }

        public AppService(DatabaseService db)
        {
            AuthorizationService = new AuthorizationService();
            DatabaseService = db;
        }

        public void SetUserId(int userId)
        {
            LoadService = new LoadService(userId, DatabaseService);
            UserId = userId;
        }

        internal bool IsLoginAvailable(string login)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login))
                {
                    throw new ArgumentException("Login cannot be empty or null");
                }

                return !DatabaseService.ExistsUser(login);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsLoginAvailable: {ex.Message}");
                return false;
            }
        }

        internal static bool IsAuthorizathed(string token, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("Token and key cannot be empty or null");
                }

                return AuthorizationService.CheckAuthorization(token, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in IsAuthorizathed: {ex.Message}");
                return false;
            }
        }

        internal static int GetUserId(string token, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException("Token and key cannot be empty or null");
                }

                return AuthorizationService.GetIdOfCurrentUser(token, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserId: {ex.Message}");
                return -1;
            }
        }

        internal async Task<AuthorizationModel?> AuthorizeUser(string login, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Login and password cannot be empty or null");
                }

                AccountModel? accountModel = await DatabaseService.CheckAuthorization(login, password);
                if (accountModel == null)
                {
                    return null;
                }
                return AuthorizationService.GenerateToken(accountModel.Id, accountModel.Role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AuthorizeUser: {ex.Message}");
                return null;
            }
        }

        internal async Task<bool> RegisterUser(string login, string password, string firstname, string lastname)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Login and password cannot be empty or null");
                }

                return await DatabaseService.AddUser(login, password, firstname, lastname);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RegisterUser: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> UploadFile(FileStream file, string path)
        {
            try
            {
                if (file == null || string.IsNullOrEmpty(path))
                {
                    throw new ArgumentException("File or path cannot be null");
                }

                await LoadService.LoadToLocalPath(file, path);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadFile: {ex.Message}");
                return false;
            }
        }
    }
}
