using AHTB_TimBanCungGu_API.Chats;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("AHTBdb");
    }

    // Collection lưu tin nhắn
    public IMongoCollection<Message> Messages => _database.GetCollection<Message>("NhanTin");

    // Collection lưu danh sách trò chuyện
    public IMongoCollection<Conversation> Conversations => _database.GetCollection<Conversation>("DanhSachTroChuyen");
    // Collection lưu thông tin các lần swipe
    public IMongoCollection<MatchNguoiDung> MatchNguoiDung => _database.GetCollection<MatchNguoiDung>("MatchNguoiDung");
    // Collection lưu danh sách chặn
    public IMongoCollection<BlockUser> BlockUser => _database.GetCollection<BlockUser>("DanhSachChan");
}
