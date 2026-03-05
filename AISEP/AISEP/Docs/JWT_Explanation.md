# JWT Token - Giải thích chi tiết

---

## 1. Token là gì?

Token giống như một **thẻ ra vào tòa nhà**:

```
Bảo vệ (Server) cấp thẻ cho nhân viên (User) sau khi login
Mỗi lần vào cửa → quẹt thẻ → bảo vệ kiểm tra → cho vào
Không cần hỏi tên/mật khẩu mỗi lần nữa
```

Trong hệ thống AISEP, có **2 loại token**:

| Token | Giống với | Thời hạn |
|-------|-----------|----------|
| **AccessToken** | Thẻ vào cửa hàng ngày | 15 phút |
| **RefreshToken** | Giấy ủy quyền làm thẻ mới | 7 ngày |

---

## 2. Flow hoạt động thực tế

### Bước 1 — Đăng nhập
```
User gửi email + password
    ↓
Server kiểm tra đúng → Tạo 2 token
    ↓
Trả về:
  - AccessToken  (hết hạn sau 15 phút)
  - RefreshToken (hết hạn sau 7 ngày, lưu vào DB)
```

### Bước 2 — Dùng API bình thường
```
User gọi API (VD: xem danh sách deals)
    ↓
Gửi kèm AccessToken trong header:
    Authorization: Bearer eyJhbGci...
    ↓
Server chỉ verify chữ ký → Không query DB → Nhanh ✅
    ↓
Trả về dữ liệu
```

### Bước 3 — AccessToken hết hạn (sau 15 phút)
```
User gọi API → Server báo 401 Unauthorized
    ↓
App tự động gửi RefreshToken lên server
    ↓
Server kiểm tra DB:
  - RefreshToken có tồn tại không?
  - IsRevoked = false không?
  - Chưa hết hạn không?
    ↓
Nếu hợp lệ → Tạo AccessToken mới + RefreshToken mới
             → Revoke RefreshToken cũ (IsRevoked = true)
    ↓
User tiếp tục dùng app mà không cần login lại ✅
```

### Bước 4 — RefreshToken hết hạn (sau 7 ngày)
```
Server báo RefreshToken hết hạn
    ↓
App chuyển user về màn hình Login
    ↓
User đăng nhập lại
```

---

## 3. "Đánh cắp token" là gì?

### Tình huống thực tế:

```
Hacker ngồi cùng mạng WiFi quán cafe với User
    ↓
Hacker dùng công cụ bắt gói tin (Wireshark)
    ↓
Hacker lấy được AccessToken của User từ request HTTP
    ↓
Hacker dùng AccessToken đó để gọi API như User thật
    ↓
Hacker xem được deals, documents của User!
```

### Các cách token bị đánh cắp:

| Cách | Mô tả |
|------|-------|
| **Sniff mạng** | Bắt gói tin trên mạng không bảo mật (HTTP, WiFi công cộng) |
| **XSS Attack** | Hacker chèn JavaScript độc vào web → đọc token từ localStorage |
| **MITM Attack** | Hacker đứng giữa client và server, đọc traffic |
| **Lộ log** | Token vô tình bị ghi vào file log của server |
| **Thiết bị bị mất** | Người khác lấy điện thoại/máy tính có lưu token |

---

## 4. Tại sao AccessToken ngắn (15 phút) giúp bảo vệ?

### Kịch bản không có thời hạn ngắn:
```
❌ AccessToken sống 7 ngày

Hacker lấy được AccessToken lúc 8:00 sáng
    ↓
Admin phát hiện → Ban user trong DB lúc 8:05
    ↓
Nhưng AccessToken vẫn HỢP LỆ!
    ↓
Hacker vẫn truy cập được đến 8:00 sáng ngày thứ 8
→ Thiệt hại: 7 ngày!
```

### Kịch bản với AccessToken 15 phút:
```
✅ AccessToken sống 15 phút

Hacker lấy được AccessToken lúc 8:00 sáng
    ↓
Admin phát hiện → Ban user trong DB lúc 8:05
    ↓
Lúc 8:15 → AccessToken hết hạn
    ↓
Hacker cần RefreshToken để lấy AccessToken mới
    ↓
Server check DB → RefreshToken đã bị revoke → Từ chối!
→ Thiệt hại tối đa: 15 phút ✅
```

---

## 5. Token Rotation — Phát hiện token bị đánh cắp

Hệ thống AISEP dùng **Token Rotation**: mỗi lần dùng RefreshToken → tạo cái mới, vô hiệu hóa cái cũ.

```
RefreshToken A (hợp lệ)
    ↓
User dùng → Server tạo RefreshToken B mới
          → RefreshToken A: IsRevoked = true, ReplacedByToken = B
    ↓
Bây giờ chỉ RefreshToken B mới hợp lệ
```

### Phát hiện khi bị đánh cắp:

```
User đang dùng RefreshToken B (token mới)
    ↓
Hacker (đã lấy RefreshToken A trước đó) cố dùng A
    ↓
Server phát hiện: A đã bị revoke!
    ↓
Server biết: "Có kẻ đang dùng token cũ → Tài khoản bị xâm phạm"
    ↓
Server revoke TẤT CẢ token của user đó
    ↓
User bị logout → Phải đăng nhập lại → Đổi mật khẩu
```

---

## 6. Tại sao RefreshToken cần lưu DB?

```
RefreshToken KHÔNG lưu DB:
  - Server không biết token nào đang tồn tại
  - Không thể revoke
  - Không thể logout từ tất cả thiết bị
  - Ban user → vô tác dụng cho đến khi token hết hạn

RefreshToken LƯU DB (AISEP đang dùng):
  - Server biết chính xác token nào hợp lệ
  - Revoke ngay lập tức khi cần ✅
  - Logout tất cả thiết bị ✅
  - Phát hiện token bị đánh cắp ✅
```

---

## 7. Tóm tắt bảo mật hệ thống AISEP

```
┌─────────────────────────────────────────────────────┐
│                    AISEP Security                   │
├─────────────────┬───────────────────────────────────┤
│ AccessToken     │ 15 phút, stateless, không lưu DB  │
│                 │ → Nhanh, nhưng không revoke được  │
├─────────────────┼───────────────────────────────────┤
│ RefreshToken    │ 7 ngày, lưu DB, có thể revoke     │
│                 │ → Kiểm soát toàn bộ session       │
├─────────────────┼───────────────────────────────────┤
│ Token Rotation  │ Mỗi lần refresh → đổi token mới  │
│                 │ → Phát hiện token bị đánh cắp     │
├─────────────────┼───────────────────────────────────┤
│ HTTPS           │ Mã hóa traffic → khó bắt gói tin  │
└─────────────────┴───────────────────────────────────┘
```

### Kết luận:
- **AccessToken ngắn** → Giới hạn thiệt hại nếu bị đánh cắp
- **RefreshToken lưu DB** → Kiểm soát và thu hồi khi cần
- **Token Rotation** → Phát hiện tài khoản bị xâm phạm
- **Ba cơ chế kết hợp** → Bảo mật phù hợp cho hệ thống tài chính/đầu tư như AISEP
