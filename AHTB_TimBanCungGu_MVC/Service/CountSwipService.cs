using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_MVC.Models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Service
{
    public class CountSwipService
    {
        private readonly DBAHTBContext _context;
        private readonly IMongoCollection<UserSwipeInfo> _userSwipes;

        // Constructor to initialize MongoDB collection
        public CountSwipService()
        {
            
            var connectionString = "mongodb://localhost:27017";  // MongoDB connection string
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("AHTBdb");  // Database name
            _userSwipes = database.GetCollection<UserSwipeInfo>("SoLuotVuot");  // Collection name
        }
        // Method to handle user login and swipe count logic
        public async Task HandleUserLoginAsync(string userId)
        {
            // Tìm kiếm thông tin vuốt của người dùng
            var userSwipeInfo = await _userSwipes.Find(u => u.Uservuot == userId).FirstOrDefaultAsync();
            
            if (userSwipeInfo == null)
            {

                
                // Thêm mới nếu không tồn tại
                userSwipeInfo = new UserSwipeInfo
                {
                    Uservuot = userId,
                    SwipesRemaining = 10, // Lượt vuốt mặc định
                    LastSwipeResetDate = DateTime.UtcNow.Date
                };

                await _userSwipes.InsertOneAsync(userSwipeInfo);
            }
            else if (userSwipeInfo.LastSwipeResetDate < DateTime.UtcNow.Date)
            {
                // Reset nếu ngày hiện tại khác với ngày reset lần cuối
                userSwipeInfo.SwipesRemaining = 10;
                userSwipeInfo.LastSwipeResetDate = DateTime.UtcNow.Date;

                await _userSwipes.ReplaceOneAsync(u => u.Uservuot == userId, userSwipeInfo);
            }
        }
        public async Task LuotVuotPrimeumAsync(string userId)
        {
            // Tìm kiếm thông tin vuốt của người dùng
            var userSwipeInfo = await _userSwipes.Find(u => u.Uservuot == userId).FirstOrDefaultAsync();

            if (userSwipeInfo == null)
            {


                // Thêm mới nếu không tồn tại
                userSwipeInfo = new UserSwipeInfo
                {
                    Uservuot = userId,
                    SwipesRemaining = 999, // Lượt vuốt mặc định
                    LastSwipeResetDate = DateTime.UtcNow.Date
                };

                await _userSwipes.InsertOneAsync(userSwipeInfo);
            }
        }
        public async Task GiamLuotVuot(string username)
        {
            var userSwipeInfo = await _userSwipes.Find(u => u.Uservuot == username).FirstOrDefaultAsync();
            userSwipeInfo.SwipesRemaining = userSwipeInfo.SwipesRemaining-1;
            await _userSwipes.ReplaceOneAsync(u => u.Uservuot == username, userSwipeInfo);
        }
        public class UserSwipeInfo
        {
            public MongoDB.Bson.ObjectId Id { get; set; }
            public string Uservuot { get; set; }
            public int SwipesRemaining { get; set; }
            public DateTime LastSwipeResetDate { get; set; }
        }

    }
}
