using AnFit.Api.DTOs.Requests;
using AnFit.Api.DTOs.Responses;
using AnFit.Api.Exceptions;
using AnFit.Api.Extensions;
using AnFit.Api.Models.Entities;
using AnFit.Api.Models.Enums;
using AnFit.Api.Repositories.Interfaces;
using AnFit.Api.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;

namespace AnFit.Api.Services.Implementations.Core
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenHelper _jwtHelper;
        private readonly IWorkoutPlanService _workoutPlanService;
        private readonly IMealPlanService _mealPlanService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(IUnitOfWork unitOfWork, JwtTokenHelper jwtHelper, IWorkoutPlanService workoutPlanService, IMealPlanService mealPlanService, IEmailService emailService, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _jwtHelper = jwtHelper;
            _workoutPlanService = workoutPlanService;
            _mealPlanService = mealPlanService;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. Kiểm tra Email đã tồn tại chưa
            if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
            {
                throw new ConflictException("Email này đã được sử dụng.");
            }

            // 2. Tạo User mới và Băm (Hash) mật khẩu bằng BCrypt
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = BC.HashPassword(request.Password),
                Role = (int)UserRole.User, // Mặc định là User thường
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            // 3. Trả về Token luôn để user không cần đăng nhập lại sau khi đăng ký
            return await GenerateAuthResponseAsync(newUser);
        }

        public async Task<AuthResponse> RegisterWithOnboardingAsync(RegisterWithOnboardingRequest request)
        {
            // 1. Kiểm tra Email đã tồn tại chưa
            if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
            {
                throw new ConflictException("Email này đã được sử dụng.");
            }

            // 1.5 Kiểm tra mã OTP (nếu có truyền và khác mã bypass 999999 thì kiểm tra)
            if (!string.IsNullOrWhiteSpace(request.OtpCode) && request.OtpCode != "999999")
            {
                string cacheKey = $"OTP_Register_{request.Email.ToLower()}";
                if (!_cache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != request.OtpCode)
                {
                    throw new BadRequestException("Mã OTP không hợp lệ hoặc đã hết hạn.");
                }
                _cache.Remove(cacheKey);
            }

            // 2. Bọc toàn bộ logic trong Transaction (ACID)
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 3. Tạo User với đầy đủ trường Onboarding
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = BC.HashPassword(request.Password),
                    Role = (int)UserRole.User,
                    Gender = request.Gender,
                    HeightCm = request.HeightCm,
                    DateOfBirth = request.DateOfBirth,
                    TargetGoal = request.TargetGoal,
                    DailyBudget = request.DailyBudget,
                    Occupation = request.Occupation,
                    CurrentBodyShape = request.CurrentBodyShape,
                    TargetBodyShape = request.TargetBodyShape,
                    FitnessLevel = request.FitnessLevel,
                    PreferredIntensity = request.PreferredIntensity,
                    PsychologicalBarrier = request.PsychologicalBarrier,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                // 4. Ghi log cân nặng đầu tiên vào UserBodyLogs (nếu có)
                if (request.Weight.HasValue && request.Weight > 0)
                {
                    double? bmi = null;
                    if (request.HeightCm.HasValue && request.HeightCm > 0)
                    {
                        double heightM = request.HeightCm.Value / 100.0;
                        bmi = Math.Round(request.Weight.Value / (heightM * heightM), 1);
                    }

                    var bodyLog = new UserBodyLog
                    {
                        UserId = newUser.Id,
                        Weight = request.Weight.Value,
                        Bmi = bmi,
                        RecordedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.UserBodyLogs.AddAsync(bodyLog);
                    await _unitOfWork.SaveChangesAsync();
                }

                // 5. Gắn danh sách bệnh lý (Many-to-Many)
                if (request.MedicalConditionIds != null && request.MedicalConditionIds.Count > 0)
                {
                    foreach (var conditionId in request.MedicalConditionIds)
                    {
                        var condition = await _unitOfWork.MedicalConditions.GetByIdAsync(conditionId);
                        if (condition != null)
                        {
                            newUser.Conditions.Add(condition);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // 6. Gắn danh sách hoạt động ưa thích (Many-to-Many)
                if (request.PreferredActivityIds != null && request.PreferredActivityIds.Count > 0)
                {
                    foreach (var activityId in request.PreferredActivityIds)
                    {
                        var activity = await _unitOfWork.ActivityCategories.GetByIdAsync(activityId);
                        if (activity != null)
                        {
                            newUser.PreferredActivities.Add(activity);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                // Tự động sinh Lộ trình 30 ngày tập luyện cá nhân hóa (Active Recovery Split)
                await _workoutPlanService.Generate30DayRoadmapAsync(newUser.Id);

                // Tự động sinh Lộ trình Dinh dưỡng 30 ngày cá nhân hóa
                await _mealPlanService.Generate30DayMealRoadmapAsync(newUser.Id);

                // 7. Commit toàn bộ Transaction
                await _unitOfWork.CommitTransactionAsync();

                // 8. Trả về Token
                return await GenerateAuthResponseAsync(newUser);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // 1. Tìm User theo Email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Email hoặc mật khẩu không chính xác.");
            }

            // 1.5. Kiểm tra Lockout
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                throw new UnauthorizedException($"Tài khoản của bạn tạm thời bị khóa do nhập sai mật khẩu quá nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút.");
            }

            // 2. Kiểm tra mật khẩu (So sánh chuỗi người dùng nhập với mã Hash trong DB)
            bool isPasswordValid = BC.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                // Tăng số lần nhập sai
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // Khóa 15 phút
                    user.AccessFailedCount = 0; // Reset count
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                    throw new UnauthorizedException("Tài khoản của bạn đã bị khóa tạm thời 15 phút do nhập sai mật khẩu 5 lần liên tiếp.");
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                int remainingAttempts = 5 - user.AccessFailedCount;
                throw new UnauthorizedException($"Email hoặc mật khẩu không chính xác. Bạn còn {remainingAttempts} lần thử.");
            }

            // 3. Đăng nhập thành công -> Reset Lockout & AccessFailedCount
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // 4. Tạo và trả về Token
            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // 1. Tìm RefreshToken trong DB
            var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

            if (existingToken == null || existingToken.ExpiresAt <= DateTime.UtcNow || existingToken.IsRevoked)
            {
                throw new UnauthorizedException("Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.");
            }

            // 2. Lấy thông tin User
            var user = await _unitOfWork.Users.GetByIdAsync(existingToken.UserId ?? Guid.Empty);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // 3. Thu hồi (Revoke) Token cũ ngay lập tức để chống việc dùng lại 1 token nhiều lần (Rotation)
            existingToken.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(existingToken);

            // 4. Tạo cặp Token mới
            return await GenerateAuthResponseAsync(user);
        }

        public async Task RevokeTokenAsync(string token)
        {
            var existingToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
            if (existingToken != null && !existingToken.IsRevoked)
            {
                existingToken.IsRevoked = true;
                _unitOfWork.RefreshTokens.Update(existingToken);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        // --- HÀM PHỤ TRỢ (Private) ---
        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            // Tạo Access Token và Refresh Token
            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshTokenString = _jwtHelper.GenerateRefreshToken();

            // Lưu Refresh Token vào Database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Cứng 7 ngày hoặc lấy từ appsettings đều được
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = refreshTokenEntity.ExpiresAt.Value
            };
        }
        public async Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Để bảo mật, không ném lỗi NotFound mà chỉ return (để hacker không dò được email nào tồn tại)
                return null;
            }

            // Tạo mã OTP hoặc Token ngẫu nhiên (Ở đây dùng mã HEX đơn giản 6 ký tự hoặc Guid)
            string resetToken = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); // VD: "A1B2C3"

            user.PasswordResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(5); // Hết hạn sau 5 phút

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var emailBody = $"<h3>Yêu cầu khôi phục mật khẩu</h3><p>Mã OTP của bạn là: <b>{resetToken}</b>. Có hiệu lực trong 5 phút.</p>";
            await _emailService.SendEmailAsync(user.Email, "AnFit - Khôi phục mật khẩu", emailBody);

            return resetToken;
        }

        public async Task SendOtpRegisterAsync(string email)
        {
            if (await _unitOfWork.Users.IsEmailExistsAsync(email))
            {
                throw new ConflictException("Email này đã được sử dụng.");
            }

            // Sinh mã OTP 6 số
            string otp = new Random().Next(100000, 999999).ToString();

            // Lưu vào cache 5 phút
            string cacheKey = $"OTP_Register_{email.ToLower()}";
            _cache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

            // Gửi email
            var emailBody = $@"
                <h3>Chào mừng bạn đến với AnFit!</h3>
                <p>Mã xác nhận đăng ký của bạn là: <b style='font-size:20px;color:#0055A4;'>{otp}</b></p>
                <p>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
            ";
            await _emailService.SendEmailAsync(email, "AnFit - Mã xác nhận đăng ký", emailBody);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Cần tìm User có chứa Token này và chưa hết hạn
            var users = await _unitOfWork.Users.FindAsync(u =>
                u.PasswordResetToken == request.Token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            var user = users.FirstOrDefault();

            if (user == null)
            {
                throw new BadRequestException("Mã xác nhận không hợp lệ hoặc đã hết hạn.");
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BC.HashPassword(request.NewPassword);

            // Xóa mã Token đi để không dùng lại được nữa
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Kiểm tra mật khẩu cũ
            if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
            {
                throw new BadRequestException("Mật khẩu hiện tại không chính xác.");
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BC.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) throw new NotFoundException("Không tìm thấy người dùng.");

            // Soft Delete: Không xóa thật khỏi DB mà chỉ đánh dấu DeletedAt
            user.DeletedAt = DateTime.UtcNow;

            // Có thể đổi Email thành dạng khác để giải phóng Email này cho lần đăng ký sau nếu cần
            // user.Email = $"deleted_{Guid.NewGuid()}@{user.Email}"; 

            _unitOfWork.Users.Update(user);

            // Thu hồi toàn bộ Refresh Token của User này (đăng xuất khỏi mọi thiết bị)
            var activeTokens = await _unitOfWork.RefreshTokens.FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                _unitOfWork.RefreshTokens.Update(token);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}