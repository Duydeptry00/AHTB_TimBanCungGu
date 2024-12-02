using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class NoiDungBaoCao
    {
        [BsonId]
        public ObjectId Id { get; set; } // ID tự động sinh ra bởi MongoDB

        [BsonElement("nguoi_bao_cao")]
        public string NguoiBaoCao { get; set; }

        [BsonElement("doi_tuong_bao_cao")]
        public string DoiTuongBaoCao { get; set; }

        [BsonElement("ngay_bao_cao")]
        public DateTime NgayBaoCao { get; set; }

        [BsonElement("ly_do_bao_cao")]
        public string LyDoBaoCao { get; set; }

        [BsonElement("tin_nhan_lien_quan")]
        public List<string> TinNhanLienQuan { get; set; }

        public NoiDungBaoCao()
        {
            TinNhanLienQuan = new List<string>();
        }
    }
}
