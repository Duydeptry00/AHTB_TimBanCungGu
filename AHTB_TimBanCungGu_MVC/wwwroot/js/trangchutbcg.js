// Dữ liệu về các thẻ người dùng
const cards = [
    {
        name: "Madison Beer",
        location: "Cần Thơ",
        profileImage: "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5f/Madison_Beer_2019_by_Glenn_Francis_%28cropped%29.jpg/1200px-Madison_Beer_2019_by_Glenn_Francis_%28cropped%29.jpg",
        hobbies: [
            "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0f/Madison_Beer_%40_Grammy_Museum_01_17_2024_%2853835030223%29_%28cropped%29.jpg/1200px-Madison_Beer_%40_Grammy_Museum_01_17_2024_%2853835030223%29_%28cropped%29.jpg",
            "https://i.pinimg.com/736x/3f/42/60/3f4260d42df039183ae7b58513738eed.jpg",
            "https://i.redd.it/wa94beg6fos61.jpg"
        ],
        details: {
            gender: "Nữ",
            dob: "01/01/1990",
            movieTaste: "Hành động, Lãng mạn"
        }
    },
    {
        name: "Amee",
        location: "Hà Nội",
        profileImage: "https://static-images.vnncdn.net/files/publish/2022/5/27/hain7580-102.jpg",
        hobbies: [
            "https://cdn.tuoitre.vn/thumb_w/480/471584752817336320/2023/7/19/amee-1689779640938664001509.jpeg",
            "https://kenh14cdn.com/203336854389633024/2024/8/4/45374993910178009863832918929137856824468416n-1722747656496585194283-1722750282872-1722750284000757015621.jpeg",
            "https://thantuong.tv/custom/domain_1/2024/08/04/image-105-1722785169.jpg"
        ],
        details: {
            gender: "Nữ",
            dob: "21/06/1995",
            movieTaste: "Drama, Hài hước"
        }
    },
    {
        name: "Bích Phương",
        location: "TP.HCM",
        profileImage: "https://phunuvietnam.mediacdn.vn/179072216278405120/2023/5/21/347601739762812892234696162022642829033544n-16846027874261401835622-1684632390002-1684632390132508713362-1684655004170-16846550055431923728775.jpg",
        hobbies: [
            "https://cdnphoto.dantri.com.vn/yt7vH8_s1KLL-kOvvZAKWZ880_E=/thumb_w/1020/2023/08/21/28555373352968095836752334397678568250905n-1692602094180.jpg",
            "https://cly.1cdn.vn/2022/10/01/bvcl.1cdn.vn-2022-09-30-_s1.media.ngoisao.vn-resize_580-news-2022-09-30-_sinh-nhat-bich-phuong-1-ngoisaovn-w1200-h1600.jpg",
            "https://i1.sndcdn.com/artworks-jfSmtd1wh4UicqLL-i297cw-t500x500.jpg"
        ],
        details: {
            gender: "Nữ",
            dob: "26/06/1993",
            movieTaste: "Kinh dị, Hành động"
        }
    }
];

let currentIndex = 0;

const card = document.querySelector('.card');
const detailsContainer = document.querySelector('.details-container');
const dislikeButton = document.querySelector('.dislike');
const likeButton = document.querySelector('.like');

// Hàm cập nhật thông tin thẻ người dùng
function updateCard() {
    const data = cards[currentIndex];
    card.querySelector('img').src = data.profileImage;
    card.querySelector('.info h2').textContent = data.name;
    card.querySelector('.info p').textContent = data.location;

    detailsContainer.querySelector('h2').textContent = "Thông tin chi tiết";
    detailsContainer.querySelector('p:nth-of-type(1)').innerHTML = `<strong>Tên:</strong> ${data.name}`;
    detailsContainer.querySelector('p:nth-of-type(2)').innerHTML = `<strong>Giới tính:</strong> ${data.details.gender}`;
    detailsContainer.querySelector('p:nth-of-type(3)').innerHTML = `<strong>Ngày sinh:</strong> ${data.details.dob}`;
    detailsContainer.querySelector('p:nth-of-type(4)').innerHTML = `<strong>Gu phim:</strong> ${data.details.movieTaste}`;

    const images = detailsContainer.querySelector('.images');
    images.innerHTML = '';
    data.hobbies.forEach(hobby => {
        const img = document.createElement('img');
        img.src = hobby;
        img.alt = "Hobby";
        images.appendChild(img);
    });
}

// Hàm đặt lại trạng thái thẻ
function resetCard() {
    card.style.transition = 'none';
    detailsContainer.style.transition = 'none';
    card.style.transform = 'translateX(0) rotate(0)';
    card.style.opacity = '1';
    detailsContainer.style.transform = 'translateX(0)';
    detailsContainer.style.opacity = '1';
    setTimeout(() => {
        card.style.transition = 'transform 0.5s ease, opacity 0.5s ease';
        detailsContainer.style.transition = 'transform 0.5s ease, opacity 0.5s ease';
    }, 50);
}

// Xử lý khi nhấn nút không thích
dislikeButton.addEventListener('click', () => {
    card.style.transform = 'translateX(-100vw) rotate(-15deg)';
    card.style.opacity = '0';
    detailsContainer.style.transform = 'translateX(-100vw)';
    detailsContainer.style.opacity = '0';

    setTimeout(() => {
        currentIndex = (currentIndex + 1) % cards.length;
        updateCard();
        resetCard();
    }, 500); // Đợi hiệu ứng kết thúc trước khi cập nhật thẻ mới
});

// Xử lý khi nhấn nút thích
likeButton.addEventListener('click', () => {
    card.style.transform = 'translateX(100vw) rotate(15deg)';
    card.style.opacity = '0';
    detailsContainer.style.transform = 'translateX(100vw)';
    detailsContainer.style.opacity = '0';

    setTimeout(() => {
        currentIndex = (currentIndex + 1) % cards.length;
        updateCard();
        resetCard();
    }, 500); // Đợi hiệu ứng kết thúc trước khi cập nhật thẻ mới
});

// Khởi tạo thẻ đầu tiên khi tải trang
document.addEventListener('DOMContentLoaded', () => {
    updateCard();
});


// Khởi tạo thẻ với người dùng đầu tiên
updateCard();
