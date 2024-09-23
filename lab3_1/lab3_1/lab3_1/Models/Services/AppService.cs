using lab3_1.Models.Database;
using lab3_1.Models.Services.AzureServices;
using lab3_1.Models.Services.DatabaseServices;
using lab3_1.Models.Services.LoadServices;
using lab3_1.Models.ViewModels;
using System.Net;

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
            return !DatabaseService.ExistsUser(login);
        }

        internal static bool IsAuthorizathed(string token, string key)
        {
            return AuthorizationService.CheckAuthorization(token, key);
        }

        internal static int GetUserId(string token, string key)
        {
            return AuthorizationService.GetIdOfCurrentUser(token, key);
        }

        internal async Task<AuthorizationModel?> AuthorizeUser(string login, string password)
        {
            try
            {
                AccountModel? accountModel = await DatabaseService.CheckAuthorization(login, password);
                if (accountModel == null)
                {
                    return null;
                }
                return AuthorizationService.GenerateToken(accountModel.Id, accountModel.Role);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal async Task<bool> RegisterUser(string login, string password, string firstname, string lastname)
        {
            return await DatabaseService.AddUser(login, password, firstname, lastname);
        }

        internal void Bsp()
        {
            LoadService.LoadToLocalPath(new FileStream("D:\\Фотоаппарат\\DSC_0728.jpg", FileMode.Open, FileAccess.Read), "D:\\Фотоаппарат\\DSC_0728.jpg");
        }

        internal void Bsp2()
        {
            LoadService.SendAllToStorage();
        }
    }
}